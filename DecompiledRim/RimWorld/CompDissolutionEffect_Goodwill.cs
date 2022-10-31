using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompDissolutionEffect_Goodwill : CompDissolutionEffect
	{
		private struct GoodwillPollutionEvent
		{
			public int tile;

			public int amount;
		}

		private static readonly SimpleCurve GoodwillFactorOverDistanceCurve = new SimpleCurve
		{
			new CurvePoint(0f, 2f),
			new CurvePoint(1f, 1f),
			new CurvePoint(4f, 0.5f),
			new CurvePoint(8f, 0.1f),
			new CurvePoint(16f, 0.05f),
			new CurvePoint(16.0001f, 0f)
		};

		private const float MaxGoodwillChangeDistance = 16f;

		private const int BaseGoodwillFactor = -1;

		private const int MinGoodwillChange = 1;

		private static List<GoodwillPollutionEvent> pendingGoodwillEvents = new List<GoodwillPollutionEvent>();

		private static List<Settlement> tmpFactionSettlements = new List<Settlement>();

		public override void DoDissolutionEffectWorld(int amount, int tileId)
		{
			GoodwillPollutionEvent goodwillPollutionEvent = default(GoodwillPollutionEvent);
			goodwillPollutionEvent.tile = tileId;
			goodwillPollutionEvent.amount = amount;
			GoodwillPollutionEvent item = goodwillPollutionEvent;
			pendingGoodwillEvents.Add(item);
		}

		public static void WorldUpdate()
		{
			if (pendingGoodwillEvents.Count <= 0)
			{
				return;
			}
			foreach (IGrouping<int, GoodwillPollutionEvent> item in from g in pendingGoodwillEvents
				group g by g.tile)
			{
				int tile = item.Key;
				List<Settlement> settlementBases = Find.WorldObjects.SettlementBases;
				foreach (Faction faction in Find.FactionManager.AllFactionsVisible)
				{
					if (faction.IsPlayer || faction.HostileTo(Faction.OfPlayer))
					{
						continue;
					}
					tmpFactionSettlements.Clear();
					tmpFactionSettlements.AddRange(settlementBases.Where((Settlement s) => s.Faction == faction));
					tmpFactionSettlements.SortBy((Settlement s) => Find.WorldGrid.ApproxDistanceInTiles(s.Tile, tile));
					if (tmpFactionSettlements.Count > 0)
					{
						Settlement settlement = tmpFactionSettlements[0];
						float num = Find.WorldGrid.ApproxDistanceInTiles(settlement.Tile, tile);
						if (num > 16f)
						{
							continue;
						}
						int num2 = item.Sum((GoodwillPollutionEvent p) => p.amount);
						int goodwillChange = Mathf.RoundToInt(Mathf.Max(GoodwillFactorOverDistanceCurve.Evaluate(Mathf.RoundToInt(num)) * (float)num2, 1f)) * -1;
						Faction.OfPlayer.TryAffectGoodwillWith(settlement.Faction, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, (settlement.Tile == tile) ? HistoryEventDefOf.PollutedBase : HistoryEventDefOf.PollutedNearbySite, settlement);
					}
					tmpFactionSettlements.Clear();
				}
			}
			pendingGoodwillEvents.Clear();
		}
	}
}

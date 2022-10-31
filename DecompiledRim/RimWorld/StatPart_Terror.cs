using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatPart_Terror : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			Pawn thing;
			if ((thing = req.Thing as Pawn) != null)
			{
				val += TerrorUtility.SuppressionFallRateOverTerror.Evaluate(thing.GetStatValue(StatDefOf.Terror));
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			Pawn thing;
			if (req.HasThing && (thing = req.Thing as Pawn) != null && !Mathf.Approximately(TerrorUtility.SuppressionFallRateOverTerror.Evaluate(thing.GetStatValue(StatDefOf.Terror)), 0f))
			{
				return "StatsReport_Terror".Translate() + (": " + TerrorUtility.SuppressionFallRateOverTerror.Evaluate(thing.GetStatValue(StatDefOf.Terror)).ToStringPercent());
			}
			return null;
		}
	}
}

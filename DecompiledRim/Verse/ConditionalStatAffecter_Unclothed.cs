using RimWorld;

namespace Verse
{
	public class ConditionalStatAffecter_Unclothed : ConditionalStatAffecter
	{
		public override string Label => "StatsReport_Unclothed".Translate();

		public override bool Applies(StatRequest req)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			Pawn pawn;
			if (req.HasThing && (pawn = req.Thing as Pawn) != null && pawn.apparel != null)
			{
				foreach (Apparel item in pawn.apparel.WornApparel)
				{
					if (item.def.apparel.countsAsClothingForNudity)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}

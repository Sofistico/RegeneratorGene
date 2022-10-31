using RimWorld;

namespace Verse
{
	public class ConditionalStatAffecter_Clothed : ConditionalStatAffecter
	{
		public override string Label => "StatsReport_Clothed".Translate();

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
						return true;
					}
				}
			}
			return false;
		}
	}
}

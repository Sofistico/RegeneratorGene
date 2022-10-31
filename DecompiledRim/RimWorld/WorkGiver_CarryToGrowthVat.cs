using Verse;

namespace RimWorld
{
	public class WorkGiver_CarryToGrowthVat : WorkGiver_CarryToBuilding
	{
		public override ThingRequest ThingRequest => ThingRequest.ForDef(ThingDefOf.GrowthVat);

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !ModsConfig.BiotechActive;
		}
	}
}

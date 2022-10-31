using Verse;

namespace RimWorld
{
	public class WorkGiver_CarryToGeneExtractor : WorkGiver_CarryToBuilding
	{
		public override ThingRequest ThingRequest => ThingRequest.ForDef(ThingDefOf.GeneExtractor);

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !ModsConfig.BiotechActive;
		}
	}
}

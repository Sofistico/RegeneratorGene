using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeAsColonistOrColonyAnimal : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			if (pawn.Faction == Faction.OfPlayer && !pawn.HasExtraHomeFaction())
			{
				return !pawn.IsSlave;
			}
			return false;
		}
	}
}

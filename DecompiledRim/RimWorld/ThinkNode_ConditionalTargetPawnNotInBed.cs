using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalTargetPawnNotInBed : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			Pawn pawn2 = pawn.mindState.duty.focusSecond.Pawn;
			Building_Bed building_Bed;
			if ((building_Bed = pawn.mindState.duty.focusThird.Thing as Building_Bed) != null)
			{
				return pawn2.CurrentBed() != building_Bed;
			}
			return !pawn2.InBed();
		}
	}
}

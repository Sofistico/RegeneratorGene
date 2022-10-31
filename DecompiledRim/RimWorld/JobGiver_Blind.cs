using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_Blind : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			LordJob_Ritual_Mutilation lordJob_Ritual_Mutilation;
			if (lord == null || (lordJob_Ritual_Mutilation = lord.LordJob as LordJob_Ritual_Mutilation) == null)
			{
				return null;
			}
			Pawn pawn2 = pawn.mindState.duty.focusSecond.Pawn;
			if (lordJob_Ritual_Mutilation.mutilatedPawns.Contains(pawn2) || !pawn.CanReserveAndReach(pawn2, PathEndMode.ClosestTouch, Danger.None))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Blind, pawn2, pawn.mindState.duty.focus);
		}
	}
}

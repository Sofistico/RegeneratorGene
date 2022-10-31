namespace Verse.AI
{
	public class JobGiver_WanderNearRoamingExit : JobGiver_Wander
	{
		public JobGiver_WanderNearRoamingExit()
		{
			wanderRadius = 12f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			MentalState_Roaming mentalState_Roaming = pawn.MentalState as MentalState_Roaming;
			if (mentalState_Roaming == null)
			{
				return null;
			}
			if (mentalState_Roaming.ShouldExitMapNow())
			{
				return null;
			}
			return base.TryGiveJob(pawn);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return (pawn.MentalState as MentalState_Roaming)?.exitDest ?? pawn.Position;
		}
	}
}

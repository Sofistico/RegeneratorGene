using Verse;

namespace RimWorld
{
	public class RaidAgeRestrictionWorker_Children : RaidAgeRestrictionWorker
	{
		public override bool CanUseWith(IncidentParms parms)
		{
			if (!Find.Storyteller.difficulty.ChildrenAllowed || Find.Storyteller.difficulty.babiesAreHealthy || !Find.Storyteller.difficulty.childRaidersAllowed)
			{
				return false;
			}
			return base.CanUseWith(parms);
		}
	}
}

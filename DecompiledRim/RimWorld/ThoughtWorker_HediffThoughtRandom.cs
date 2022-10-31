using Verse;

namespace RimWorld
{
	public class ThoughtWorker_HediffThoughtRandom : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			foreach (Hediff hediff in p.health.hediffSet.hediffs)
			{
				HediffWithComps hediffWithComps;
				if ((hediffWithComps = hediff as HediffWithComps) == null)
				{
					continue;
				}
				foreach (HediffComp comp in hediffWithComps.comps)
				{
					HediffComp_GiveRandomSituationalThought hediffComp_GiveRandomSituationalThought;
					if ((hediffComp_GiveRandomSituationalThought = comp as HediffComp_GiveRandomSituationalThought) != null && hediffComp_GiveRandomSituationalThought.selectedThought == def)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}

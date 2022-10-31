using Verse;

namespace RimWorld
{
	public class RitualObligationTrigger_MemberCorpseDestroyed : RitualObligationTrigger
	{
		public override void Notify_MemberCorpseDestroyed(Pawn p)
		{
			if (Current.ProgramState == ProgramState.Playing && (!mustBePlayerIdeo || Faction.OfPlayer.ideos.Has(ritual.ideo)) && p.HomeFaction == Faction.OfPlayer && p.IsFreeNonSlaveColonist && !p.IsKidnapped())
			{
				ritual.AddObligation(new RitualObligation(ritual, p));
			}
		}
	}
}

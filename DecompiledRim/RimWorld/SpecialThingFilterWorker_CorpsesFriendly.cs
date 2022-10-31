using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_CorpsesFriendly : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			Corpse corpse;
			if ((corpse = t as Corpse) == null)
			{
				return false;
			}
			return corpse.InnerPawn.Faction == Faction.OfPlayer;
		}
	}
}

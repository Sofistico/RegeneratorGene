using RimWorld;

namespace Verse.AI
{
	public class MentalState_CocoonDisturbed : MentalState
	{
		public override bool ForceHostileTo(Thing t)
		{
			Pawn pawn;
			if ((pawn = t as Pawn) != null)
			{
				if (pawn.RaceProps.Insect)
				{
					return false;
				}
				if (pawn.RaceProps.Animal && pawn.RaceProps.Roamer)
				{
					return false;
				}
			}
			return true;
		}

		public override bool ForceHostileTo(Faction f)
		{
			return true;
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}

using RimWorld;

namespace Verse.AI
{
	public class MentalState_BabyGiggle : MentalState_BabyFit
	{
		protected override void AuraEffect(Thing source, Pawn hearer)
		{
			Pawn otherPawn;
			if ((otherPawn = source as Pawn) != null && hearer.needs.mood != null)
			{
				if (hearer == otherPawn.GetMother() || hearer == otherPawn.GetFather())
				{
					hearer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MyGigglingBaby, otherPawn);
				}
				else
				{
					hearer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.GigglingBaby, otherPawn);
				}
				hearer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BabyGiggledSocial, otherPawn);
			}
		}
	}
}

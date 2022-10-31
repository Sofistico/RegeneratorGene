using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_BrainDamageChance : CompAbilityEffect
	{
		public new CompProperties_AbilityBrainDamageChance Props => (CompProperties_AbilityBrainDamageChance)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			Pawn pawn;
			if ((pawn = target.Thing as Pawn) != null && !pawn.Dead && Rand.Value <= Props.brainDamageChance)
			{
				BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
				if (brain != null)
				{
					int num = Rand.RangeInclusive(1, 5);
					pawn.TakeDamage(new DamageInfo(DamageDefOf.Flame, num, 0f, -1f, parent.pawn, brain));
				}
			}
		}
	}
}

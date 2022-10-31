using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_HemogenCost : CompAbilityEffect
	{
		public new CompProperties_AbilityHemogenCost Props => (CompProperties_AbilityHemogenCost)props;

		private bool HasEnoughHemogen
		{
			get
			{
				if ((parent.pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>()).Value < Props.hemogenCost)
				{
					return false;
				}
				return true;
			}
		}

		public override bool CanCast
		{
			get
			{
				if (!HasEnoughHemogen)
				{
					return false;
				}
				return base.CanCast;
			}
		}

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			GeneUtility.OffsetHemogen(parent.pawn, 0f - Props.hemogenCost);
		}

		public override bool GizmoDisabled(out string reason)
		{
			Gene_Hemogen gene_Hemogen = parent.pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
			if (gene_Hemogen == null)
			{
				reason = "AbilityDisabledNoHemogenGene".Translate(parent.pawn);
				return true;
			}
			if (gene_Hemogen.Value < Props.hemogenCost)
			{
				reason = "AbilityDisabledNoHemogen".Translate(parent.pawn);
				return true;
			}
			float num = TotalHemogenCostOfQueuedAbilities();
			float num2 = Props.hemogenCost + num;
			if (Props.hemogenCost > float.Epsilon && num2 > gene_Hemogen.Value)
			{
				reason = "AbilityDisabledNoHemogen".Translate(parent.pawn);
				return true;
			}
			reason = null;
			return false;
		}

		public override bool AICanTargetNow(LocalTargetInfo target)
		{
			return HasEnoughHemogen;
		}

		private float TotalHemogenCostOfQueuedAbilities()
		{
			Verb_CastAbility verb_CastAbility = parent.pawn.jobs?.curJob?.verbToUse as Verb_CastAbility;
			float num = ((verb_CastAbility == null) ? 0f : (verb_CastAbility.ability?.HemogenCost() ?? 0f));
			if (parent.pawn.jobs != null)
			{
				for (int i = 0; i < parent.pawn.jobs.jobQueue.Count; i++)
				{
					Verb_CastAbility verb_CastAbility2;
					if ((verb_CastAbility2 = parent.pawn.jobs.jobQueue[i].job.verbToUse as Verb_CastAbility) != null)
					{
						num += verb_CastAbility2.ability?.HemogenCost() ?? 0f;
					}
				}
			}
			return num;
		}
	}
}

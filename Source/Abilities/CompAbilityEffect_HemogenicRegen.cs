using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RegeneratorGene
{
    public class CompAbilityEffect_HemogenicRegen : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            if (pawn == null)
            {
                return;
            }
            int num = 0;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int num2 = hediffs.Count - 1; num2 >= 0; num2--)
            {
                if (hediffs[num2] is Hediff_Injury && hediffs[num2].TendableNow())
                {
                    hediffs[num2].Heal(hediffs[num2].Severity);
                    num++;
                }
            }
            if (num > 0)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "NumWoundsTended".Translate(num), 3.65f);
            }

            FleckMaker.AttachedOverlay(pawn, FleckDefOf.FlashHollow, Vector3.zero, 1.5f);

            // shouldn't see anything!
            RegeneratorUtilities.TryRegenAllLimbs(pawn);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = parent.pawn;
            if (pawn != null)
            {
                AbilityUtility.ValidateHasTendableWound(pawn, throwMessages, parent);
            }
            return base.Valid(target, throwMessages);
        }
    }
}

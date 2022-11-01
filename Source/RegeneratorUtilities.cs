using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RegeneratorGene
{
    public static class RegeneratorUtilities
    {
        public static bool TryRegenLimbOnce(Pawn pawn, HediffDef hediffToAdd)
        {
            // will only regenerate one limb per day!
            bool healedOnce = false;

            List<BodyPartRecord> missingBP = GetAllLostBp(pawn);

            if (missingBP.Any())
            {
                Hediff_MissingPart removedMissingPartHediff =
                    GetAnyRemovedMissingPartAfterRegen(pawn, missingBP).FirstOrDefault();
                if (removedMissingPartHediff != null)
                {
                    healedOnce = HealLimb(pawn, removedMissingPartHediff, hediffToAdd);
                }
            }

            return healedOnce;
        }

        public static bool TryRegenAllLimbsHemogenRecovery(Pawn pawn, HediffDef hediffToAdd)
        {
            // will only regenerate one limb per day!
            bool healedOnce = false;

            List<BodyPartRecord> missingBP = GetAllLostBp(pawn);

            if (missingBP.Any())
            {
                List<Hediff_MissingPart> removedMissingPartHediff =
                    GetAnyRemovedMissingPartAfterRegen(pawn, missingBP);
                if (removedMissingPartHediff.Count > 0)
                {
                    foreach (var part in removedMissingPartHediff)
                    {
                        healedOnce = HealLimb(pawn, part, Regen_DefOf.Sofis_Hemogen_Regenerating, true);
                    }
                }
            }

            return healedOnce;
        }

        private static List<BodyPartRecord> GetAllLostBp(Pawn pawn)
        {
            var noMissingBP = pawn.health.hediffSet.GetNotMissingParts().ToList();
            var missingBP = pawn.def.race.body.AllParts.Where(i =>
                pawn.health.hediffSet.PartIsMissing(i)
                && noMissingBP.Contains(i.parent)
                && !pawn.health.hediffSet.AncestorHasDirectlyAddedParts(i)).ToList();
            return missingBP;
        }

        private static bool HealLimb(Pawn pawn, Hediff_MissingPart removedMissingPartHediff,
            HediffDef hediffToAdd,
            bool instaHeal = false)
        {
            bool healedOnce;
            Hediff regeneratingHediff = HediffMaker.MakeHediff(hediffToAdd,
                                    pawn,
                                    removedMissingPartHediff.Part);
            if(!instaHeal)
                regeneratingHediff.Severity = removedMissingPartHediff.Part.def.GetMaxHealth(pawn) - 1;
            else
                regeneratingHediff.Severity = removedMissingPartHediff.Part.def.GetMaxHealth(pawn);

            pawn.health.AddHediff(regeneratingHediff);
            healedOnce = true;
            return healedOnce;
        }

        /// <summary>
        /// Helper method to regen a missing limb and add the regenerating hediff
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="missingBP"></param>
        /// <returns></returns>
        private static List<Hediff_MissingPart> GetAnyRemovedMissingPartAfterRegen(Pawn pawn, List<BodyPartRecord> missingBP)
        {
            var missingPart = missingBP.RandomElement();
            var currentMissingHediffs = GetMissingsHediffs(pawn);
            pawn.health.RestorePart(missingPart);
            var currentMissingHediffs2 = GetMissingsHediffs(pawn);
            var removedMissingPartHediff = currentMissingHediffs.Where(i
                => !currentMissingHediffs2.Contains(i)).ToList();
            return removedMissingPartHediff;
        }

        private static List<Hediff_MissingPart> GetMissingsHediffs(Pawn pawn)
        {
            var missingHediffs = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList();
            return missingHediffs;
        }
    }
}
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

        public static void NaturalRegenerationOfLimbs(Pawn pawn, HediffDef hediffToAdd)
        {
            var toRemove = new List<Hediff>();
            var toAdd = new List<Hediff>();
            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_MissingPart && !pawn.health.hediffSet.PartIsMissing(hediff.Part.parent) &&
                    !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(hediff.Part))
                {
                    var part = hediff.Part;
                    var flag = true;
                    while (part != null)
                        if (pawn.health.hediffSet.hediffs.Any(hd => hd.Part == part && hd.def == hediffToAdd))
                        {
                            flag = false;
                            part = null;
                        }
                        else
                        {
                            part = part.parent;
                        }

                    if (flag)
                    {
                        var newHediff = HediffMaker.MakeHediff(hediffToAdd, pawn, hediff.Part);
                        newHediff.Severity = 0.01f;
                        toAdd.Add(newHediff);
                        toRemove.Add(hediff);
                    }
                }

                if (hediff.def == hediffToAdd)
                {
                    hediff.Severity += 1f / 180000f;
                    if (hediff.Severity >= 1f) toRemove.Add(hediff);
                }
            }

            foreach (var hediff in toRemove) pawn.health.RemoveHediff(hediff);

            foreach (var hediff in toAdd) pawn.health.AddHediff(hediff);
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
            if (!instaHeal)
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
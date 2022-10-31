using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RegeneratorGene
{
    public class Gene_Regenerator : Gene
    {
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
            {
                // will only regenerate one limb per day!
                bool healedOnce = false;

                var noMissingBP = pawn.health.hediffSet.GetNotMissingParts().ToList();
                var missingBP = pawn.def.race.body.AllParts.Where(i =>
                    pawn.health.hediffSet.PartIsMissing(i)
                    && noMissingBP.Contains(i.parent)
                    && !pawn.health.hediffSet.AncestorHasDirectlyAddedParts(i)).ToList();

                if (missingBP.Any())
                {
                    var missingPart = missingBP.RandomElement();
                    var currentMissingHediffs = GetMissingsBps();
                    pawn.health.RestorePart(missingPart);
                    var currentMissingHediffs2 = GetMissingsBps();
                    var removedMissingPartHediff = currentMissingHediffs.Where(i
                        => !currentMissingHediffs2.Contains(i)).FirstOrDefault();
                    if (removedMissingPartHediff != null)
                    {
                        var regeneratingHediff = HediffMaker.MakeHediff(Regen_DefOf.Sofis_Regenerating,
                            pawn,
                            removedMissingPartHediff.Part);
                        regeneratingHediff.Severity = removedMissingPartHediff.Part.def.GetMaxHealth(pawn) - 1;
                        pawn.health.AddHediff(regeneratingHediff);
                        healedOnce = true;
                    }
                }
                if (healedOnce)
                {
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
                }
            }
        }

        private List<Hediff_MissingPart> GetMissingsBps()
        {
            var missingHediffs = pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList();
            return missingHediffs;
        }
    }
}
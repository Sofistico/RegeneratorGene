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
            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                RegeneratorUtilities.NaturalRegenerationOfLimbs(pawn, Regen_DefOf.Sofis_Regenerating);
            }
        }
    }
}
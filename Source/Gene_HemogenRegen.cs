using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RegeneratorGene
{
    public class Gene_HemogenRegen : Gene
    {
        public Gene_HemogenRegen()
        {
            // empty constructor
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % (GenDate.TicksPerHour * 0.5) == 0) // the vampire regens lost limbs in half an hour!
            {
                RegeneratorUtilities.NaturalRegenerationOfLimbs(pawn, Regen_DefOf.Sofis_Regenerating);
            }
        }
    }
}

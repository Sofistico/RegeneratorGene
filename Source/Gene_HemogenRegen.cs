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
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0) // the vampire regens lost limbs in an hour!
            {
                bool healedOnce = RegeneratorUtilities.TryRegenLimbOnce(pawn, Regen_DefOf.Sofis_Hemogen_Regenerating);
                if (healedOnce)
                {
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
                }
            }
        }
    }
}

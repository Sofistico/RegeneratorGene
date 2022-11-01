﻿using RimWorld;
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
                bool healedOnce = RegeneratorUtilities.TryRegenLimbOnce(pawn, Regen_DefOf.Sofis_Regenerating);
                if (healedOnce)
                {
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
                }
            }
        }
    }
}
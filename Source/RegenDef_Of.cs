﻿using RimWorld;
using Verse;

namespace RegeneratorGene
{
    [DefOf]
    public static class Regen_DefOf
    {
        public static HediffDef Sofis_Regenerating;

        public static AbilityDef HemogenRegenerate;

        static Regen_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(Regen_DefOf));
        }
    }
}

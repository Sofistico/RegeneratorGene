using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RegeneratorGene
{
    /*public class GeneRegensMod : Mod
    {
        public RegenSettings regenSettings;

        public GeneRegensMod(ModContentPack pack) : base(pack)
        {
            this.regenSettings = GetSettings<RegenSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Should a generated Sanguaphobe not have the Hemogen Regen gene?", 
                ref regenSettings.doNotSpawnSanguaphobeWithNewGene,
                "Mark so that the Sanguaphobe doesn't spawn with the new gene");

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Regenerator gene".Translate();
        }

        public RegenSettings GetSettings()
            => LoadedModManager.GetMod<GeneRegensMod>().GetSettings<RegenSettings>();
    }*/
}

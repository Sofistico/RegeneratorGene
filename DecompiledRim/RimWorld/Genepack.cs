using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Genepack : GeneSetHolderBase
	{
		public Thing targetContainer;

		private float hpRecoveryPct;

		private float deteriorationPct;

		private const int HitPointRecoveryPerDayInGeneBankPerDay = 1;

		public override string LabelNoCount
		{
			get
			{
				if (geneSet != null && geneSet.Label != "ERR")
				{
					return geneSet.Label;
				}
				return base.LabelNoCount;
			}
		}

		private CompGenepackContainer ParentContainer
		{
			get
			{
				IThingHolder parentHolder = base.ParentHolder;
				CompGenepackContainer result;
				if (parentHolder == null || (result = parentHolder as CompGenepackContainer) == null)
				{
					return null;
				}
				return result;
			}
		}

		public bool Deteriorating
		{
			get
			{
				CompGenepackContainer parentContainer = ParentContainer;
				if (parentContainer == null || !parentContainer.PowerOn)
				{
					return true;
				}
				return false;
			}
		}

		public void Initialize(List<GeneDef> genes)
		{
			geneSet = new GeneSet();
			foreach (GeneDef gene in genes)
			{
				geneSet.AddGene(gene);
			}
		}

		public override void PostMake()
		{
			if (!ModLister.CheckBiotech("genepack"))
			{
				Destroy();
				return;
			}
			base.PostMake();
			geneSet = GeneUtility.GenerateGeneSet();
		}

		public override void TickRare()
		{
			base.TickRare();
			if (ParentContainer == null)
			{
				return;
			}
			if (Deteriorating)
			{
				float statValue = this.GetStatValue(StatDefOf.DeteriorationRate);
				if (statValue > 0.001f && base.ParentHolder != null)
				{
					deteriorationPct += statValue * 250f / 60000f;
					if (deteriorationPct >= 1f)
					{
						deteriorationPct -= 1f;
						SteadyEnvironmentEffects.DoDeteriorationDamage(this, base.PositionHeld, base.MapHeld, sendMessage: true);
					}
				}
			}
			else
			{
				if (HitPoints >= base.MaxHitPoints)
				{
					return;
				}
				hpRecoveryPct += 0.004166667f;
				if (hpRecoveryPct >= 1f)
				{
					hpRecoveryPct -= 1f;
					HitPoints++;
					if (HitPoints == base.MaxHitPoints)
					{
						hpRecoveryPct = 0f;
					}
				}
			}
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (targetContainer != null && targetContainer.Map == base.Map)
			{
				GenDraw.DrawLineBetween(DrawPos, targetContainer.DrawPos);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref targetContainer, "targetContainer");
			Scribe_Values.Look(ref hpRecoveryPct, "hpRecoveryPct", 0f);
			Scribe_Values.Look(ref deteriorationPct, "deteriorationPct", 0f);
		}
	}
}

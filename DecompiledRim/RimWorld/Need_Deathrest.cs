using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Need_Deathrest : Need
	{
		public int lastDeathrestTick = -999;

		[Unsaved(false)]
		private Gene_Deathrest cachedDeathrestGene;

		public const float LevelForAlert = 0.1f;

		public const float FallPerDay = 71f / (678f * (float)Math.PI);

		public const float GainPerDayDeathresting = 0.2f;

		private const float Interval = 400f;

		public const float HemogenGainPerDayDeathrest = 0.08f;

		public bool Deathresting => Find.TickManager.TicksGame <= lastDeathrestTick + 1;

		private Gene_Deathrest DeathrestGene
		{
			get
			{
				if (cachedDeathrestGene == null)
				{
					cachedDeathrestGene = pawn.genes?.GetFirstGeneOfType<Gene_Deathrest>();
				}
				return cachedDeathrestGene;
			}
		}

		public override int GUIChangeArrow
		{
			get
			{
				if (IsFrozen)
				{
					return 0;
				}
				if (!Deathresting)
				{
					return -1;
				}
				return 1;
			}
		}

		public Need_Deathrest(Pawn pawn)
			: base(pawn)
		{
			threshPercents = new List<float> { 0.1f };
		}

		public override void SetInitialLevel()
		{
			CurLevel = 1f;
		}

		public override void NeedInterval()
		{
			if (!IsFrozen)
			{
				CurLevel += (Deathresting ? (0.2f * (DeathrestGene?.DeathrestEfficiency ?? 1f)) : (-71f / (678f * (float)Math.PI))) / 400f;
				CheckForStateChange();
			}
		}

		private void CheckForStateChange()
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DeathrestExhaustion);
			if (firstHediffOfDef != null && CurLevel > 0f)
			{
				firstHediffOfDef.Severity = 0f;
			}
			else if (CurLevel == 0f)
			{
				DeathrestGene?.RemoveOldDeathrestBonuses();
				pawn.health.AddHediff(HediffDefOf.DeathrestExhaustion);
			}
		}

		public override string GetTipString()
		{
			string text = (base.LabelCap + ": " + base.CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + "\n";
			if (!Deathresting)
			{
				text = ((!(base.CurLevelPercentage > 0.1f)) ? (text + "PawnShouldDeathrestNow".Translate(pawn.Named("PAWN")).CapitalizeFirst().Colorize(ColorLibrary.RedReadable)) : (text + TranslatorFormattedStringExtensions.Translate(arg2: "PeriodDays".Translate(((base.CurLevelPercentage - 0.1f) / (71f / (678f * (float)Math.PI))).ToString("F1")).Named("DURATION"), key: "NextDeathrestNeed", arg1: pawn.Named("PAWN")).Resolve().CapitalizeFirst()));
				text += "\n\n";
			}
			return text + def.description;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref lastDeathrestTick, "lastDeathrestTick", -999);
		}
	}
}

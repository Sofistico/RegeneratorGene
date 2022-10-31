using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class SanguophageUtility
	{
		public static bool ShouldBeDeathrestingOrInComaInsteadOfDead(Pawn pawn)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			if (!pawn.health.ShouldBeDead())
			{
				return false;
			}
			if (pawn.genes != null && pawn.genes.HasGene(GeneDefOf.Deathless))
			{
				BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
				if (brain != null && !pawn.health.hediffSet.PartIsMissing(brain) && pawn.health.hediffSet.GetPartHealth(brain) > 0f)
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryStartDeathrest(Pawn pawn, DeathrestStartReason reason)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			if (!pawn.Spawned)
			{
				return false;
			}
			if (!pawn.Deathresting)
			{
				Gene_Deathrest gene_Deathrest = pawn.genes?.GetFirstGeneOfType<Gene_Deathrest>();
				if (gene_Deathrest == null)
				{
					return false;
				}
				if (PawnUtility.ShouldSendNotificationAbout(pawn))
				{
					TaggedString label = "LetterLabelInvoluntaryDeathrest".Translate() + ": " + pawn.LabelShortCap;
					TaggedString text = "LetterTextInvoluntaryDeathrest".Translate(pawn.Named("PAWN"));
					if (reason == DeathrestStartReason.LethalDamage)
					{
						text += "\n\n" + "Reason".Translate() + ": " + "DeathrestLethalDamage".Translate();
					}
					Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, pawn);
				}
				gene_Deathrest.autoWake = reason != DeathrestStartReason.PlayerForced;
				pawn.health.AddHediff(HediffDefOf.Deathrest);
				return true;
			}
			return false;
		}

		public static bool InSunlight(this IntVec3 cell, Map map)
		{
			if (!cell.InBounds(map))
			{
				return false;
			}
			if (!map.roofGrid.Roofed(cell))
			{
				return map.skyManager.CurSkyGlow > 0.1f;
			}
			return false;
		}

		public static string DeathrestJobReport(Pawn pawn)
		{
			Hediff_Deathrest hediff_Deathrest = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Deathrest) as Hediff_Deathrest;
			if (hediff_Deathrest != null && hediff_Deathrest.Paused)
			{
				return "DeathrestPaused".Translate() + ": " + "LethalInjuries".Translate();
			}
			Gene_Deathrest firstGeneOfType = pawn.genes.GetFirstGeneOfType<Gene_Deathrest>();
			TaggedString taggedString = "Deathresting".Translate().CapitalizeFirst() + ": ";
			float deathrestPercent = firstGeneOfType.DeathrestPercent;
			if (deathrestPercent < 1f)
			{
				taggedString += Mathf.Min(deathrestPercent, 0.99f).ToStringPercent("F0");
			}
			else
			{
				taggedString += string.Format("{0} - {1}", "Complete".Translate().CapitalizeFirst(), "CanWakeSafely".Translate());
			}
			if (deathrestPercent < 1f)
			{
				taggedString += ", " + "DurationLeft".Translate((firstGeneOfType.MinDeathrestTicks - firstGeneOfType.deathrestTicks).ToStringTicksToPeriod());
			}
			return taggedString.Resolve();
		}

		public static bool WouldDieFromAdditionalBloodLoss(this Pawn pawn, float severity)
		{
			if (pawn.Dead || !pawn.RaceProps.IsFlesh)
			{
				return false;
			}
			float num = severity;
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
			if (firstHediffOfDef != null)
			{
				num += firstHediffOfDef.Severity;
			}
			if (num >= HediffDefOf.BloodLoss.lethalSeverity)
			{
				return true;
			}
			if (HediffDefOf.BloodLoss.stages[HediffDefOf.BloodLoss.StageAtSeverity(num)].lifeThreatening)
			{
				return true;
			}
			return false;
		}

		public static void DoBite(Pawn biter, Pawn victim, float targetHemogenGain, float nutritionGain, float targetBloodLoss, float victimResistanceGain, IntRange bloodFilthToSpawnRange, ThoughtDef thoughtDefToGiveTarget = null, ThoughtDef opinionThoughtToGiveTarget = null)
		{
			if (!ModLister.CheckBiotech("Sanguophage bite"))
			{
				return;
			}
			float offset = targetHemogenGain * victim.BodySize;
			GeneUtility.OffsetHemogen(biter, offset);
			GeneUtility.OffsetHemogen(victim, offset);
			if (biter.needs?.food != null)
			{
				biter.needs.food.CurLevel += nutritionGain;
			}
			if (thoughtDefToGiveTarget != null)
			{
				victim.needs?.mood?.thoughts?.memories?.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(thoughtDefToGiveTarget), biter);
			}
			if (opinionThoughtToGiveTarget != null)
			{
				victim.needs?.mood?.thoughts?.memories?.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(opinionThoughtToGiveTarget), biter);
			}
			if (targetBloodLoss > 0f)
			{
				victim.health.AddHediff(HediffDefOf.BloodfeederMark, ExecutionUtility.ExecuteCutPart(victim));
				Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, victim);
				hediff.Severity = targetBloodLoss;
				victim.health.AddHediff(hediff);
			}
			if (victim.IsPrisoner && victimResistanceGain > 0f)
			{
				victim.guest.resistance = Mathf.Min(victim.guest.resistance + victimResistanceGain, victim.kindDef.initialResistanceRange.Value.TrueMax);
			}
			int randomInRange = bloodFilthToSpawnRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				IntVec3 c = victim.Position;
				if (randomInRange > 1 && Rand.Chance(0.8888f))
				{
					c = victim.Position.RandomAdjacentCell8Way();
				}
				if (c.InBounds(victim.MapHeld))
				{
					FilthMaker.TryMakeFilth(c, victim.MapHeld, victim.RaceProps.BloodDef, victim.LabelShort);
				}
			}
		}
	}
}

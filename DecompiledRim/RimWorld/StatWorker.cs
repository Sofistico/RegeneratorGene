using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatWorker
	{
		public const int IGNORE_CACHE = -1;

		private Dictionary<Thing, StatCacheEntry> temporaryStatCache;

		private Dictionary<Thing, float> immutableStatCache;

		protected StatDef stat;

		public void InitSetStat(StatDef newStat)
		{
			stat = newStat;
		}

		public void SetCacheability(bool immutable)
		{
			immutableStatCache = (immutable ? new Dictionary<Thing, float>() : null);
			if (stat.cacheable)
			{
				temporaryStatCache = new Dictionary<Thing, StatCacheEntry>();
			}
		}

		public float GetValue(Thing thing, bool applyPostProcess = true, int cacheStaleAfterTicks = -1)
		{
			if (stat.immutable)
			{
				if (immutableStatCache.ContainsKey(thing))
				{
					return immutableStatCache[thing];
				}
				float value = GetValue(StatRequest.For(thing));
				immutableStatCache[thing] = value;
				return value;
			}
			int ticksGame = Find.TickManager.TicksGame;
			if (cacheStaleAfterTicks != -1 && temporaryStatCache != null && temporaryStatCache.TryGetValue(thing, out var value2) && ticksGame - value2.gameTick < cacheStaleAfterTicks)
			{
				return value2.statValue;
			}
			float value3 = GetValue(StatRequest.For(thing));
			if (temporaryStatCache != null)
			{
				if (!temporaryStatCache.ContainsKey(thing))
				{
					temporaryStatCache[thing] = new StatCacheEntry(value3, ticksGame);
				}
				else
				{
					value2 = temporaryStatCache[thing];
					value2.statValue = value3;
					value2.gameTick = ticksGame;
				}
			}
			return value3;
		}

		public float GetValue(Thing thing, Pawn pawn, bool applyPostProcess = true)
		{
			return GetValue(StatRequest.For(thing, pawn));
		}

		public float GetValue(StatRequest req, bool applyPostProcess = true)
		{
			if (stat.minifiedThingInherits)
			{
				MinifiedThing minifiedThing = req.Thing as MinifiedThing;
				if (minifiedThing != null)
				{
					if (minifiedThing.InnerThing != null)
					{
						return minifiedThing.InnerThing.GetStatValue(stat, applyPostProcess);
					}
					Log.Error("MinifiedThing's inner thing is null.");
				}
			}
			float val = GetValueUnfinalized(req, applyPostProcess);
			FinalizeValue(req, ref val, applyPostProcess);
			return val;
		}

		public float GetValueAbstract(BuildableDef def, ThingDef stuffDef = null)
		{
			return GetValue(StatRequest.For(def, stuffDef));
		}

		public float GetValueAbstract(AbilityDef def, Pawn forPawn = null)
		{
			return GetValue(StatRequest.For(def, forPawn));
		}

		public virtual float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			if (!stat.supressDisabledError && Prefs.DevMode && IsDisabledFor(req.Thing))
			{
				Log.ErrorOnce($"Attempted to calculate value for disabled stat {stat}; this is meant as a consistency check, either set the stat to neverDisabled or ensure this pawn cannot accidentally use this stat (thing={req.Thing.ToStringSafe()})", 75193282 + stat.index);
			}
			float num = GetBaseValueFor(req);
			Pawn pawn = req.Thing as Pawn;
			if (pawn != null)
			{
				if (pawn.skills != null)
				{
					if (stat.skillNeedOffsets != null)
					{
						for (int i = 0; i < stat.skillNeedOffsets.Count; i++)
						{
							num += stat.skillNeedOffsets[i].ValueFor(pawn);
						}
					}
				}
				else
				{
					num += stat.noSkillOffset;
				}
				if (stat.capacityOffsets != null)
				{
					for (int j = 0; j < stat.capacityOffsets.Count; j++)
					{
						PawnCapacityOffset pawnCapacityOffset = stat.capacityOffsets[j];
						num += pawnCapacityOffset.GetOffset(pawn.health.capacities.GetLevel(pawnCapacityOffset.capacity));
					}
				}
				if (pawn.story != null)
				{
					for (int k = 0; k < pawn.story.traits.allTraits.Count; k++)
					{
						if (!pawn.story.traits.allTraits[k].Suppressed)
						{
							num += pawn.story.traits.allTraits[k].OffsetOfStat(stat);
						}
					}
				}
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (int l = 0; l < hediffs.Count; l++)
				{
					HediffStage curStage = hediffs[l].CurStage;
					if (curStage != null)
					{
						float num2 = curStage.statOffsets.GetStatOffsetFromList(stat);
						if (num2 != 0f && curStage.statOffsetEffectMultiplier != null)
						{
							num2 *= pawn.GetStatValue(curStage.statOffsetEffectMultiplier);
						}
						if (num2 != 0f && curStage.multiplyStatChangesBySeverity)
						{
							num2 *= hediffs[l].Severity;
						}
						num += num2;
					}
				}
				if (pawn.Ideo != null)
				{
					List<Precept> preceptsListForReading = pawn.Ideo.PreceptsListForReading;
					for (int m = 0; m < preceptsListForReading.Count; m++)
					{
						if (preceptsListForReading[m].def.statOffsets != null)
						{
							float statOffsetFromList = preceptsListForReading[m].def.statOffsets.GetStatOffsetFromList(stat);
							num += statOffsetFromList;
						}
					}
					Precept_Role role = pawn.Ideo.GetRole(pawn);
					if (role != null && role.def.roleEffects != null)
					{
						foreach (RoleEffect roleEffect in role.def.roleEffects)
						{
							RoleEffect_PawnStatOffset roleEffect_PawnStatOffset;
							if ((roleEffect_PawnStatOffset = roleEffect as RoleEffect_PawnStatOffset) != null && roleEffect_PawnStatOffset.statDef == stat)
							{
								num += roleEffect_PawnStatOffset.modifier;
							}
						}
					}
				}
				if (ModsConfig.BiotechActive && pawn.genes != null)
				{
					List<Gene> genesListForReading = pawn.genes.GenesListForReading;
					for (int n = 0; n < genesListForReading.Count; n++)
					{
						if (!genesListForReading[n].Active)
						{
							continue;
						}
						num += genesListForReading[n].def.statOffsets.GetStatOffsetFromList(stat);
						if (genesListForReading[n].def.conditionalStatAffecters == null)
						{
							continue;
						}
						for (int num3 = 0; num3 < genesListForReading[n].def.conditionalStatAffecters.Count; num3++)
						{
							ConditionalStatAffecter conditionalStatAffecter = genesListForReading[n].def.conditionalStatAffecters[num3];
							if (conditionalStatAffecter.Applies(req))
							{
								num += conditionalStatAffecter.statOffsets.GetStatOffsetFromList(stat);
							}
						}
					}
				}
				num += pawn.ageTracker.CurLifeStage.statOffsets.GetStatOffsetFromList(stat);
				if (pawn.apparel != null)
				{
					for (int num4 = 0; num4 < pawn.apparel.WornApparel.Count; num4++)
					{
						num += StatOffsetFromGear(pawn.apparel.WornApparel[num4], stat);
					}
				}
				if (pawn.equipment != null && pawn.equipment.Primary != null)
				{
					num += StatOffsetFromGear(pawn.equipment.Primary, stat);
				}
				if (pawn.story != null)
				{
					for (int num5 = 0; num5 < pawn.story.traits.allTraits.Count; num5++)
					{
						if (!pawn.story.traits.allTraits[num5].Suppressed)
						{
							num *= pawn.story.traits.allTraits[num5].MultiplierOfStat(stat);
						}
					}
				}
				for (int num6 = 0; num6 < hediffs.Count; num6++)
				{
					HediffStage curStage2 = hediffs[num6].CurStage;
					if (curStage2 != null)
					{
						float num7 = curStage2.statFactors.GetStatFactorFromList(stat);
						if (Math.Abs(num7 - 1f) > float.Epsilon && curStage2.statFactorEffectMultiplier != null)
						{
							num7 = ScaleFactor(num7, pawn.GetStatValue(curStage2.statFactorEffectMultiplier));
						}
						if (curStage2.multiplyStatChangesBySeverity)
						{
							num7 = ScaleFactor(num7, hediffs[num6].Severity);
						}
						num *= num7;
					}
				}
				if (pawn.Ideo != null)
				{
					List<Precept> preceptsListForReading2 = pawn.Ideo.PreceptsListForReading;
					for (int num8 = 0; num8 < preceptsListForReading2.Count; num8++)
					{
						if (preceptsListForReading2[num8].def.statFactors != null)
						{
							float statFactorFromList = preceptsListForReading2[num8].def.statFactors.GetStatFactorFromList(stat);
							num *= statFactorFromList;
						}
					}
					Precept_Role role2 = pawn.Ideo.GetRole(pawn);
					if (role2 != null && role2.def.roleEffects != null)
					{
						foreach (RoleEffect roleEffect2 in role2.def.roleEffects)
						{
							RoleEffect_PawnStatFactor roleEffect_PawnStatFactor;
							if ((roleEffect_PawnStatFactor = roleEffect2 as RoleEffect_PawnStatFactor) != null && roleEffect_PawnStatFactor.statDef == stat)
							{
								num *= roleEffect_PawnStatFactor.modifier;
							}
						}
					}
				}
				if (ModsConfig.BiotechActive && pawn.genes != null)
				{
					List<Gene> genesListForReading2 = pawn.genes.GenesListForReading;
					for (int num9 = 0; num9 < genesListForReading2.Count; num9++)
					{
						if (!genesListForReading2[num9].Active)
						{
							continue;
						}
						num *= genesListForReading2[num9].def.statFactors.GetStatFactorFromList(stat);
						if (genesListForReading2[num9].def.conditionalStatAffecters == null)
						{
							continue;
						}
						for (int num10 = 0; num10 < genesListForReading2[num9].def.conditionalStatAffecters.Count; num10++)
						{
							ConditionalStatAffecter conditionalStatAffecter2 = genesListForReading2[num9].def.conditionalStatAffecters[num10];
							if (conditionalStatAffecter2.Applies(req))
							{
								num *= conditionalStatAffecter2.statFactors.GetStatFactorFromList(stat);
							}
						}
					}
				}
				num *= pawn.ageTracker.CurLifeStage.statFactors.GetStatFactorFromList(stat);
			}
			if (req.StuffDef != null)
			{
				if (num > 0f || stat.applyFactorsIfNegative)
				{
					num *= req.StuffDef.stuffProps.statFactors.GetStatFactorFromList(stat);
				}
				num += req.StuffDef.stuffProps.statOffsets.GetStatOffsetFromList(stat);
			}
			if (req.ForAbility)
			{
				if (stat.statFactors != null)
				{
					for (int num11 = 0; num11 < stat.statFactors.Count; num11++)
					{
						num *= req.AbilityDef.statBases.GetStatValueFromList(stat.statFactors[num11], 1f);
					}
				}
				Pawn pawn2 = req.Pawn;
				if (pawn2 != null && pawn2.Ideo != null)
				{
					List<Precept> preceptsListForReading3 = pawn2.Ideo.PreceptsListForReading;
					for (int num12 = 0; num12 < preceptsListForReading3.Count; num12++)
					{
						if (preceptsListForReading3[num12].def.statFactors != null)
						{
							float statFactorFromList2 = preceptsListForReading3[num12].def.statFactors.GetStatFactorFromList(stat);
							num *= statFactorFromList2;
						}
						if (preceptsListForReading3[num12].def.abilityStatFactors == null)
						{
							continue;
						}
						foreach (AbilityStatModifiers abilityStatFactor in preceptsListForReading3[num12].def.abilityStatFactors)
						{
							if (abilityStatFactor.ability == req.AbilityDef)
							{
								float statFactorFromList3 = abilityStatFactor.modifiers.GetStatFactorFromList(stat);
								num *= statFactorFromList3;
							}
						}
					}
				}
			}
			if (req.HasThing)
			{
				CompAffectedByFacilities compAffectedByFacilities = req.Thing.TryGetComp<CompAffectedByFacilities>();
				if (compAffectedByFacilities != null)
				{
					num += compAffectedByFacilities.GetStatOffset(stat);
				}
				if (stat.statFactors != null)
				{
					for (int num13 = 0; num13 < stat.statFactors.Count; num13++)
					{
						num *= req.Thing.GetStatValue(stat.statFactors[num13]);
					}
				}
				if (pawn != null)
				{
					if (pawn.skills != null)
					{
						if (stat.skillNeedFactors != null)
						{
							for (int num14 = 0; num14 < stat.skillNeedFactors.Count; num14++)
							{
								num *= stat.skillNeedFactors[num14].ValueFor(pawn);
							}
						}
					}
					else
					{
						num *= stat.noSkillFactor;
					}
					if (stat.capacityFactors != null)
					{
						for (int num15 = 0; num15 < stat.capacityFactors.Count; num15++)
						{
							PawnCapacityFactor pawnCapacityFactor = stat.capacityFactors[num15];
							float factor = pawnCapacityFactor.GetFactor(pawn.health.capacities.GetLevel(pawnCapacityFactor.capacity));
							num = Mathf.Lerp(num, num * factor, pawnCapacityFactor.weight);
						}
					}
					if (pawn.Inspired)
					{
						num += pawn.InspirationDef.statOffsets.GetStatOffsetFromList(stat);
						num *= pawn.InspirationDef.statFactors.GetStatFactorFromList(stat);
					}
				}
			}
			return num;
		}

		public virtual string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
		{
			StringBuilder stringBuilder = new StringBuilder();
			float baseValueFor = GetBaseValueFor(req);
			if (baseValueFor != 0f || stat.showZeroBaseValue)
			{
				stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(baseValueFor, numberSense));
			}
			Pawn pawn = req.Thing as Pawn;
			if (pawn != null)
			{
				if (pawn.skills != null)
				{
					if (stat.skillNeedOffsets != null)
					{
						stringBuilder.AppendLine("StatsReport_Skills".Translate());
						for (int i = 0; i < stat.skillNeedOffsets.Count; i++)
						{
							SkillNeed skillNeed = stat.skillNeedOffsets[i];
							int level = pawn.skills.GetSkill(skillNeed.skill).Level;
							float val = skillNeed.ValueFor(pawn);
							stringBuilder.AppendLine((string)("    " + skillNeed.skill.LabelCap + " (") + level + "): " + val.ToStringSign() + ValueToString(val, finalized: false));
						}
					}
				}
				else if (stat.noSkillOffset != 0f)
				{
					stringBuilder.AppendLine("StatsReport_Skills".Translate());
					stringBuilder.AppendLine("    " + "default".Translate().CapitalizeFirst() + " : " + stat.noSkillOffset.ToStringSign() + ValueToString(stat.noSkillOffset, finalized: false));
				}
				if (stat.capacityOffsets != null)
				{
					stringBuilder.AppendLine("StatsReport_Health".CanTranslate() ? "StatsReport_Health".Translate() : "StatsReport_HealthFactors".Translate());
					foreach (PawnCapacityOffset item in stat.capacityOffsets.OrderBy((PawnCapacityOffset hfa) => hfa.capacity.listOrder))
					{
						string text = item.capacity.GetLabelFor(pawn).CapitalizeFirst();
						float level2 = pawn.health.capacities.GetLevel(item.capacity);
						float offset = item.GetOffset(pawn.health.capacities.GetLevel(item.capacity));
						string text2 = ValueToString(offset, finalized: false);
						string text3 = Mathf.Min(level2, item.max).ToStringPercent() + ", " + "HealthOffsetScale".Translate(item.scale + "x");
						if (item.max < 999f)
						{
							text3 += ", " + "HealthFactorMaxImpact".Translate(item.max.ToStringPercent());
						}
						stringBuilder.AppendLine("    " + text + ": " + offset.ToStringSign() + text2 + " (" + text3 + ")");
					}
				}
				if ((int)pawn.RaceProps.intelligence >= 1)
				{
					if (pawn.story != null && pawn.story.traits != null)
					{
						List<Trait> list = pawn.story.traits.allTraits.Where((Trait tr) => !tr.Suppressed && tr.CurrentData.statOffsets != null && tr.CurrentData.statOffsets.Any((StatModifier se) => se.stat == stat)).ToList();
						List<Trait> list2 = pawn.story.traits.allTraits.Where((Trait tr) => !tr.Suppressed && tr.CurrentData.statFactors != null && tr.CurrentData.statFactors.Any((StatModifier se) => se.stat == stat)).ToList();
						if (list.Count > 0 || list2.Count > 0)
						{
							stringBuilder.AppendLine("StatsReport_RelevantTraits".Translate());
							for (int j = 0; j < list.Count; j++)
							{
								Trait trait = list[j];
								string valueToStringAsOffset = trait.CurrentData.statOffsets.First((StatModifier se) => se.stat == stat).ValueToStringAsOffset;
								stringBuilder.AppendLine("    " + trait.LabelCap + ": " + valueToStringAsOffset);
							}
							for (int k = 0; k < list2.Count; k++)
							{
								Trait trait2 = list2[k];
								string toStringAsFactor = trait2.CurrentData.statFactors.First((StatModifier se) => se.stat == stat).ToStringAsFactor;
								stringBuilder.AppendLine("    " + trait2.LabelCap + ": " + toStringAsFactor);
							}
						}
					}
					if (RelevantGear(pawn, stat).Any())
					{
						stringBuilder.AppendLine("StatsReport_RelevantGear".Translate());
						if (pawn.apparel != null)
						{
							for (int l = 0; l < pawn.apparel.WornApparel.Count; l++)
							{
								Apparel apparel = pawn.apparel.WornApparel[l];
								if (GearAffectsStat(apparel.def, stat))
								{
									stringBuilder.AppendLine(InfoTextLineFromGear(apparel, stat));
								}
							}
						}
						if (pawn.equipment != null && pawn.equipment.Primary != null && (GearAffectsStat(pawn.equipment.Primary.def, stat) || GearHasCompsThatAffectStat(pawn.equipment.Primary, stat)))
						{
							stringBuilder.AppendLine(InfoTextLineFromGear(pawn.equipment.Primary, stat));
						}
					}
				}
				bool flag = false;
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (int m = 0; m < hediffs.Count; m++)
				{
					HediffStage curStage = hediffs[m].CurStage;
					if (curStage == null)
					{
						continue;
					}
					float num = curStage.statOffsets.GetStatOffsetFromList(stat);
					if (num != 0f)
					{
						float val2 = num;
						if (curStage.statOffsetEffectMultiplier != null)
						{
							num *= pawn.GetStatValue(curStage.statOffsetEffectMultiplier);
						}
						if (curStage.multiplyStatChangesBySeverity)
						{
							num *= hediffs[m].Severity;
						}
						if (!flag)
						{
							stringBuilder.AppendLine("StatsReport_RelevantHediffs".Translate());
							flag = true;
						}
						stringBuilder.Append("    " + hediffs[m].LabelBaseCap + ": " + ValueToString(num, finalized: false, ToStringNumberSense.Offset));
						if (curStage.statOffsetEffectMultiplier != null)
						{
							stringBuilder.Append(" (" + ValueToString(val2, finalized: false, ToStringNumberSense.Offset) + " x " + ValueToString(pawn.GetStatValue(curStage.statOffsetEffectMultiplier), finalized: true, curStage.statOffsetEffectMultiplier.toStringNumberSense) + " " + curStage.statOffsetEffectMultiplier.LabelCap + ")");
						}
						stringBuilder.AppendLine();
					}
					float num2 = curStage.statFactors.GetStatFactorFromList(stat);
					if (Math.Abs(num2 - 1f) > float.Epsilon)
					{
						float val3 = num2;
						if (curStage.multiplyStatChangesBySeverity)
						{
							num2 = ScaleFactor(num2, hediffs[m].Severity);
						}
						if (curStage.statFactorEffectMultiplier != null)
						{
							num2 = ScaleFactor(num2, pawn.GetStatValue(curStage.statFactorEffectMultiplier));
						}
						if (!flag)
						{
							stringBuilder.AppendLine("StatsReport_RelevantHediffs".Translate());
							flag = true;
						}
						stringBuilder.Append("    " + hediffs[m].LabelBaseCap + ": " + ValueToString(num2, finalized: false, ToStringNumberSense.Factor));
						if (curStage.statFactorEffectMultiplier != null)
						{
							stringBuilder.Append(" (" + ValueToString(val3, finalized: false, ToStringNumberSense.Factor) + " x " + ValueToString(pawn.GetStatValue(curStage.statFactorEffectMultiplier), finalized: false) + " " + curStage.statFactorEffectMultiplier.LabelCap + ")");
						}
						stringBuilder.AppendLine();
					}
				}
				if (pawn.Ideo != null)
				{
					List<Precept> preceptsListForReading = pawn.Ideo.PreceptsListForReading;
					for (int n = 0; n < preceptsListForReading.Count; n++)
					{
						float statOffsetFromList = preceptsListForReading[n].def.statOffsets.GetStatOffsetFromList(stat);
						if (statOffsetFromList != 0f)
						{
							stringBuilder.AppendLine("StatsReport_Ideoligion".Translate() + ": " + ValueToString(statOffsetFromList, finalized: false, ToStringNumberSense.Offset));
						}
						float statFactorFromList = preceptsListForReading[n].def.statFactors.GetStatFactorFromList(stat);
						if (statFactorFromList != 1f)
						{
							stringBuilder.AppendLine("StatsReport_Ideoligion".Translate() + ": " + ValueToString(statFactorFromList, finalized: false, ToStringNumberSense.Factor));
						}
					}
					Precept_Role role = pawn.Ideo.GetRole(pawn);
					if (role != null && role.def.roleEffects != null)
					{
						foreach (RoleEffect roleEffect in role.def.roleEffects)
						{
							RoleEffect_PawnStatOffset roleEffect_PawnStatOffset;
							RoleEffect_PawnStatFactor roleEffect_PawnStatFactor;
							if ((roleEffect_PawnStatOffset = roleEffect as RoleEffect_PawnStatOffset) != null)
							{
								if (roleEffect_PawnStatOffset.statDef == stat)
								{
									stringBuilder.AppendLine(role.LabelCap + ": " + ValueToString(roleEffect_PawnStatOffset.modifier, finalized: false, ToStringNumberSense.Offset));
								}
							}
							else if ((roleEffect_PawnStatFactor = roleEffect as RoleEffect_PawnStatFactor) != null && roleEffect_PawnStatFactor.statDef == stat)
							{
								stringBuilder.AppendLine(role.LabelCap + ": " + ValueToString(roleEffect_PawnStatFactor.modifier, finalized: false, ToStringNumberSense.Factor));
							}
						}
					}
				}
				if (ModsConfig.BiotechActive && pawn.genes != null)
				{
					bool flag2 = false;
					List<Gene> genesListForReading = pawn.genes.GenesListForReading;
					for (int num3 = 0; num3 < genesListForReading.Count; num3++)
					{
						if (!genesListForReading[num3].Active)
						{
							continue;
						}
						float statOffsetFromList2 = genesListForReading[num3].def.statOffsets.GetStatOffsetFromList(stat);
						if (statOffsetFromList2 != 0f)
						{
							if (!flag2)
							{
								stringBuilder.AppendLine("StatsReport_Genes".Translate());
								flag2 = true;
							}
							stringBuilder.AppendLine("    " + genesListForReading[num3].LabelCap + ": " + ValueToString(statOffsetFromList2, finalized: false, ToStringNumberSense.Offset));
						}
						float statFactorFromList2 = genesListForReading[num3].def.statFactors.GetStatFactorFromList(stat);
						if (statFactorFromList2 != 1f)
						{
							if (!flag2)
							{
								stringBuilder.AppendLine("StatsReport_Genes".Translate());
								flag2 = true;
							}
							stringBuilder.AppendLine("    " + genesListForReading[num3].LabelCap + ": " + ValueToString(statFactorFromList2, finalized: false, ToStringNumberSense.Factor));
						}
						if (genesListForReading[num3].def.conditionalStatAffecters == null)
						{
							continue;
						}
						for (int num4 = 0; num4 < genesListForReading[num3].def.conditionalStatAffecters.Count; num4++)
						{
							ConditionalStatAffecter conditionalStatAffecter = genesListForReading[num3].def.conditionalStatAffecters[num4];
							if (!conditionalStatAffecter.Applies(req))
							{
								continue;
							}
							float statOffsetFromList3 = conditionalStatAffecter.statOffsets.GetStatOffsetFromList(stat);
							if (statOffsetFromList3 != 0f)
							{
								if (!flag2)
								{
									stringBuilder.AppendLine("StatsReport_Genes".Translate());
									flag2 = true;
								}
								stringBuilder.AppendLine("    " + genesListForReading[num3].LabelCap + " (" + conditionalStatAffecter.Label + "): " + ValueToString(statOffsetFromList3, finalized: false, ToStringNumberSense.Offset));
							}
							float statFactorFromList3 = conditionalStatAffecter.statFactors.GetStatFactorFromList(stat);
							if (statFactorFromList3 != 1f)
							{
								if (!flag2)
								{
									stringBuilder.AppendLine("StatsReport_Genes".Translate());
									flag2 = true;
								}
								stringBuilder.AppendLine("    " + genesListForReading[num3].LabelCap + " (" + conditionalStatAffecter.Label + "): " + ValueToString(statFactorFromList3, finalized: false, ToStringNumberSense.Factor));
							}
						}
					}
				}
				float statOffsetFromList4 = pawn.ageTracker.CurLifeStage.statOffsets.GetStatOffsetFromList(stat);
				if (statOffsetFromList4 != 0f)
				{
					stringBuilder.AppendLine("StatsReport_LifeStage".Translate() + " (" + pawn.ageTracker.CurLifeStage.label + "): " + statOffsetFromList4.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset));
				}
				float statFactorFromList4 = pawn.ageTracker.CurLifeStage.statFactors.GetStatFactorFromList(stat);
				if (statFactorFromList4 != 1f)
				{
					stringBuilder.AppendLine("StatsReport_LifeStage".Translate() + " (" + pawn.ageTracker.CurLifeStage.label + "): " + statFactorFromList4.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
				}
			}
			if (req.StuffDef != null)
			{
				if (baseValueFor > 0f || stat.applyFactorsIfNegative)
				{
					float statFactorFromList5 = req.StuffDef.stuffProps.statFactors.GetStatFactorFromList(stat);
					if (statFactorFromList5 != 1f)
					{
						stringBuilder.AppendLine("StatsReport_Material".Translate() + " (" + req.StuffDef.LabelCap + "): " + statFactorFromList5.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
					}
				}
				float statOffsetFromList5 = req.StuffDef.stuffProps.statOffsets.GetStatOffsetFromList(stat);
				if (statOffsetFromList5 != 0f)
				{
					stringBuilder.AppendLine("StatsReport_Material".Translate() + " (" + req.StuffDef.LabelCap + "): " + statOffsetFromList5.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset));
				}
			}
			req.Thing.TryGetComp<CompAffectedByFacilities>()?.GetStatsExplanation(stat, stringBuilder);
			if (stat.statFactors != null)
			{
				stringBuilder.AppendLine(stat.statFactorsExplanationHeader ?? ((string)"StatsReport_OtherStats".Translate()));
				for (int num5 = 0; num5 < stat.statFactors.Count; num5++)
				{
					StatDef statDef = stat.statFactors[num5];
					stringBuilder.AppendLine("    " + statDef.LabelCap + ": x" + statDef.Worker.GetValue(req).ToStringPercent());
				}
			}
			if (pawn != null)
			{
				if (pawn.skills != null)
				{
					if (stat.skillNeedFactors != null)
					{
						stringBuilder.AppendLine("StatsReport_Skills".Translate());
						for (int num6 = 0; num6 < stat.skillNeedFactors.Count; num6++)
						{
							SkillNeed skillNeed2 = stat.skillNeedFactors[num6];
							int level3 = pawn.skills.GetSkill(skillNeed2.skill).Level;
							stringBuilder.AppendLine((string)("    " + skillNeed2.skill.LabelCap + " (") + level3 + "): x" + skillNeed2.ValueFor(pawn).ToStringPercent());
						}
					}
				}
				else if (stat.noSkillFactor != 1f)
				{
					stringBuilder.AppendLine("StatsReport_Skills".Translate());
					stringBuilder.AppendLine("    " + "default".Translate().CapitalizeFirst() + " : x" + stat.noSkillFactor.ToStringPercent());
				}
				if (stat.capacityFactors != null)
				{
					stringBuilder.AppendLine("StatsReport_Health".CanTranslate() ? "StatsReport_Health".Translate() : "StatsReport_HealthFactors".Translate());
					if (stat.capacityFactors != null)
					{
						foreach (PawnCapacityFactor item2 in stat.capacityFactors.OrderBy((PawnCapacityFactor hfa) => hfa.capacity.listOrder))
						{
							string text4 = item2.capacity.GetLabelFor(pawn).CapitalizeFirst();
							string text5 = item2.GetFactor(pawn.health.capacities.GetLevel(item2.capacity)).ToStringPercent();
							string text6 = "HealthFactorPercentImpact".Translate(item2.weight.ToStringPercent());
							if (item2.max < 999f)
							{
								text6 += ", " + "HealthFactorMaxImpact".Translate(item2.max.ToStringPercent());
							}
							if (item2.allowedDefect != 0f)
							{
								text6 += ", " + "HealthFactorAllowedDefect".Translate((1f - item2.allowedDefect).ToStringPercent());
							}
							stringBuilder.AppendLine("    " + text4 + ": x" + text5 + " (" + text6 + ")");
						}
					}
				}
				if (pawn.Inspired)
				{
					float statOffsetFromList6 = pawn.InspirationDef.statOffsets.GetStatOffsetFromList(stat);
					if (statOffsetFromList6 != 0f)
					{
						stringBuilder.AppendLine("StatsReport_Inspiration".Translate(pawn.Inspiration.def.LabelCap) + ": " + ValueToString(statOffsetFromList6, finalized: false, ToStringNumberSense.Offset));
					}
					float statFactorFromList6 = pawn.InspirationDef.statFactors.GetStatFactorFromList(stat);
					if (statFactorFromList6 != 1f)
					{
						stringBuilder.AppendLine("StatsReport_Inspiration".Translate(pawn.Inspiration.def.LabelCap) + ": " + statFactorFromList6.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
					}
				}
			}
			return stringBuilder.ToString();
		}

		public virtual void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
		{
			if (stat.parts != null)
			{
				for (int i = 0; i < stat.parts.Count; i++)
				{
					stat.parts[i].TransformValue(req, ref val);
				}
			}
			if (applyPostProcess && stat.postProcessCurve != null)
			{
				val = stat.postProcessCurve.Evaluate(val);
			}
			if (applyPostProcess && !stat.postProcessStatFactors.NullOrEmpty() && req.HasThing)
			{
				for (int j = 0; j < stat.postProcessStatFactors.Count; j++)
				{
					val *= req.Thing.GetStatValue(stat.postProcessStatFactors[j]);
				}
			}
			if (Find.Scenario != null)
			{
				val *= Find.Scenario.GetStatFactor(stat);
			}
			if (Mathf.Abs(val) > stat.roundToFiveOver)
			{
				val = Mathf.Round(val / 5f) * 5f;
			}
			if (stat.roundValue)
			{
				val = Mathf.RoundToInt(val);
			}
			if (applyPostProcess)
			{
				val = Mathf.Clamp(val, stat.minValue, stat.maxValue);
			}
		}

		public virtual string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (stat.parts != null)
			{
				for (int i = 0; i < stat.parts.Count; i++)
				{
					string text = stat.parts[i].ExplanationPart(req);
					if (!text.NullOrEmpty())
					{
						stringBuilder.AppendLine(text);
					}
				}
			}
			if (stat.postProcessCurve != null)
			{
				float value = GetValue(req, applyPostProcess: false);
				float num = stat.postProcessCurve.Evaluate(value);
				if (!Mathf.Approximately(value, num))
				{
					string text2 = ValueToString(value, finalized: false);
					string text3 = stat.ValueToString(num, numberSense);
					stringBuilder.AppendLine("StatsReport_PostProcessed".Translate() + ": " + text2 + " => " + text3);
				}
			}
			if (stat.postProcessStatFactors != null)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				for (int j = 0; j < stat.postProcessStatFactors.Count; j++)
				{
					if (stat.postProcessStatFactors[j].Worker.ShouldShowFor(req))
					{
						StatDef statDef = stat.postProcessStatFactors[j];
						stringBuilder2.AppendLine($"    {statDef.LabelCap}: x{statDef.Worker.GetValue(req).ToStringPercent()}");
					}
				}
				if (stringBuilder2.Length > 0)
				{
					stringBuilder.AppendLine("StatsReport_OtherStats".Translate());
					stringBuilder.AppendLine(stringBuilder2.ToString());
				}
			}
			float statFactor = Find.Scenario.GetStatFactor(stat);
			if (statFactor != 1f)
			{
				stringBuilder.AppendLine("StatsReport_ScenarioFactor".Translate() + ": " + statFactor.ToStringPercent());
			}
			stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(finalVal, stat.toStringNumberSense));
			return stringBuilder.ToString();
		}

		public string GetExplanationFull(StatRequest req, ToStringNumberSense numberSense, float value)
		{
			if (IsDisabledFor(req.Thing))
			{
				return "StatsReport_PermanentlyDisabled".Translate();
			}
			string text = stat.Worker.GetExplanationUnfinalized(req, numberSense).TrimEndNewlines();
			if (!text.NullOrEmpty())
			{
				text += "\n\n";
			}
			return text + stat.Worker.GetExplanationFinalizePart(req, numberSense, value);
		}

		public virtual bool ShouldShowFor(StatRequest req)
		{
			if (stat.alwaysHide)
			{
				return false;
			}
			Def def = req.Def;
			if (!stat.showIfUndefined && !req.StatBases.StatListContains(stat))
			{
				return false;
			}
			if (!stat.CanShowWithLoadedMods())
			{
				return false;
			}
			if (stat.hideInClassicMode && Find.IdeoManager.classicMode)
			{
				return false;
			}
			Pawn pawn;
			if ((pawn = req.Thing as Pawn) != null)
			{
				if (pawn.health != null && !stat.showIfHediffsPresent.NullOrEmpty())
				{
					for (int i = 0; i < stat.showIfHediffsPresent.Count; i++)
					{
						if (!pawn.health.hediffSet.HasHediff(stat.showIfHediffsPresent[i]))
						{
							return false;
						}
					}
				}
				if (stat.showOnSlavesOnly && !pawn.IsSlave)
				{
					return false;
				}
			}
			if (stat == StatDefOf.MaxHitPoints && req.HasThing)
			{
				return false;
			}
			if (!stat.showOnUntradeables && !DisplayTradeStats(req))
			{
				return false;
			}
			ThingDef thingDef = def as ThingDef;
			if (thingDef != null)
			{
				if (thingDef.category == ThingCategory.Pawn)
				{
					if (!stat.showOnPawns)
					{
						return false;
					}
					if (!stat.showOnHumanlikes && thingDef.race.Humanlike)
					{
						return false;
					}
					if (!stat.showOnNonWildManHumanlikes && thingDef.race.Humanlike && !((req.Thing as Pawn)?.IsWildMan() ?? false))
					{
						return false;
					}
					if (!stat.showOnAnimals && thingDef.race.Animal)
					{
						return false;
					}
					if (!stat.showOnMechanoids && thingDef.race.IsMechanoid)
					{
						return false;
					}
					Pawn pawn2;
					if ((pawn2 = req.Thing as Pawn) != null && !stat.showDevelopmentalStageFilter.Has(pawn2.DevelopmentalStage))
					{
						return false;
					}
				}
				if (!stat.showOnUnhaulables && !thingDef.EverHaulable && !thingDef.Minifiable)
				{
					return false;
				}
			}
			if (stat.category == StatCategoryDefOf.BasicsPawn || stat.category == StatCategoryDefOf.BasicsPawnImportant || stat.category == StatCategoryDefOf.PawnCombat)
			{
				if (thingDef != null)
				{
					return thingDef.category == ThingCategory.Pawn;
				}
				return false;
			}
			if (stat.category == StatCategoryDefOf.PawnMisc || stat.category == StatCategoryDefOf.PawnSocial || stat.category == StatCategoryDefOf.PawnWork)
			{
				if (thingDef == null || thingDef.category != ThingCategory.Pawn)
				{
					return false;
				}
				Pawn pawn3;
				if ((pawn3 = req.Thing as Pawn) != null)
				{
					if (pawn3.IsColonyMech && stat.showOnPlayerMechanoids)
					{
						return true;
					}
					if (stat.showOnPawnKind.NotNullAndContains(pawn3.kindDef))
					{
						return true;
					}
				}
				return thingDef.race.Humanlike;
			}
			if (stat.category == StatCategoryDefOf.Building)
			{
				if (thingDef == null)
				{
					return false;
				}
				if (stat == StatDefOf.DoorOpenSpeed)
				{
					return thingDef.IsDoor;
				}
				if (!stat.showOnNonWorkTables && !thingDef.IsWorkTable)
				{
					return false;
				}
				return thingDef.category == ThingCategory.Building;
			}
			if (stat.category == StatCategoryDefOf.Apparel)
			{
				if (thingDef != null)
				{
					if (!thingDef.IsApparel)
					{
						return thingDef.category == ThingCategory.Pawn;
					}
					return true;
				}
				return false;
			}
			if (stat.category == StatCategoryDefOf.Weapon)
			{
				if (thingDef != null)
				{
					if (!thingDef.IsMeleeWeapon)
					{
						return thingDef.IsRangedWeapon;
					}
					return true;
				}
				return false;
			}
			if (stat.category == StatCategoryDefOf.Weapon_Ranged)
			{
				return thingDef?.IsRangedWeapon ?? false;
			}
			if (stat.category == StatCategoryDefOf.Weapon_Melee)
			{
				return thingDef?.IsMeleeWeapon ?? false;
			}
			if (stat.category == StatCategoryDefOf.BasicsNonPawn || stat.category == StatCategoryDefOf.BasicsNonPawnImportant)
			{
				if (thingDef == null || thingDef.category != ThingCategory.Pawn)
				{
					return !req.ForAbility;
				}
				return false;
			}
			if (stat.category == StatCategoryDefOf.Terrain)
			{
				return def is TerrainDef;
			}
			if (req.ForAbility)
			{
				return stat.category == StatCategoryDefOf.Ability;
			}
			if (stat.category.displayAllByDefault)
			{
				return true;
			}
			Log.Error(string.Concat("Unhandled case: ", stat, ", ", def));
			return false;
		}

		public virtual bool IsDisabledFor(Thing thing)
		{
			if (stat.neverDisabled)
			{
				return false;
			}
			if (stat.skillNeedFactors.NullOrEmpty() && stat.skillNeedOffsets.NullOrEmpty() && stat.disableIfSkillDisabled == null)
			{
				return false;
			}
			Pawn pawn = thing as Pawn;
			if (pawn != null && pawn.story != null)
			{
				if (stat.skillNeedFactors != null)
				{
					for (int i = 0; i < stat.skillNeedFactors.Count; i++)
					{
						if (pawn.skills.GetSkill(stat.skillNeedFactors[i].skill).TotallyDisabled)
						{
							return true;
						}
					}
				}
				if (stat.skillNeedOffsets != null)
				{
					for (int j = 0; j < stat.skillNeedOffsets.Count; j++)
					{
						if (pawn.skills.GetSkill(stat.skillNeedOffsets[j].skill).TotallyDisabled)
						{
							return true;
						}
					}
				}
				if (stat.disableIfSkillDisabled != null && pawn.skills.GetSkill(stat.disableIfSkillDisabled).TotallyDisabled)
				{
					return true;
				}
			}
			return false;
		}

		public virtual string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
		{
			return stat.ValueToString(value, numberSense, finalized);
		}

		private static string InfoTextLineFromGear(Thing gear, StatDef stat)
		{
			float f = StatOffsetFromGear(gear, stat);
			return "    " + gear.LabelCap + ": " + f.ToStringByStyle(stat.finalizeEquippedStatOffset ? stat.toStringStyle : stat.ToStringStyleUnfinalized, ToStringNumberSense.Offset);
		}

		public static float StatOffsetFromGear(Thing gear, StatDef stat)
		{
			float val = gear.def.equippedStatOffsets.GetStatOffsetFromList(stat);
			CompBladelinkWeapon compBladelinkWeapon = gear.TryGetComp<CompBladelinkWeapon>();
			if (compBladelinkWeapon != null)
			{
				List<WeaponTraitDef> traitsListForReading = compBladelinkWeapon.TraitsListForReading;
				for (int i = 0; i < traitsListForReading.Count; i++)
				{
					val += traitsListForReading[i].equippedStatOffsets.GetStatOffsetFromList(stat);
				}
			}
			if (Math.Abs(val) > float.Epsilon && !stat.parts.NullOrEmpty())
			{
				foreach (StatPart part in stat.parts)
				{
					part.TransformValue(StatRequest.For(gear), ref val);
				}
				return val;
			}
			return val;
		}

		private static IEnumerable<Thing> RelevantGear(Pawn pawn, StatDef stat)
		{
			if (pawn.apparel != null)
			{
				foreach (Apparel item in pawn.apparel.WornApparel)
				{
					if (GearAffectsStat(item.def, stat))
					{
						yield return item;
					}
				}
			}
			if (pawn.equipment == null)
			{
				yield break;
			}
			foreach (ThingWithComps item2 in pawn.equipment.AllEquipmentListForReading)
			{
				if (GearAffectsStat(item2.def, stat) || GearHasCompsThatAffectStat(item2, stat))
				{
					yield return item2;
				}
			}
		}

		private static bool GearAffectsStat(ThingDef gearDef, StatDef stat)
		{
			if (gearDef.equippedStatOffsets != null)
			{
				for (int i = 0; i < gearDef.equippedStatOffsets.Count; i++)
				{
					if (gearDef.equippedStatOffsets[i].stat == stat && gearDef.equippedStatOffsets[i].value != 0f)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool GearHasCompsThatAffectStat(Thing gear, StatDef stat)
		{
			CompBladelinkWeapon compBladelinkWeapon = gear.TryGetComp<CompBladelinkWeapon>();
			if (compBladelinkWeapon == null)
			{
				return false;
			}
			List<WeaponTraitDef> traitsListForReading = compBladelinkWeapon.TraitsListForReading;
			for (int i = 0; i < traitsListForReading.Count; i++)
			{
				if (traitsListForReading[i].equippedStatOffsets.NullOrEmpty())
				{
					continue;
				}
				for (int j = 0; j < traitsListForReading[i].equippedStatOffsets.Count; j++)
				{
					StatModifier statModifier = traitsListForReading[i].equippedStatOffsets[j];
					if (statModifier.stat == stat && statModifier.value != 0f)
					{
						return true;
					}
				}
			}
			return false;
		}

		public float GetBaseValueFor(StatRequest request)
		{
			float result = stat.defaultBaseValue;
			if (request.StatBases != null)
			{
				for (int i = 0; i < request.StatBases.Count; i++)
				{
					if (request.StatBases[i].stat == stat)
					{
						result = request.StatBases[i].value;
						break;
					}
				}
			}
			return result;
		}

		public virtual string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
		{
			if (!finalized)
			{
				string text = val.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense);
				if (numberSense != ToStringNumberSense.Factor && !stat.formatStringUnfinalized.NullOrEmpty())
				{
					text = string.Format(stat.formatStringUnfinalized, text);
				}
				return text;
			}
			string text2 = val.ToStringByStyle(stat.toStringStyle, numberSense);
			if (numberSense != ToStringNumberSense.Factor && !stat.formatString.NullOrEmpty())
			{
				text2 = string.Format(stat.formatString, text2);
			}
			return text2;
		}

		public virtual IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
		{
			Pawn pawn = statRequest.Thing as Pawn;
			if (pawn == null)
			{
				yield break;
			}
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null)
				{
					float num = curStage.statOffsets.GetStatOffsetFromList(stat);
					if (num != 0f && curStage.statOffsetEffectMultiplier != null)
					{
						num *= pawn.GetStatValue(curStage.statOffsetEffectMultiplier);
					}
					float num2 = curStage.statFactors.GetStatFactorFromList(stat);
					if (Math.Abs(num2 - 1f) > float.Epsilon && curStage.statFactorEffectMultiplier != null)
					{
						num2 = ScaleFactor(num2, pawn.GetStatValue(curStage.statFactorEffectMultiplier));
					}
					if (Mathf.Abs(num) > 0f || Math.Abs(num2 - 1f) > float.Epsilon)
					{
						yield return new Dialog_InfoCard.Hyperlink(hediffs[i].def);
					}
				}
			}
			foreach (Thing item in RelevantGear(pawn, stat))
			{
				yield return new Dialog_InfoCard.Hyperlink(item);
			}
			if (stat.parts == null)
			{
				yield break;
			}
			foreach (StatPart part in stat.parts)
			{
				foreach (Dialog_InfoCard.Hyperlink infoCardHyperlink in part.GetInfoCardHyperlinks(statRequest))
				{
					yield return infoCardHyperlink;
				}
			}
		}

		public static float ScaleFactor(float factor, float scale)
		{
			return 1f - (1f - factor) * scale;
		}

		private static bool DisplayTradeStats(StatRequest req)
		{
			ThingDef thingDef;
			if ((thingDef = req.Def as ThingDef) == null)
			{
				return false;
			}
			if (req.HasThing && CompBiocodable.IsBiocoded(req.Thing))
			{
				return false;
			}
			if (thingDef.category == ThingCategory.Building && thingDef.Minifiable)
			{
				return true;
			}
			if (TradeUtility.EverPlayerSellable(thingDef))
			{
				return true;
			}
			if (thingDef.tradeability.TraderCanSell() && (thingDef.category == ThingCategory.Item || thingDef.category == ThingCategory.Pawn))
			{
				return true;
			}
			return false;
		}

		public void TryClearCache()
		{
			if (temporaryStatCache != null)
			{
				temporaryStatCache.Clear();
			}
			if (immutableStatCache != null)
			{
				immutableStatCache.Clear();
			}
		}

		public void DeleteStatCache()
		{
			temporaryStatCache = null;
			immutableStatCache = null;
		}
	}
}

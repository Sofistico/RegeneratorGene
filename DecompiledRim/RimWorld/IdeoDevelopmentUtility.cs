using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class IdeoDevelopmentUtility
	{
		private static readonly SimpleCurve AnytimeAndFuneralRitualDevelopmentPointsOverOutcomeIndex = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(1f, 0f),
			new CurvePoint(2f, 1f),
			new CurvePoint(3f, 2f)
		};

		private static readonly SimpleCurve[] DateRitualDevelopmentCurvesByCount = new SimpleCurve[5]
		{
			new SimpleCurve
			{
				new CurvePoint(0f, 2f),
				new CurvePoint(1f, 4f),
				new CurvePoint(2f, 6f),
				new CurvePoint(3f, 7f)
			},
			new SimpleCurve
			{
				new CurvePoint(0f, 1f),
				new CurvePoint(1f, 2f),
				new CurvePoint(2f, 3f),
				new CurvePoint(3f, 4f)
			},
			new SimpleCurve
			{
				new CurvePoint(0f, 1f),
				new CurvePoint(1f, 1f),
				new CurvePoint(2f, 2f),
				new CurvePoint(3f, 3f)
			},
			new SimpleCurve
			{
				new CurvePoint(0f, 0f),
				new CurvePoint(1f, 1f),
				new CurvePoint(2f, 1f),
				new CurvePoint(3f, 2f)
			},
			new SimpleCurve
			{
				new CurvePoint(0f, 0f),
				new CurvePoint(1f, 1f),
				new CurvePoint(2f, 1f),
				new CurvePoint(3f, 1f)
			}
		};

		private static List<Precept> toRemovePrecepts = new List<Precept>();

		public static int DevelopmentPointsForQuestSuccess(Ideo ideo, QuestScriptDef root)
		{
			int num = 0;
			if (root.successHistoryEvent == null || !ideo.Fluid)
			{
				return num;
			}
			List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
			for (int i = 0; i < preceptsListForReading.Count; i++)
			{
				for (int j = 0; j < preceptsListForReading[i].def.comps.Count; j++)
				{
					PreceptComp_DevelopmentPoints preceptComp_DevelopmentPoints;
					if ((preceptComp_DevelopmentPoints = preceptsListForReading[i].def.comps[j] as PreceptComp_DevelopmentPoints) != null && preceptComp_DevelopmentPoints.eventDef == root.successHistoryEvent)
					{
						num += preceptComp_DevelopmentPoints.points;
					}
				}
			}
			return num;
		}

		public static SimpleCurve GetDevelopmentPointsOverOutcomeIndexCurveForRitual(Ideo ideo, Precept_Ritual ritual)
		{
			RitualOutcomeEffectWorker_FromQuality ritualOutcomeEffectWorker_FromQuality;
			if ((ritualOutcomeEffectWorker_FromQuality = ritual.outcomeEffect as RitualOutcomeEffectWorker_FromQuality) == null || !ritualOutcomeEffectWorker_FromQuality.GivesDevelopmentPoints)
			{
				return null;
			}
			if (ritual.isAnytime || ritual.def == PreceptDefOf.Funeral)
			{
				return AnytimeAndFuneralRitualDevelopmentPointsOverOutcomeIndex;
			}
			if (ritual.IsDateTriggered)
			{
				Precept_Ritual precept_Ritual;
				int num = Mathf.Clamp(ideo.PreceptsListForReading.Count((Precept p) => (precept_Ritual = p as Precept_Ritual) != null && precept_Ritual.IsDateTriggered) - 1, 0, DateRitualDevelopmentCurvesByCount.Length - 1);
				return DateRitualDevelopmentCurvesByCount[num];
			}
			return null;
		}

		public static void GetAllRitualsThatGiveDevelopmentPoints(Ideo ideo, List<Precept_Ritual> rituals)
		{
			if (!ideo.Fluid)
			{
				return;
			}
			List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
			for (int i = 0; i < preceptsListForReading.Count; i++)
			{
				Precept_Ritual precept_Ritual;
				if ((precept_Ritual = preceptsListForReading[i] as Precept_Ritual) != null && GetDevelopmentPointsOverOutcomeIndexCurveForRitual(ideo, precept_Ritual) != null)
				{
					rituals.Add(precept_Ritual);
				}
			}
		}

		public static void GetAllQuestSuccessEventsThatGiveDevelopmentPoints(Ideo ideo, List<HistoryEventDef> successEvents)
		{
			if (!ideo.Fluid)
			{
				return;
			}
			foreach (QuestScriptDef allDef in DefDatabase<QuestScriptDef>.AllDefs)
			{
				if (!successEvents.Contains(allDef.successHistoryEvent) && DevelopmentPointsForQuestSuccess(ideo, allDef) > 0)
				{
					successEvents.Add(allDef.successHistoryEvent);
				}
			}
		}

		public static void ConfirmChangesToIdeo(Ideo ideo, Ideo newIdeo, Action confirmCallback)
		{
			toRemovePrecepts.Clear();
			GetPreceptsToRemove(ideo, newIdeo, toRemovePrecepts);
			string text = "";
			for (int i = 0; i < toRemovePrecepts.Count; i++)
			{
				if (toRemovePrecepts[i].TryGetLostByReformingWarning(out var warning))
				{
					text += warning;
				}
			}
			if (!text.NullOrEmpty())
			{
				text += "\n\n" + "ReformIdeoContinue".Translate();
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, confirmCallback));
			}
			else
			{
				confirmCallback();
			}
			toRemovePrecepts.Clear();
		}

		public static void ApplyChangesToIdeo(Ideo ideo, Ideo newIdeo)
		{
			toRemovePrecepts.Clear();
			GetPreceptsToRemove(ideo, newIdeo, toRemovePrecepts);
			ideo.development.Notify_PreReform(newIdeo);
			newIdeo.CopyTo(ideo);
			ideo.development.Notify_Reformed();
			for (int i = 0; i < toRemovePrecepts.Count; i++)
			{
				toRemovePrecepts[i].Notify_RemovedByReforming();
			}
			toRemovePrecepts.Clear();
		}

		private static void GetPreceptsToRemove(Ideo ideo, Ideo newIdeo, List<Precept> preceptsOut)
		{
			List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
			List<Precept> preceptsListForReading2 = newIdeo.PreceptsListForReading;
			for (int i = 0; i < preceptsListForReading.Count; i++)
			{
				bool flag = false;
				for (int j = 0; j < preceptsListForReading2.Count; j++)
				{
					if (preceptsListForReading2[j].Id == preceptsListForReading[i].Id)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					preceptsOut.Add(preceptsListForReading[i]);
				}
			}
		}
	}
}

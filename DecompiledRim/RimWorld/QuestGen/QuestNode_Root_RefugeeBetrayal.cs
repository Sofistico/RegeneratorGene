using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Root_RefugeeBetrayal : QuestNode
	{
		protected override void RunInt()
		{
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			Map map = QuestGen_Get.GetMap();
			List<Pawn> list = slate.Get<List<Pawn>>("lodgers");
			ExtraFaction extraFaction = slate.Get<ExtraFaction>("refugeeFaction");
			Pawn factionOpponent = slate.Get<Pawn>("factionOpponent");
			float num = (float)list.Count * 300f;
			FloatRange value = new FloatRange(0.7f, 1.3f) * num * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor;
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.totalMarketValueRange = value;
			parms.qualityGenerator = QualityGenerator.Reward;
			parms.makingFaction = extraFaction.faction;
			string inSignalRemovePawn = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Recruited");
			string item = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Destroyed");
			string item2 = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Kidnapped");
			string item3 = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.LeftMap");
			string item4 = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Banished");
			List<Thing> betrayalRewardThings = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
			quest.ExtraFaction(extraFaction.faction, list, ExtraFactionType.MiniFaction, areHelpers: false, inSignalRemovePawn);
			quest.BetrayalOffer(list, extraFaction, factionOpponent, delegate
			{
				float num2 = 0f;
				for (int i = 0; i < betrayalRewardThings.Count; i++)
				{
					num2 += betrayalRewardThings[i].MarketValue * (float)betrayalRewardThings[i].stackCount;
				}
				slate.Set("betrayalRewards", GenLabel.ThingsLabel(betrayalRewardThings));
				slate.Set("betrayalRewardMarketValue", num2);
				quest.DropPods(map.Parent, betrayalRewardThings, null, null, null, null, true, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, null, null, QuestPart.SignalListenMode.Always, null, destroyItemsOnCleanup: false);
				quest.FactionGoodwillChange(factionOpponent.Faction, 10, null, canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, null, QuestPart.SignalListenMode.Always);
				quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.Always, betrayalRewardThings, filterDeadPawnsFromLookTargets: false, "[betrayalOfferRewardLetterText]", null, "[betrayalOfferRewardLetterLabel]");
				quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.Always);
			}, delegate
			{
				quest.DestroyThingsOrPassToWorld(betrayalRewardThings, null, questLookTargets: true, QuestPart.SignalListenMode.Always);
				quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.Always, null, filterDeadPawnsFromLookTargets: false, "[betrayalOfferFailedLetterText]", null, "[betrayalOfferFailedLetterLabel]");
				quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.Always);
			}, null, new List<string> { item, item2, item3, item4 }, null, QuestPart.SignalListenMode.Always);
		}

		protected override bool TestRunInt(Slate slate)
		{
			if (slate.Get<Pawn>("rewardGiver") != null && slate.TryGet<FloatRange>("marketValueRange", out var _))
			{
				return slate.Get<Faction>("faction") != null;
			}
			return false;
		}
	}
}

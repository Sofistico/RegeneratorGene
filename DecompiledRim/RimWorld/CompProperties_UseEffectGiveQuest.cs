namespace RimWorld
{
	public class CompProperties_UseEffectGiveQuest : CompProperties_Usable
	{
		public QuestScriptDef quest;

		public bool sendLetterQuestAvailable = true;

		public bool? discovered;

		public CompProperties_UseEffectGiveQuest()
		{
			compClass = typeof(CompUseEffect_GiveQuest);
		}
	}
}

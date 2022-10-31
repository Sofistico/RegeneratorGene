using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ChoiceLetter_GrowthMoment : LetterWithTimeout
	{
		public Pawn pawn;

		public TaggedString text;

		public TaggedString mouseoverText;

		private int passionChoiceCount;

		public int traitChoiceCount;

		public int passionGainsCount;

		public List<SkillDef> passionChoices;

		public List<Trait> traitChoices;

		public Trait chosenTrait;

		public List<SkillDef> chosenPassions;

		public List<string> enabledWorkTypes;

		public bool choiceMade;

		public Name oldName;

		public int growthTier;

		public override bool CanDismissWithRightClick => false;

		public override bool CanShowInLetterStack
		{
			get
			{
				if (ArchiveView || base.TimeoutPassed)
				{
					return false;
				}
				return true;
			}
		}

		public bool ArchiveView
		{
			get
			{
				if (!choiceMade)
				{
					return pawn.DestroyedOrNull();
				}
				return true;
			}
		}

		public bool ShowInfoTabs
		{
			get
			{
				if (!ArchiveView)
				{
					if (passionChoices.NullOrEmpty())
					{
						return !traitChoices.NullOrEmpty();
					}
					return true;
				}
				return false;
			}
		}

		public void ConfigureGrowthLetter(Pawn pawn, int passionChoiceCount, int traitChoiceCount, int passionGainsCount, List<string> enabledWorkTypes, Name oldName)
		{
			this.pawn = pawn;
			this.passionChoiceCount = passionChoiceCount;
			this.passionGainsCount = passionGainsCount;
			this.traitChoiceCount = traitChoiceCount;
			this.enabledWorkTypes = enabledWorkTypes;
			this.oldName = oldName;
			growthTier = pawn.ageTracker.GrowthTier;
			if (passionGainsCount > passionChoiceCount)
			{
				Log.Error("ConfigureGrowthLetter: passionGainsCount > passionChoiceCount.");
				passionGainsCount = passionChoiceCount;
			}
			CacheLetterText();
		}

		private void CacheLetterText()
		{
			text = "BirthdayBiologicalGrowthMoment".Translate(pawn, pawn.ageTracker.AgeBiologicalYears);
			if (pawn.ageTracker.AgeBiologicalYears >= GrowthUtility.GrowthMomentAges[0])
			{
				text += "\n\n" + GrowthUtility.GrowthFlavorForTier(pawn, growthTier);
			}
			if (!enabledWorkTypes.NullOrEmpty())
			{
				text += "\n\n" + "BirthdayBiologicalAgeWorkTypes".Translate(pawn, pawn.ageTracker.AgeBiologicalYears) + ":\n" + enabledWorkTypes.ToLineList("  - ");
			}
			mouseoverText = text;
			if (pawn.Name != oldName)
			{
				TaggedString taggedString = "BirthdayNickname".Translate(oldName.ToStringFull.Colorize(ColoredText.NameColor), pawn.LabelShort.Colorize(ColoredText.NameColor));
				mouseoverText += "\n\n" + taggedString;
			}
			if (passionChoiceCount > 0 || traitChoiceCount > 0)
			{
				mouseoverText += "\n\n" + "BirthdayChooseHowPawnWillGrow".Translate(pawn);
			}
		}

		public override void OpenLetter()
		{
			TrySetChoices();
			Dialog_GrowthMomentChoices window = new Dialog_GrowthMomentChoices(text, this);
			Find.WindowStack.Add(window);
		}

		protected override string GetMouseoverText()
		{
			return mouseoverText.Resolve();
		}

		public void MakeChoices(List<SkillDef> skills, Trait trait)
		{
			if (ArchiveView)
			{
				return;
			}
			choiceMade = true;
			chosenPassions = skills;
			chosenTrait = trait;
			if (!skills.NullOrEmpty())
			{
				foreach (SkillDef skill2 in skills)
				{
					SkillRecord skill = pawn.skills.GetSkill(skill2);
					if (skill.passion == Passion.Major)
					{
						Log.Warning($"{pawn?.LabelShort} Tried to upgrade a passion for {skill2} but it's already major");
						return;
					}
					skill.passion = skill.passion.IncrementPassion();
				}
			}
			if (trait != null)
			{
				pawn.story.traits.GainTrait(trait);
			}
			if (pawn.ageTracker.AgeBiologicalYears == 13)
			{
				PawnGenerator.TryGenerateSexualityTraitFor(pawn, allowGay: true);
			}
			pawn.ageTracker.growthPoints = 0f;
		}

		public static List<SkillDef> PassionOptions(Pawn pawn, int count)
		{
			return DefDatabase<SkillDef>.AllDefsListForReading.Where((SkillDef s) => IsValidGrowthPassionOption(pawn, s)).InRandomOrder().Take(count)
				.ToList();
		}

		private void TrySetChoices()
		{
			if (!choiceMade && !pawn.DestroyedOrNull())
			{
				if (passionChoiceCount > 0 && passionChoices == null)
				{
					passionChoices = PassionOptions(pawn, passionChoiceCount);
				}
				if (traitChoiceCount > 0 && traitChoices == null)
				{
					traitChoices = PawnGenerator.GenerateTraitsFor(pawn, traitChoiceCount, null, growthMomentTrait: true);
				}
			}
		}

		private static bool IsValidGrowthPassionOption(Pawn pawn, SkillDef skill)
		{
			SkillRecord skill2 = pawn.skills.GetSkill(skill);
			if (!skill2.PermanentlyDisabled)
			{
				return skill2.passion != Passion.Major;
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref pawn, "pawn");
			Scribe_Collections.Look(ref traitChoices, "traitChoices", LookMode.Deep);
			Scribe_Values.Look(ref growthTier, "growthTier", 0);
			Scribe_Collections.Look(ref passionChoices, "passionChoices", LookMode.Undefined);
			Scribe_Collections.Look(ref enabledWorkTypes, "enabledWorkTypes", LookMode.Undefined);
			Scribe_Collections.Look(ref chosenPassions, "chosenPassions", LookMode.Def);
			Scribe_Deep.Look(ref chosenTrait, "chosenTrait");
			Scribe_Values.Look(ref passionChoiceCount, "passionChoiceCount", 0);
			Scribe_Values.Look(ref passionGainsCount, "passionGainsCount", 0);
			Scribe_Values.Look(ref traitChoiceCount, "traitChoiceCount", 0);
			Scribe_Values.Look(ref text, "text");
			Scribe_Values.Look(ref mouseoverText, "mouseoverText");
			Scribe_Values.Look(ref choiceMade, "choiceMade", defaultValue: false);
			Scribe_Deep.Look(ref oldName, "oldName");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				traitChoices?.RemoveAll((Trait x) => x == null);
				traitChoices?.RemoveAll((Trait x) => x.def == null);
				passionChoices?.RemoveAll((SkillDef x) => x == null);
			}
		}
	}
}

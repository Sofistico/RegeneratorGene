using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ChoiceLetter_BabyBirth : ChoiceLetter
	{
		private Pawn pawn;

		public override bool CanShowInLetterStack
		{
			get
			{
				Pawn obj = pawn;
				if (obj != null && obj.Faction?.IsPlayer == true)
				{
					Pawn obj2 = pawn;
					if (obj2 != null && obj2.babyNamingDeadline >= 0)
					{
						return base.CanShowInLetterStack;
					}
				}
				return false;
			}
		}

		public override bool ShouldAutomaticallyOpenLetter
		{
			get
			{
				Pawn obj = pawn;
				if (obj != null && obj.babyNamingDeadline >= 0)
				{
					return base.ShouldAutomaticallyOpenLetter;
				}
				return false;
			}
		}

		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				if (!base.ArchivedOnly && (pawn.Faction?.IsPlayer ?? false))
				{
					yield return new DiaOption("NameBaby".Translate().CapitalizeFirst())
					{
						action = Rename,
						disabled = (pawn.babyNamingDeadline < Find.TickManager.TicksGame),
						disabledReason = "BabyAlreadyNamed".Translate(pawn),
						resolveTree = true
					};
				}
				if (base.ArchivedOnly)
				{
					yield return base.Option_JumpToLocation;
				}
				if (base.ArchivedOnly)
				{
					yield return base.Option_Close;
				}
				else if (LastTickBeforeTimeout)
				{
					if (pawn.babyNamingDeadline < 0)
					{
						yield return base.Option_Close;
					}
				}
				else if (pawn.babyNamingDeadline < Find.TickManager.TicksGame)
				{
					yield return base.Option_JumpToLocationAndPostpone;
					yield return base.Option_Close;
				}
				else
				{
					yield return base.Option_JumpToLocationAndPostpone;
					yield return base.Option_Postpone;
				}
			}
		}

		public void Start()
		{
			pawn = lookTargets.TryGetPrimaryTarget().Thing as Pawn;
			if (pawn.babyNamingDeadline >= Find.TickManager.TicksGame)
			{
				StartTimeout(pawn.babyNamingDeadline - Find.TickManager.TicksGame);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
			if (Scribe.mode != LoadSaveMode.Saving && pawn == null)
			{
				pawn = lookTargets.TryGetPrimaryTarget().Thing as Pawn;
			}
		}

		private void Rename()
		{
			string initialFirstNameOverride = null;
			NameTriple nameTriple;
			if ((nameTriple = pawn.Name as NameTriple) != null && nameTriple.First == (string)"Baby".Translate().CapitalizeFirst())
			{
				Rand.PushState();
				Name name;
				try
				{
					Rand.Seed = pawn.thingIDNumber;
					name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full, null, forceNoNick: false, pawn.genes?.Xenotype);
				}
				finally
				{
					Rand.PopState();
				}
				NameTriple nameTriple2;
				initialFirstNameOverride = (((nameTriple2 = name as NameTriple) != null) ? nameTriple2.First : ((NameSingle)name).Name);
			}
			Find.WindowStack.Add(pawn.NamePawnDialog(initialFirstNameOverride));
		}
	}
}

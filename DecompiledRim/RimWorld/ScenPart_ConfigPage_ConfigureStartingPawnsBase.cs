using Verse;

namespace RimWorld
{
	public abstract class ScenPart_ConfigPage_ConfigureStartingPawnsBase : ScenPart_ConfigPage
	{
		public int pawnChoiceCount = 10;

		protected const int MaxPawnCount = 10;

		protected abstract int TotalPawnCount { get; }

		protected abstract void GenerateStartingPawns();

		public override void PostIdeoChosen()
		{
			Find.GameInitData.startingPawnCount = TotalPawnCount;
			if (ModsConfig.IdeologyActive && Faction.OfPlayerSilentFail?.ideos?.PrimaryIdeo != null)
			{
				foreach (Precept item in Faction.OfPlayerSilentFail.ideos.PrimaryIdeo.PreceptsListForReading)
				{
					if (item.def.defaultDrugPolicyOverride != null)
					{
						Current.Game.drugPolicyDatabase.MakePolicyDefault(item.def.defaultDrugPolicyOverride);
					}
				}
			}
			GenerateStartingPawns();
			while (Find.GameInitData.startingAndOptionalPawns.Count < pawnChoiceCount)
			{
				StartingPawnUtility.AddNewPawn();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref pawnChoiceCount, "pawnChoiceCount", 0);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ pawnChoiceCount;
		}
	}
}

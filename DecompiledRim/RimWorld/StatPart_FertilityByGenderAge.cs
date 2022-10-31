using Verse;

namespace RimWorld
{
	public class StatPart_FertilityByGenderAge : StatPart
	{
		protected SimpleCurve maleFertilityAgeFactor;

		protected SimpleCurve femaleFertilityAgeFactor;

		public override string ExplanationPart(StatRequest req)
		{
			Pawn pawn;
			if ((pawn = req.Thing as Pawn) != null && pawn != null)
			{
				return "StatsReport_FertilityAgeFactor".Translate() + ": x" + AgeFactor(pawn).ToStringPercent();
			}
			return null;
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			Pawn pawn;
			if ((pawn = req.Thing as Pawn) != null && pawn != null)
			{
				val *= AgeFactor(pawn);
			}
		}

		private float AgeFactor(Pawn pawn)
		{
			return ((pawn.gender == Gender.Female) ? femaleFertilityAgeFactor : maleFertilityAgeFactor).Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
		}
	}
}

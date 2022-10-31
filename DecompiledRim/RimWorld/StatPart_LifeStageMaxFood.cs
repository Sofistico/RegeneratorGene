using Verse;

namespace RimWorld
{
	public class StatPart_LifeStageMaxFood : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			Pawn pawn;
			if (req.HasThing && (pawn = req.Thing as Pawn) != null)
			{
				val *= pawn.ageTracker.CurLifeStage.foodMaxFactor;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			Pawn pawn;
			if (req.HasThing && (pawn = req.Thing as Pawn) != null)
			{
				return "LifeStageMaxFood".Translate() + ": x" + pawn.ageTracker.CurLifeStage.foodMaxFactor.ToStringPercent();
			}
			return null;
		}
	}
}

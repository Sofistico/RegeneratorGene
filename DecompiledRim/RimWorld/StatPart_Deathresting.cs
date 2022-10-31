using Verse;

namespace RimWorld
{
	public class StatPart_Deathresting : StatPart
	{
		public float factor;

		public override void TransformValue(StatRequest req, ref float val)
		{
			Pawn pawn;
			if (req.HasThing && (pawn = req.Thing as Pawn) != null && pawn.Deathresting)
			{
				val *= factor;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			Pawn pawn;
			if (req.HasThing && (pawn = req.Thing as Pawn) != null && pawn.Deathresting)
			{
				return "Deathresting".Translate().CapitalizeFirst() + ": x" + factor.ToStringPercent();
			}
			return null;
		}
	}
}

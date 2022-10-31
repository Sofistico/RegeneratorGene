using Verse;

namespace RimWorld
{
	public class StatPart_Pain : StatPart
	{
		private float factor = 1f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			Pawn pawn;
			if ((pawn = req.Thing as Pawn) != null)
			{
				val *= PainFactor(pawn);
			}
		}

		public float PainFactor(Pawn pawn)
		{
			return 1f + pawn.health.hediffSet.PainTotal * factor;
		}

		public override string ExplanationPart(StatRequest req)
		{
			Pawn pawn;
			if (req.HasThing && (pawn = req.Thing as Pawn) != null)
			{
				return "StatsReport_Pain".Translate() + (": " + PainFactor(pawn).ToStringPercent("F0"));
			}
			return null;
		}
	}
}

using Verse;

namespace RimWorld
{
	public abstract class StatPart_BiostatTotal : StatPart_Curve
	{
		public abstract string ExplanationLabelBase { get; }

		protected override bool AppliesTo(StatRequest req)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			Pawn pawn;
			if (req.HasThing && (pawn = req.Thing as Pawn) != null)
			{
				return pawn.genes != null;
			}
			return false;
		}

		protected override string ExplanationLabel(StatRequest req)
		{
			return ExplanationLabelBase + " " + CurveXGetter(req).ToStringByStyle(ToStringStyle.Integer);
		}
	}
}

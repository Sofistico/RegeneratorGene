using Verse;

namespace RimWorld
{
	public class StatWorker_Mechanitor : StatWorker
	{
		public override bool ShouldShowFor(StatRequest req)
		{
			if (!base.ShouldShowFor(req))
			{
				return false;
			}
			Pawn pawn;
			if (req.Thing != null && (pawn = req.Thing as Pawn) != null)
			{
				return MechanitorUtility.IsMechanitor(pawn);
			}
			return false;
		}
	}
}

using Verse;

namespace RimWorld
{
	public class StatWorker_Terror : StatWorker
	{
		public override bool ShouldShowFor(StatRequest req)
		{
			if (!base.ShouldShowFor(req))
			{
				return false;
			}
			Pawn pawn;
			if ((pawn = req.Thing as Pawn) == null)
			{
				return false;
			}
			return pawn.IsSlave;
		}

		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			return ((Pawn)req.Thing).GetTerrorLevel();
		}
	}
}

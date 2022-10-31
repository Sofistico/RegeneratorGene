using Verse;

namespace RimWorld
{
	public class PreceptWorker_Building : PreceptWorker
	{
		public override AcceptanceReport CanUse(ThingDef def, Ideo ideo, FactionDef generatingFor)
		{
			bool flag = false;
			foreach (Precept item in ideo.PreceptsListForReading)
			{
				Precept_Building precept_Building;
				if ((precept_Building = item as Precept_Building) != null && precept_Building.ThingDef != null && precept_Building.ThingDef.isAltar)
				{
					flag = true;
					break;
				}
			}
			if (flag && def.isAltar)
			{
				return new AcceptanceReport("IdeoAlreadyHasAltar".Translate());
			}
			if (!flag)
			{
				return def.isAltar;
			}
			if (!def.isAltar)
			{
				return true;
			}
			if (flag && def.isAltar)
			{
				return false;
			}
			return true;
		}
	}
}

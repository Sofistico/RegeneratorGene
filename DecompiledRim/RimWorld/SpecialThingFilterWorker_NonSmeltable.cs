using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_NonSmeltable : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (!CanEverMatch(t.def))
			{
				return false;
			}
			return !t.Smeltable;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			if (!def.smeltable)
			{
				return true;
			}
			if (def.MadeFromStuff)
			{
				foreach (ThingDef item in GenStuff.AllowedStuffsFor(def))
				{
					if (!item.smeltable)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override bool AlwaysMatches(ThingDef def)
		{
			return CanEverMatch(def);
		}
	}
}

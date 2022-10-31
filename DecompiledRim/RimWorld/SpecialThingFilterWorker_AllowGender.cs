using Verse;

namespace RimWorld
{
	public abstract class SpecialThingFilterWorker_AllowGender : SpecialThingFilterWorker
	{
		private Gender targetGender;

		protected SpecialThingFilterWorker_AllowGender(Gender targetGender)
		{
			this.targetGender = targetGender;
		}

		public override bool Matches(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn == null)
			{
				return false;
			}
			return pawn.gender == targetGender;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return def.category == ThingCategory.Pawn;
		}
	}
}

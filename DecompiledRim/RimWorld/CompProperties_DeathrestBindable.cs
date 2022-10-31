using Verse;

namespace RimWorld
{
	public class CompProperties_DeathrestBindable : CompProperties
	{
		public int stackLimit;

		public bool countsTowardsBuildingLimit = true;

		public bool displayTimeActive = true;

		public float powerUsageDeathresting;

		public float powerUsageNotDeathresting;

		public float deathrestEffectivenessFactor = 1f;

		public bool mustBeLayingInToBind;

		public float hemogenLimitOffset;

		public HediffDef hediffToApply;

		public SoundDef soundWorking;

		public SoundDef soundStart;

		public SoundDef soundEnd;

		public CompProperties_DeathrestBindable()
		{
			compClass = typeof(CompDeathrestBindable);
		}
	}
}

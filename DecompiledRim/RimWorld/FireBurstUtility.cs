using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class FireBurstUtility
	{
		private const float FuelSpawnChancePerCastingTick = 0.15f;

		public static void ThrowFuelTick(IntVec3 position, float radius, Map map)
		{
			if (!Rand.Chance(0.15f))
			{
				return;
			}
			using IEnumerator<IntVec3> enumerator = GenRadial.RadialCellsAround(position, radius, useCenter: true).InRandomOrder().GetEnumerator();
			while (enumerator.MoveNext() && !FilthMaker.TryMakeFilth(enumerator.Current, map, ThingDefOf.Filth_Fuel))
			{
			}
		}
	}
}

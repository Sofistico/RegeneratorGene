using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_Nursery : RoomRoleWorker
	{
		private const int MinBabyBeds = 2;

		public override float GetScore(Room room)
		{
			if (!ModsConfig.BiotechActive)
			{
				return 0f;
			}
			int num = 0;
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				Building_Bed building_Bed;
				if ((building_Bed = containedAndAdjacentThings[i] as Building_Bed) != null && building_Bed.def.building.bed_humanlike && !building_Bed.Medical)
				{
					if (building_Bed.ForPrisoners || building_Bed.def.building.bed_maxBodySize >= LifeStageDefOf.HumanlikeChild.bodySizeFactor)
					{
						return 0f;
					}
					num++;
				}
			}
			if (num < 2)
			{
				return 0f;
			}
			return (float)num * 100200f;
		}
	}
}

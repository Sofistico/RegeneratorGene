using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_AddShipJob_Wait : QuestNode_AddShipJob
	{
		public SlateRef<int> ticks;

		public SlateRef<bool> leaveImmediatelyWhenSatisfied;

		public SlateRef<List<Thing>> sendAwayIfAllDespawned;

		protected override void AddJobVars(ShipJob shipJob, Slate slate)
		{
			ShipJob_Wait shipJob_Wait;
			if ((shipJob_Wait = shipJob as ShipJob_Wait) != null)
			{
				shipJob_Wait.leaveImmediatelyWhenSatisfied = leaveImmediatelyWhenSatisfied.GetValue(slate);
				shipJob_Wait.sendAwayIfAllDespawned = sendAwayIfAllDespawned.GetValue(slate);
			}
			ShipJob_WaitTime shipJob_WaitTime;
			if ((shipJob_WaitTime = shipJob as ShipJob_WaitTime) != null)
			{
				shipJob_WaitTime.duration = ticks.GetValue(slate);
			}
		}
	}
}

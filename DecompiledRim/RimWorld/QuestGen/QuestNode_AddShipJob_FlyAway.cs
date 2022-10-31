using RimWorld.Planet;

namespace RimWorld.QuestGen
{
	public class QuestNode_AddShipJob_FlyAway : QuestNode_AddShipJob
	{
		public SlateRef<MapParent> destination;

		public SlateRef<TransportShipDropMode?> unsatisfiedDropMode;

		protected override ShipJobDef DefaultShipJobDef => ShipJobDefOf.FlyAway;

		protected override void AddJobVars(ShipJob shipJob, Slate slate)
		{
			ShipJob_FlyAway shipJob_FlyAway;
			if ((shipJob_FlyAway = shipJob as ShipJob_FlyAway) != null)
			{
				MapParent value = destination.GetValue(slate);
				if (value != null)
				{
					shipJob_FlyAway.destinationTile = value.Tile;
				}
				shipJob_FlyAway.dropMode = unsatisfiedDropMode.GetValue(slate) ?? TransportShipDropMode.NonRequired;
			}
		}
	}
}

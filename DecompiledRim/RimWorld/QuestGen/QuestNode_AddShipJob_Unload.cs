namespace RimWorld.QuestGen
{
	public class QuestNode_AddShipJob_Unload : QuestNode_AddShipJob
	{
		public SlateRef<TransportShipDropMode?> dropMode;

		protected override ShipJobDef DefaultShipJobDef => ShipJobDefOf.Unload;

		protected override void AddJobVars(ShipJob shipJob, Slate slate)
		{
			ShipJob_Unload shipJob_Unload;
			if ((shipJob_Unload = shipJob as ShipJob_Unload) != null)
			{
				shipJob_Unload.dropMode = dropMode.GetValue(slate) ?? TransportShipDropMode.All;
			}
		}
	}
}

namespace RimWorld
{
	public class MainButtonWorker_ToggleMechTab : MainButtonWorker_ToggleTab
	{
		public override bool Disabled => !MechanitorUtility.AnyMechsInPlayerFaction();

		public override bool Visible => !Disabled;
	}
}

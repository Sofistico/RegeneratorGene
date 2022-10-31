using System.Collections.Generic;

namespace Verse.AI.Group
{
	public class Trigger_OnPlayerMechHarmAnything : Trigger
	{
		private List<Thing> things;

		public Trigger_OnPlayerMechHarmAnything(List<Thing> things)
		{
			this.things = things;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			Pawn pawn;
			if (ModsConfig.BiotechActive && signal.dinfo.Def != null && signal.dinfo.Def.ExternalViolenceFor(signal.thing) && signal.dinfo.Instigator != null && (pawn = signal.dinfo.Instigator as Pawn) != null && pawn.IsColonyMech && things.Contains(signal.thing))
			{
				return true;
			}
			return false;
		}
	}
}

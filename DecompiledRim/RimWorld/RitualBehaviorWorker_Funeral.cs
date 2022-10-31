using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualBehaviorWorker_Funeral : RitualBehaviorWorker
	{
		public RitualBehaviorWorker_Funeral()
		{
		}

		public RitualBehaviorWorker_Funeral(RitualBehaviorDef def)
			: base(def)
		{
		}

		public override string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
		{
			Building_Grave building_Grave;
			if (target.HasThing && (building_Grave = target.Thing as Building_Grave) != null && building_Grave.Corpse != null && building_Grave.Corpse.InnerPawn.IsSlave)
			{
				return "CantStartFuneralForSlave".Translate(building_Grave.Corpse.InnerPawn);
			}
			return base.CanStartRitualNow(target, ritual, selectedPawn, forcedForRole);
		}
	}
}

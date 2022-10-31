using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class RitualObligationTargetWorker_IdeoBuildingOrRitualSpot : RitualObligationTargetFilter
	{
		public RitualObligationTargetWorker_IdeoBuildingOrRitualSpot()
		{
		}

		public RitualObligationTargetWorker_IdeoBuildingOrRitualSpot(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
		{
			return Enumerable.Empty<TargetInfo>();
		}

		protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
		{
			if (!ModLister.CheckIdeology("Ideo building target"))
			{
				return false;
			}
			Building building = target.Thing as Building;
			if (building == null || building.Faction == null || !building.Faction.IsPlayer)
			{
				return false;
			}
			if (building.def == ThingDefOf.RitualSpot)
			{
				return true;
			}
			for (int i = 0; i < parent.ideo.PreceptsListForReading.Count; i++)
			{
				Precept_Building precept_Building = parent.ideo.PreceptsListForReading[i] as Precept_Building;
				if (precept_Building != null && building.def == precept_Building.ThingDef)
				{
					return true;
				}
			}
			if (building.TryGetComp<CompGatherSpot>() != null)
			{
				return true;
			}
			return false;
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			for (int i = 0; i < parent.ideo.PreceptsListForReading.Count; i++)
			{
				Precept_Building precept_Building = parent.ideo.PreceptsListForReading[i] as Precept_Building;
				if (precept_Building != null)
				{
					yield return precept_Building.LabelCap;
				}
			}
			yield return ThingDefOf.RitualSpot.label;
			yield return "RitualTargetGatherSpotInfo".Translate();
		}

		public override List<string> MissingTargetBuilding(Ideo ideo)
		{
			return null;
		}
	}
}

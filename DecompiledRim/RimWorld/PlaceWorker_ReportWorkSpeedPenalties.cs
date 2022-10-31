using Verse;

namespace RimWorld
{
	public class PlaceWorker_ReportWorkSpeedPenalties : PlaceWorker
	{
		public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
		{
			ThingDef thingDef = def as ThingDef;
			if (thingDef == null)
			{
				return;
			}
			bool flag = StatPart_WorkTableOutdoors.Applies(thingDef, map, loc);
			bool flag2 = StatPart_WorkTableTemperature.Applies(thingDef, map, loc);
			if (!(flag || flag2))
			{
				return;
			}
			string text = "WillGetWorkSpeedPenalty".Translate(def.label).CapitalizeFirst() + ": ";
			string text2 = "";
			if (flag)
			{
				text2 += "Outdoors".Translate().CapitalizeFirst();
			}
			if (flag2)
			{
				if (!text2.NullOrEmpty())
				{
					text2 += ", ";
				}
				text2 += "BadTemperature".Translate();
			}
			Messages.Message(string.Concat(text + text2.CapitalizeFirst(), "."), new TargetInfo(loc, map), MessageTypeDefOf.CautionInput, historical: false);
		}
	}
}

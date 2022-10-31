using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_SmokeCloudMaker : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			GenDraw.DrawCircleOutline(center.ToVector3Shifted(), def.GetCompProperties<CompProperties_SmokeCloudMaker>().cloudRadius);
		}
	}
}

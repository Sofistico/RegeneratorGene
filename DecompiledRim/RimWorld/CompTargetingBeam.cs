using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompTargetingBeam : ThingComp
	{
		private static Material LaserLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

		private const float LineWidth = 0.1f;

		private CompProperties_TargetingBeam Props => (CompProperties_TargetingBeam)props;

		public override void PostDraw()
		{
			base.PostDraw();
			Pawn pawn;
			Stance_Warmup stance_Warmup;
			if (parent != null && (pawn = parent as Pawn) != null && pawn.stances != null && (stance_Warmup = pawn.stances.curStance as Stance_Warmup) != null && stance_Warmup.verb != null && stance_Warmup.verb is Verb_Shoot)
			{
				GenDraw.DrawLineBetween(parent.TrueCenter(), stance_Warmup.verb.CurrentTarget.CenterVector3, LaserLineMat, 0.1f);
			}
		}
	}
}

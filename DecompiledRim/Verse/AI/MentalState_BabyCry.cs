using RimWorld;
using UnityEngine;

namespace Verse.AI
{
	public class MentalState_BabyCry : MentalState_BabyFit
	{
		private int ticksUntilLeftTear;

		private int ticksUntilRightTear;

		private const int TicksBetweenTearDots = 35;

		private static readonly IntRange TicksBetweenTears = new IntRange(25, 40);

		private const float speed = 0.66f;

		private static readonly FloatRange randAngle = new FloatRange(10f, 30f);

		private static readonly FloatRange randScale = new FloatRange(0.6f, 1f);

		public override void MentalStateTick()
		{
			base.MentalStateTick();
			float num = base.pawn.Drawer.renderer.BodyAngle();
			Pawn pawn;
			if ((pawn = base.pawn.SpawnedParentOrMe as Pawn) != null && !pawn.Position.Fogged(pawn.Map))
			{
				FleckCreationData fleckData;
				if (--ticksUntilLeftTear <= 0)
				{
					FleckManager flecks = pawn.Map.flecks;
					fleckData = new FleckCreationData
					{
						spawnPosition = base.pawn.DrawPosHeld.Value + new Vector3(-0.15f, 0f, 0.066f).RotatedBy(num),
						velocitySpeed = -0.66f,
						velocityAngle = 90f + num - randAngle.RandomInRange,
						def = FleckDefOf.FleckBabyCrying,
						scale = randScale.RandomInRange
					};
					flecks.CreateFleck(fleckData);
					ticksUntilLeftTear = TicksBetweenTears.RandomInRange;
				}
				if (--ticksUntilRightTear <= 0)
				{
					FleckManager flecks2 = pawn.Map.flecks;
					fleckData = new FleckCreationData
					{
						spawnPosition = base.pawn.DrawPosHeld.Value + new Vector3(0.15f, 0f, 0.066f).RotatedBy(num),
						velocitySpeed = 0.66f,
						velocityAngle = 90f + num + randAngle.RandomInRange,
						def = FleckDefOf.FleckBabyCrying,
						scale = randScale.RandomInRange,
						exactScale = new Vector3(-1f, 1f, 1f)
					};
					flecks2.CreateFleck(fleckData);
					ticksUntilRightTear = TicksBetweenTears.RandomInRange;
				}
				if (base.pawn.IsHashIntervalTick(35))
				{
					MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_BabyCryingDots, new Vector3(0.27f, 0f, 0.066f).RotatedBy(num)).exactRotation = Rand.Value * 180f;
					MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_BabyCryingDots, new Vector3(-0.27f, 0f, 0.066f).RotatedBy(num)).exactRotation = Rand.Value * 180f;
				}
			}
		}

		protected override void AuraEffect(Thing source, Pawn hearer)
		{
			hearer.HearClamor(source, ClamorDefOf.BabyCry);
			Pawn otherPawn;
			if ((otherPawn = source as Pawn) != null && hearer.needs.mood != null)
			{
				if (hearer == otherPawn.GetMother() || hearer == otherPawn.GetFather())
				{
					hearer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MyCryingBaby, otherPawn);
				}
				else
				{
					hearer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CryingBaby, otherPawn);
				}
				hearer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BabyCriedSocial, otherPawn);
			}
		}
	}
}

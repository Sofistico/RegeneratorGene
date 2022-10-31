using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnAttackGizmoUtility
	{
		public static IEnumerable<Gizmo> GetAttackGizmos(Pawn pawn)
		{
			if (ShouldUseMeleeAttackGizmo(pawn))
			{
				yield return GetMeleeAttackGizmo(pawn);
			}
			if (ShouldUseSquadAttackGizmo())
			{
				yield return GetSquadAttackGizmo(pawn);
			}
		}

		public static bool CanShowEquipmentGizmos()
		{
			return !AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons();
		}

		private static bool ShouldUseSquadAttackGizmo()
		{
			if (AtLeastOneSelectedPlayerPawnHasRangedWeapon())
			{
				return AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons();
			}
			return false;
		}

		private static bool CanOrderPlayerPawn(Pawn pawn)
		{
			if (!pawn.IsColonistPlayerControlled)
			{
				return pawn.IsColonyMechPlayerControlled;
			}
			return true;
		}

		private static Gizmo GetSquadAttackGizmo(Pawn pawn)
		{
			Command_Target command_Target = new Command_Target();
			command_Target.defaultLabel = "CommandSquadAttack".Translate();
			command_Target.defaultDesc = "CommandSquadAttackDesc".Translate();
			command_Target.hotKey = KeyBindingDefOf.Misc1;
			command_Target.icon = TexCommand.SquadAttack;
			command_Target.targetingParams = TargetingParameters.ForAttackAny();
			command_Target.targetingParams.canTargetLocations = AllSelectedPlayerPawnsCanTargetLocations();
			if (FloatMenuUtility.GetAttackAction(pawn, LocalTargetInfo.Invalid, out var failStr) == null)
			{
				command_Target.Disable(failStr.CapitalizeFirst() + ".");
			}
			command_Target.action = delegate(LocalTargetInfo target)
			{
				foreach (Pawn item in Find.Selector.SelectedObjects.Where(delegate(object x)
				{
					Pawn pawn2 = x as Pawn;
					return pawn2 != null && CanOrderPlayerPawn(pawn2) && pawn2.Drafted;
				}).Cast<Pawn>())
				{
					string failStr2;
					Action attackAction = FloatMenuUtility.GetAttackAction(item, target, out failStr2);
					if (attackAction != null)
					{
						attackAction();
					}
					else if (!failStr2.NullOrEmpty())
					{
						Messages.Message(failStr2, target.Thing, MessageTypeDefOf.RejectInput, historical: false);
					}
				}
			};
			return command_Target;
		}

		private static bool ShouldUseMeleeAttackGizmo(Pawn pawn)
		{
			if (!pawn.Drafted)
			{
				return false;
			}
			if (!AtLeastOneSelectedPlayerPawnHasRangedWeapon() && !AtLeastOneSelectedPlayerPawnHasNoWeapon())
			{
				return AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons();
			}
			return true;
		}

		private static Gizmo GetMeleeAttackGizmo(Pawn pawn)
		{
			Command_Target command_Target = new Command_Target();
			command_Target.defaultLabel = "CommandMeleeAttack".Translate();
			command_Target.defaultDesc = "CommandMeleeAttackDesc".Translate();
			command_Target.targetingParams = TargetingParameters.ForAttackAny();
			command_Target.hotKey = KeyBindingDefOf.Misc2;
			command_Target.icon = TexCommand.AttackMelee;
			if (FloatMenuUtility.GetMeleeAttackAction(pawn, LocalTargetInfo.Invalid, out var failStr) == null)
			{
				command_Target.Disable(failStr.CapitalizeFirst() + ".");
			}
			command_Target.action = delegate(LocalTargetInfo target)
			{
				foreach (Pawn item in Find.Selector.SelectedObjects.Where(delegate(object x)
				{
					Pawn pawn2 = x as Pawn;
					return pawn2 != null && CanOrderPlayerPawn(pawn2) && pawn2.Drafted;
				}).Cast<Pawn>())
				{
					string failStr2;
					Action meleeAttackAction = FloatMenuUtility.GetMeleeAttackAction(item, target, out failStr2);
					if (meleeAttackAction != null)
					{
						meleeAttackAction();
					}
					else if (!failStr2.NullOrEmpty())
					{
						Messages.Message(failStr2, target.Thing, MessageTypeDefOf.RejectInput, historical: false);
					}
				}
			};
			return command_Target;
		}

		private static bool AtLeastOneSelectedPlayerPawnHasRangedWeapon()
		{
			List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
			for (int i = 0; i < selectedObjectsListForReading.Count; i++)
			{
				Pawn pawn = selectedObjectsListForReading[i] as Pawn;
				if (pawn != null && CanOrderPlayerPawn(pawn) && pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
				{
					return true;
				}
			}
			return false;
		}

		private static bool AtLeastOneSelectedPlayerPawnHasNoWeapon()
		{
			List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
			for (int i = 0; i < selectedObjectsListForReading.Count; i++)
			{
				Pawn pawn = selectedObjectsListForReading[i] as Pawn;
				if (pawn != null && CanOrderPlayerPawn(pawn) && (pawn.equipment == null || pawn.equipment.Primary == null))
				{
					return true;
				}
			}
			return false;
		}

		private static bool AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons()
		{
			if (Find.Selector.NumSelected <= 1)
			{
				return false;
			}
			ThingDef thingDef = null;
			bool flag = false;
			List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
			for (int i = 0; i < selectedObjectsListForReading.Count; i++)
			{
				Pawn pawn = selectedObjectsListForReading[i] as Pawn;
				if (pawn != null && CanOrderPlayerPawn(pawn))
				{
					ThingDef thingDef2 = ((pawn.equipment != null && pawn.equipment.Primary != null) ? pawn.equipment.Primary.def : null);
					if (!flag)
					{
						thingDef = thingDef2;
						flag = true;
					}
					else if (thingDef2 != thingDef)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool AllSelectedPlayerPawnsCanTargetLocations()
		{
			foreach (object selectedObject in Find.Selector.SelectedObjects)
			{
				Pawn pawn;
				if ((pawn = selectedObject as Pawn) != null && CanOrderPlayerPawn(pawn) && pawn.Drafted)
				{
					if (pawn.equipment.Primary == null || pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.IsMeleeAttack)
					{
						return false;
					}
					if (!pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.targetParams.canTargetLocations)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Storage : ITab
	{
		private ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();

		private static readonly Vector2 WinSize = new Vector2(300f, 480f);

		protected virtual IStoreSettingsParent SelStoreSettingsParent
		{
			get
			{
				Thing thing = base.SelObject as Thing;
				if (thing != null)
				{
					IStoreSettingsParent thingOrThingCompStoreSettingsParent = GetThingOrThingCompStoreSettingsParent(thing);
					if (thingOrThingCompStoreSettingsParent != null)
					{
						return thingOrThingCompStoreSettingsParent;
					}
					return null;
				}
				if (base.AllSelObjects.Count > 1)
				{
					bool flag = true;
					StorageGroup storageGroup = (base.AllSelObjects.First() as IStorageGroupMember)?.Group;
					if (storageGroup != null)
					{
						foreach (object allSelObject in base.AllSelObjects)
						{
							IStorageGroupMember storageGroupMember;
							if ((storageGroupMember = allSelObject as IStorageGroupMember) == null)
							{
								flag = false;
								break;
							}
							if (storageGroupMember.Group != storageGroup)
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							return (base.AllSelObjects.First() as IStorageGroupMember)?.Group;
						}
					}
					return null;
				}
				return base.SelObject as IStoreSettingsParent;
			}
		}

		public override bool IsVisible
		{
			get
			{
				if (base.SelObject != null)
				{
					Thing thing = base.SelObject as Thing;
					if (thing != null && thing.Faction != null && thing.Faction != Faction.OfPlayer)
					{
						return false;
					}
				}
				else
				{
					if (base.AllSelObjects.Count <= 1)
					{
						return false;
					}
					foreach (object allSelObject in base.AllSelObjects)
					{
						Thing thing2 = allSelObject as Thing;
						if (thing2 != null && thing2.Faction != null && thing2.Faction != Faction.OfPlayer)
						{
							return false;
						}
					}
				}
				return SelStoreSettingsParent?.StorageTabVisible ?? false;
			}
		}

		protected virtual bool IsPrioritySettingVisible => true;

		private float TopAreaHeight => IsPrioritySettingVisible ? 35 : 20;

		public ITab_Storage()
		{
			size = WinSize;
			labelKey = "TabStorage";
			tutorTag = "Storage";
		}

		public override void OnOpen()
		{
			base.OnOpen();
			thingFilterState.quickSearch.Reset();
		}

		protected override void FillTab()
		{
			IStoreSettingsParent storeSettingsParent = SelStoreSettingsParent;
			StorageSettings settings = storeSettingsParent.GetStoreSettings();
			Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
			Widgets.BeginGroup(rect);
			if (IsPrioritySettingVisible)
			{
				Text.Font = GameFont.Small;
				Rect rect2 = new Rect(0f, 0f, 160f, TopAreaHeight - 6f);
				if (Widgets.ButtonText(rect2, "Priority".Translate() + ": " + settings.Priority.Label().CapitalizeFirst()))
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (StoragePriority value in Enum.GetValues(typeof(StoragePriority)))
					{
						if (value != 0)
						{
							StoragePriority localPr = value;
							list.Add(new FloatMenuOption(localPr.Label().CapitalizeFirst(), delegate
							{
								settings.Priority = localPr;
							}));
						}
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
				UIHighlighter.HighlightOpportunity(rect2, "StoragePriority");
			}
			ThingFilter parentFilter = null;
			if (storeSettingsParent.GetParentStoreSettings() != null)
			{
				parentFilter = storeSettingsParent.GetParentStoreSettings().filter;
			}
			Rect rect3 = new Rect(0f, TopAreaHeight, rect.width, rect.height - TopAreaHeight);
			Bill[] first = (from b in BillUtility.GlobalBills()
				where b is Bill_Production && b.GetStoreZone() == storeSettingsParent && b.recipe.WorkerCounter.CanPossiblyStoreInStockpile((Bill_Production)b, b.GetStoreZone())
				select b).ToArray();
			ThingFilterUI.DoThingFilterConfigWindow(rect3, thingFilterState, settings.filter, parentFilter, 8, null, HiddenSpecialThingFilters());
			Bill[] second = (from b in BillUtility.GlobalBills()
				where b is Bill_Production && b.GetStoreZone() == storeSettingsParent && b.recipe.WorkerCounter.CanPossiblyStoreInStockpile((Bill_Production)b, b.GetStoreZone())
				select b).ToArray();
			foreach (Bill item in first.Except(second))
			{
				Messages.Message("MessageBillValidationStoreZoneInsufficient".Translate(item.LabelCap, item.billStack.billGiver.LabelShort.CapitalizeFirst(), item.GetStoreZone().label), item.billStack.billGiver as Thing, MessageTypeDefOf.RejectInput, historical: false);
			}
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.StorageTab, KnowledgeAmount.FrameDisplayed);
			Widgets.EndGroup();
		}

		protected IStoreSettingsParent GetThingOrThingCompStoreSettingsParent(Thing t)
		{
			IStoreSettingsParent storeSettingsParent = t as IStoreSettingsParent;
			if (storeSettingsParent != null)
			{
				return storeSettingsParent;
			}
			ThingWithComps thingWithComps = t as ThingWithComps;
			if (thingWithComps != null)
			{
				List<ThingComp> allComps = thingWithComps.AllComps;
				for (int i = 0; i < allComps.Count; i++)
				{
					storeSettingsParent = allComps[i] as IStoreSettingsParent;
					if (storeSettingsParent != null)
					{
						return storeSettingsParent;
					}
				}
			}
			return null;
		}

		public override void Notify_ClickOutsideWindow()
		{
			base.Notify_ClickOutsideWindow();
			thingFilterState.quickSearch.Unfocus();
		}

		private IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
		{
			if (ModsConfig.IdeologyActive)
			{
				yield return SpecialThingFilterDefOf.AllowVegetarian;
				yield return SpecialThingFilterDefOf.AllowCarnivore;
				yield return SpecialThingFilterDefOf.AllowCannibal;
				yield return SpecialThingFilterDefOf.AllowInsectMeat;
			}
		}
	}
}

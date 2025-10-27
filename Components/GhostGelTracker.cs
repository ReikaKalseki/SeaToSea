using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	public class GhostGelTracker : MonoBehaviour {

		public static readonly float REGEN_RATE = 60*30; //30 min per harvest

		private Sealed cutter;

		private float harvestCount;

		public void setup() {
			cutter = gameObject.EnsureComponent<Sealed>();
			cutter._sealed = true;
			cutter.maxOpenedAmount = 200;
			cutter.openedEvent.AddHandler(gameObject, new UWE.Event<Sealed>.HandleFunction(se => {
				se.openedAmount = 0;
				se._sealed = true;
				if (PDAScanner.complete.Contains(TechType.GhostLeviathan) || PDAScanner.complete.Contains(TechType.GhostLeviathanJuvenile)) {
					if (canHarvest) {
						InventoryUtil.addItem(CraftingItems.getItem(CraftingItems.Items.GhostGel).TechType);
						harvestCount++;
					}
					else {
						SNUtil.writeToChat(SeaToSeaMod.mouseoverLocale.getEntry("GhostLeviathanSampleCooldown").desc);
					}
				}
			}));
			GenericHandTarget ht = gameObject.EnsureComponent<GenericHandTarget>();
			ht.onHandHover = new HandTargetEvent();
			ht.onHandHover.AddListener(hte => {
				Pickupable held = Inventory.main.GetHeld();
				if (held && held.GetTechType() == TechType.Scanner)
					return;
				if (held && held.GetTechType() == TechType.LaserCutter && (PDAScanner.complete.Contains(TechType.GhostLeviathan) || PDAScanner.complete.Contains(TechType.GhostLeviathanJuvenile))) {
					if (canHarvest) {
						HandReticle.main.SetProgress(cutter.GetSealedPercentNormalized());
						HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
						HandReticle.main.SetInteractText("GhostLeviathanSample"); //is a locale key
					}
					else {
						HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
						HandReticle.main.SetInteractText("GhostLeviathanSampleCooldown"); //is a locale key
					}
				}
			});
		}

		public bool canHarvest { 
			get {
				return harvestCount < 3;
			}
		}

		void Update() {
			if (harvestCount > 0)
				harvestCount -= Time.deltaTime / REGEN_RATE;
		}
	}
}

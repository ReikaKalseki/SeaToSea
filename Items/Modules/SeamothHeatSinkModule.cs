using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class SeamothHeatSinkModule : SeamothModule {
		
		internal static bool FREE_CHEAT = false;
				
		public SeamothHeatSinkModule() : base(SeaToSeaMod.itemLocale.getEntry("SeamothHeatSinkModule"), "742d2a09-a2d7-4acd-b9c7-1f97cb793932") {
			preventNaturalUnlock();
		}

		public override QuickSlotType QuickSlotType {
			get {
				return QuickSlotType.Chargeable;
			}
		}
		/*
		protected override Atlas.Sprite GetItemSprite()
		{
			return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
		}*/

		public override SeamothModule.SeamothModuleStorage getStorage() {
			return new SeamothModule.SeamothModuleStorage("HEAT SINK STORAGE", 3, 4).addAmmo(C2CItems.heatSink);
		}

		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
		}

		protected override float getMaxCharge() {
			return base.getMaxCharge()*6;
		}
		
		public override float getUsageCooldown() {
			return 30;
		}

		public override void onFired(SeaMoth sm, int slotID, float charge) {
			SeamothStorageContainer sc = getStorage(sm, slotID);
			if ((FREE_CHEAT || sc.container.GetCount(C2CItems.heatSink.TechType) > 0) && !sm.GetComponent<C2CMoth>().isPurgingHeat()) {
				GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.ejectedHeatSink.ClassID);
				go.transform.position = sm.transform.position+sm.transform.forward*4;
				go.GetComponent<Rigidbody>().AddForce(sm.transform.forward*10, ForceMode.VelocityChange);
				go.GetComponent<HeatSinkTag>().onFired();
				sm.GetComponent<C2CMoth>().purgeHeat();
				if (!FREE_CHEAT)
					sc.container.DestroyItem(C2CItems.heatSink.TechType);
			}
		}
		
		private SeamothStorageContainer getStorage(SeaMoth sm, int slotID) {
			string slot = sm.slotIDs[slotID];
			InventoryItem item = sm.modules.GetItemInSlot(slot);
			return item.item.GetComponent<SeamothStorageContainer>();
		}
	}
}

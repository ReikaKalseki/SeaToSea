using System;
using System.Collections.Generic;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public sealed class SeamothSpeedModule : SeamothModule {

		public SeamothSpeedModule() : base(SeaToSeaMod.itemLocale.getEntry("SeaMothSpeed")) {
			this.preventNaturalUnlock();
		}

		public override QuickSlotType QuickSlotType {
			get {
				return CraftData.GetQuickSlotType(TechType.SeamothSonarModule);
			}
		}

		public override float getUsageCooldown() {
			return 5;
		}

		public override void onFired(SeaMoth sm, int slotID, float charge) { //charge is 0-1
			sm.GetComponent<C2CMoth>().applySpeedBoost(charge);
		}

		protected override float getChargingPowerCost() {
			return 5;
		}
		/*
		protected override Atlas.Sprite GetItemSprite()
		{
			return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
		}*/
	}
}

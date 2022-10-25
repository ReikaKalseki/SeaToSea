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

		protected override float getMaxCharge() {
			return base.getMaxCharge()*6;
		}

		public override void onFired(SeaMoth sm, int slotID, float charge) {
			if (true) { //has heatsink
				GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.ejectedHeatSink.ClassID);
				go.transform.position = sm.transform.position+sm.transform.forward*6;
				go.GetComponent<Rigidbody>().AddForce(sm.transform.forward*20, ForceMode.VelocityChange);
				go.GetComponent<HeatSinkTag>().onFired();
				sm.GetComponent<C2CMoth>().purgeHeat();
				//consumeHeatsink();
			}
		}
	}
}

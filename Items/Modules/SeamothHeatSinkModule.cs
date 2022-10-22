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
	}
}

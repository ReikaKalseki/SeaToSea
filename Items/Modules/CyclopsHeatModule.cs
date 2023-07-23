using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class CyclopsHeatModule : CyclopsModule {
				
		public CyclopsHeatModule() : base(SeaToSeaMod.itemLocale.getEntry("CyclopsHeat")) {
			preventNaturalUnlock();
		}

		public override QuickSlotType QuickSlotType {
			get {
				return QuickSlotType.Passive;
			}
		}
		
		public override Vector2int SizeInInventory {
			get {
				return new Vector2int(3, 3);
			}
		}
		/*
		protected override Atlas.Sprite GetItemSprite()
		{
			return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
		}*/
	}
}

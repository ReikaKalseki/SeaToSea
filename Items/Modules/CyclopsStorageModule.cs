using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class CyclopsStorageModule : CyclopsModule {
				
		public CyclopsStorageModule() : base(SeaToSeaMod.itemLocale.getEntry("CyclopsStorage")) {
			
		}

		public override QuickSlotType QuickSlotType {
			get {
				return QuickSlotType.Passive;
			}
		}
		
		public override Vector2int SizeInInventory {
			get {
				return new Vector2int(2, 2);
			}
		}
		/*
		protected override Atlas.Sprite GetItemSprite()
		{
			return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
		}*/
	}
}

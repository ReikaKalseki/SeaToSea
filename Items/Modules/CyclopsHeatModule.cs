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
				
		public CyclopsHeatModule() : base(SeaToSeaMod.locale.getEntry("CyclopsHeat")) {
			
		}

		public override TechType RequiredForUnlock {
			get {
				return TechType.Kyanite;
			}
		}

		public override QuickSlotType QuickSlotType {
			get {
				return QuickSlotType.Passive;
			}
		}
		/*
		protected override Atlas.Sprite GetItemSprite()
		{
			return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
		}*/
	}
}

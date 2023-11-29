using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class SeamothTetherModule : SeamothModule {
				
		public SeamothTetherModule() : base(SeaToSeaMod.itemLocale.getEntry("SeamothTether")) {
			preventNaturalUnlock();
		}

		public override QuickSlotType QuickSlotType {
			get {
				return QuickSlotType.Toggleable;
			}
		}
		
		protected override float getChargingPowerCost() {
			return 1;
		}
	}
}

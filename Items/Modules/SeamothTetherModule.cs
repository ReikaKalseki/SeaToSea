using System;
using System.Collections.Generic;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public sealed class SeamothTetherModule : SeamothModule {

		public SeamothTetherModule() : base(SeaToSeaMod.itemLocale.getEntry("SeamothTether")) {
			this.preventNaturalUnlock();
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

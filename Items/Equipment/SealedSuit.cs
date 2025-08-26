using System;
using System.Collections.Generic;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public sealed class SealedSuit : CustomEquipable {

		public SealedSuit() : base(SeaToSeaMod.itemLocale.getEntry("SealedSuit"), "WorldEntities/Tools/ReinforcedDiveSuit") {
			isArmor = true;
			this.preventNaturalUnlock();
		}

		public override Vector2int SizeInInventory {
			get { return new Vector2int(2, 2); }
		}

		public override void prepareGameObject(GameObject go, Renderer[] r) {

		}

		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.Body;
			}
		}

		public override List<TechType> getAuxCrafted() {
			return new List<TechType> { C2CItems.sealGloves.TechType };
		}
	}
}

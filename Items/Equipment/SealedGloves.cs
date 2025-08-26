using System;
using System.Collections.Generic;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public sealed class SealedGloves : CustomEquipable {

		public SealedGloves() : base(SeaToSeaMod.itemLocale.getEntry("SealedGloves"), "WorldEntities/Tools/ReinforcedGloves") {
			isArmor = true;
			this.preventNaturalUnlock();
		}

		public override Vector2int SizeInInventory {
			get { return new Vector2int(2, 2); }
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.None;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Uncategorized;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.Misc;
			}
		}

		public override void prepareGameObject(GameObject go, Renderer[] r) {

		}

		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.Gloves;
			}
		}
	}
}

using System;
using System.Collections.Generic;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public sealed class RebreatherV2 : CustomEquipable {

		public RebreatherV2() : base(SeaToSeaMod.itemLocale.getEntry("RebreatherV2"), "WorldEntities/Natural/rebreather") {
			isArmor = true;
			this.preventNaturalUnlock();
		}

		public override Vector2int SizeInInventory {
			get { return new Vector2int(2, 3); }
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.Workbench;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[] { "TankMenu" };
			}
		}

		public override void prepareGameObject(GameObject go, Renderer[] r) {

		}

		public override EquipmentType EquipmentType {
			get {
				return EquipmentType.Head;
			}
		}
	}
}

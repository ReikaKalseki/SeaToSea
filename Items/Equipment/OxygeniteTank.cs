using System;
using System.Collections.Generic;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	[Obsolete]
	public sealed class OxygeniteTank : CustomEquipable {

		public static readonly int TANK_CAPACITY = 855; //total 20 min

		public OxygeniteTank() : base(SeaToSeaMod.itemLocale.getEntry("OxygeniteTank"), "WorldEntities/Tools/HighCapacityTank") {
			isArmor = true;
			this.preventNaturalUnlock();
		}

		public override Vector2int SizeInInventory {
			get { return new Vector2int(3, 4); }
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
			Oxygen o2 = go.EnsureComponent<Oxygen>();
			o2.oxygenCapacity = TANK_CAPACITY;
			o2.oxygenAvailable = o2.oxygenCapacity;
		}

		public override EquipmentType EquipmentType {
			get {
				return EquipmentType.Tank;
			}
		}
	}
}

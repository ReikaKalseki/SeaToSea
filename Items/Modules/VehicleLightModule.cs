using System;
using System.Collections.Generic;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public sealed class VehicleLightModule : CustomEquipable {

		public VehicleLightModule() : base(SeaToSeaMod.itemLocale.getEntry("VehicleLightBonus"), "d290b5da-7370-4fb8-81bc-656c6bde78f8") {
			this.preventNaturalUnlock();
		}

		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.VehicleModule;
			}
		}

		public override QuickSlotType QuickSlotType {
			get {
				return QuickSlotType.Selectable;
			}
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.SeamothUpgrades;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[] { "CommonModules" };
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.VehicleUpgrades;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.VehicleUpgrades;
			}
		}
	}
}

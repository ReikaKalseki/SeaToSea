using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea
{
	public sealed class ChargeFinRelay : CustomEquipable {
		
		public ChargeFinRelay() : base(SeaToSeaMod.itemLocale.getEntry("ChargeFinRelay"), "WorldEntities/Tools/Compass") {
			preventNaturalUnlock();
		}

		public override Vector2int SizeInInventory {
			get {return new Vector2int(2, 2);}
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.Workbench;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"C2CModElectronics"};
			}
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			
		}
		
		public override EquipmentType EquipmentType {
			get {
				return EquipmentType.Chip;
			}
		}
	}
}

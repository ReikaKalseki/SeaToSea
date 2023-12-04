using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class LiquidTank : CustomEquipable {
		
		public LiquidTank() : base(SeaToSeaMod.itemLocale.getEntry("LiquidTank"), "WorldEntities/Tools/HighCapacityTank") {
			isArmor = true;
			preventNaturalUnlock();
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<Battery>().addField("_charge"));
			};
		}

		public override Vector2int SizeInInventory {
			get {return new Vector2int(3, 3);}
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.Workbench;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"TankMenu"};
			}
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			Oxygen o2 = go.EnsureComponent<Oxygen>();
			o2.oxygenAvailable = 0;
			o2.oxygenCapacity = LiquidBreathingSystem.TANK_CAPACITY;
			Battery b = go.EnsureComponent<Battery>();
			b.charge = 0;
			b._capacity = LiquidBreathingSystem.ITEM_VALUE;
		}
		
		public override EquipmentType EquipmentType {
			get {
				return EquipmentType.Tank;
			}
		}
	}
}

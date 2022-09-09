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
		}

		public override Vector2int SizeInInventory {
			get {return new Vector2int(3, 3);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			Oxygen b = go.EnsureComponent<Oxygen>();
			b.oxygenAvailable = 0;
			b.oxygenCapacity = LiquidBreathingSystem.TANK_CAPACITY;
		}
		
		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.Tank;
			}
		}
	}
}

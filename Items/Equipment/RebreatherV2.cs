using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class RebreatherV2 : CustomEquipable {
		
		public RebreatherV2() : base(SeaToSeaMod.itemLocale.getEntry("RebreatherV2"), "WorldEntities/Natural/rebreather") {
			isArmor = true;
			preventNaturalUnlock();
		}

		public override Vector2int SizeInInventory {
			get {return new Vector2int(2, 3);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			
		}
		
		public override EquipmentType EquipmentType {
			get {
				return EquipmentType.Head;
			}
		}
	}
}

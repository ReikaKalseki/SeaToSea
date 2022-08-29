using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class SealedSuit : CustomEquipable {
		
		public SealedSuit() : base(SeaToSeaMod.itemLocale.getEntry("SealedSuit"), "WorldEntities/Tools/ReinforcedDiveSuit") {
			isArmor = true;
			preventNaturalUnlock();
		}

		public override Vector2int SizeInInventory {
			get {return new Vector2int(2, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			
		}
		
		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.Body;
			}
		}
	}
}

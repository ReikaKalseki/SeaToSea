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
		
		public SealedSuit() : base(SeaToSeaMod.locale.getEntry("SealedSuit"), "WorldEntities/Tools/ReinforcedDiveSuit") {
			
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			
		}

		public override TechType RequiredForUnlock {
			get {
				return TechType.Kyanite;
			}
		}
		
		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.Body;
			}
		}
	}
}

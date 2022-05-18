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
		
		public RebreatherV2() : base(SeaToSeaMod.locale.getEntry("RebreatherV2")) {
			
		}

		public override TechType RequiredForUnlock {
			get {
				return TechType.Kyanite;
			}
		}
		
		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.Head;
			}
		}
		
		protected override sealed string getTemplatePrefab() {
			return "WorldEntities/Natural/rebreather";
		}
	}
}

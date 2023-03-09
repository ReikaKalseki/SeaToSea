using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class AzuriteBattery : CustomBattery {
		
		public AzuriteBattery() : base(SeaToSeaMod.itemLocale.getEntry("t2battery"), 750) {
			unlockRequirement = TechType.Unobtanium;//CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType;
			inventorySize = new Vector2int(1, 2);
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.transform.localScale = new Vector3(1.2F, 1.2F, 1.5F);
			AzuriteSparker az = go.EnsureComponent<AzuriteBatterySparker>();
			//go.EnsureComponent<AzuriteBatteryTag>();
		}
	}
	
	class AzuriteBatterySparker : AzuriteSparker {
		AzuriteBatterySparker() : base(0.67F, 0.5F, new Vector3(0, 0, -0.05F)) {
			
		}		
	}
}

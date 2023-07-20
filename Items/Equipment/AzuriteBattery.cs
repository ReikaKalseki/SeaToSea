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
		
		internal static readonly Vector3 REMOVE = new Vector3(372.18F, -92.8F, 1039.2F);
		
		public AzuriteBattery() : base(SeaToSeaMod.itemLocale.getEntry("t2battery"), 750) {
			unlockRequirement = TechType.Unobtanium;//CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType;
			inventorySize = new Vector2int(1, 2);
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.transform.localScale = new Vector3(1.2F, 1.2F, 1.5F);
			AzuriteSparker az = go.EnsureComponent<AzuriteBatterySparker>();
			go.EnsureComponent<AzuriteBatteryTag>();
		}
	}
	
	class AzuriteBatterySparker : AzuriteSparker {
		AzuriteBatterySparker() : base(0.67F, 0.5F, false, new Vector3(0, 0, -0.05F)) {
			
		}		
	}
	
	class AzuriteBatteryTag : MonoBehaviour {
		
		void Update() {
			if (Vector3.Distance(transform.position, AzuriteBattery.REMOVE) <= 0.25 && (Player.main && Vector3.Distance(Player.main.transform.position, AzuriteBattery.REMOVE) > 50)) {
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}
		
	}
}

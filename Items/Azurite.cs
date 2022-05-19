using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class Azurite : BasicCustomOre {
		
		public Azurite(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<AzuriteTag>();
		}
		
	}
	
	class AzuriteTag : MonoBehaviour {
		
		void Start() {
    		
		}
		
		void Update() {
			if (Player.main != null) { //TODO check last damage time
				GameObject ep = Player.main.gameObject;
				double distsq = (ep.transform.position-gameObject.transform.position).sqrMagnitude;
				if (distsq < 64) {
					if (Inventory.main.equipment.GetCount(SeaToSeaMod.sealSuit.TechType) == 0 || Inventory.main.equipment.GetCount(TechType.SwimChargeFins) != 0) {
						ep.GetComponentInParent<LiveMixin>().TakeDamage(0.1F, ep.transform.position, DamageType.Electrical, ep); //TODO make use time elapsed
					}
				}
			}
		}
		
	}
}

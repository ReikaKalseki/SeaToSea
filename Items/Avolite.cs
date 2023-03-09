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
	
	public class Avolite : BasicCustomOre {
		
		public Avolite(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			collectSound = "event:/loot/pickup_precursorioncrystal";
			inventorySize = new Vector2int(2, 1);
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r0) {
			base.prepareGameObject(go, r0);
			foreach (Renderer r in r0) {
				//GameObject go = ;
				r.materials[0].EnableKeyword("FX_BUILDING");
				r.materials[0].SetFloat("_Built", 0.1F);
				r.materials[0].SetFloat("_BuildLinear", 0.0F);
				r.materials[0].SetFloat("_NoiseThickness", 0.05F);
				r.materials[0].SetFloat("_NoiseStr", 1F);
				//r.materials[0].SetColor("_BorderColor", new Color(20, 2F, 1, 1));
				r.materials[0].SetColor("_BorderColor", new Color(1, 0.1F, 0, 1));
				r.materials[0].SetVector("_BuildParams", new Vector4(1, 1, 0.35F, 0.1F));
			}
		}
		
	}
}

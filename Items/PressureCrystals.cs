using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	public class PressureCrystals : BasicCustomOre {

		public PressureCrystals(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			collectSound = "event:/loot/pickup_quartz";
		}

		public override void prepareGameObject(GameObject go, Renderer[] r0) {
			base.prepareGameObject(go, r0);
			foreach (Renderer r in r0) {
				RenderUtil.makeTransparent(r);
				r.sharedMaterial.SetFloat("_Fresnel", 0.65F);
				r.sharedMaterial.SetFloat("_Shininess", 15F);
				r.sharedMaterial.SetFloat("_SpecInt", 18F);
				r.materials[0].SetFloat("_Fresnel", 0.6F);
				r.materials[0].SetFloat("_Shininess", 15F);
				r.materials[0].SetFloat("_SpecInt", 18F);
				r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
			}

			Light l = go.addLight(0.3F, 4, new UnityEngine.Color(1F, 0.45F, 1F));
			l.type = LightType.Point;
			l = go.addLight(1.2F, 1, new UnityEngine.Color(1F, 0.45F, 1F));
			l.type = LightType.Point;
		}

	}
}

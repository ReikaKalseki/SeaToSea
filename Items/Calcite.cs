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

	public class Calcite : BasicCustomOre {

		public Calcite(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			collectSound = "event:/loot/pickup_quartz";
		}

		public override void prepareGameObject(GameObject go, Renderer[] r0) {
			base.prepareGameObject(go, r0);
			GameObject rref = r0[0].gameObject;
			for (int i = 0; i < 4; i++) {
				GameObject rg = UnityEngine.Object.Instantiate(r0[0].gameObject);
				rg.transform.SetParent(rref.transform);
				rg.transform.localScale = Vector3.one * (1F - ((i + 1) * 0.05F));
				rg.transform.localEulerAngles = new Vector3(270, 0, 0);
				rg.transform.localPosition = Vector3.zero;
			}
			r0 = go.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < r0.Length; i++) {
				Renderer r = r0[i];
				RenderUtil.makeTransparent(r);
				r.sharedMaterial.SetFloat("_Fresnel", 0.6F);
				r.sharedMaterial.SetFloat("_Shininess", 10F);
				r.sharedMaterial.SetFloat("_SpecInt", 3F);
				r.materials[0].SetFloat("_Fresnel", 0.6F);
				r.materials[0].SetFloat("_Shininess", 10F);
				r.materials[0].SetFloat("_SpecInt", 3F);
				r.materials[0].SetColor("_Color", new Color(1, 1, 1, i == r0.Length - 1 ? 1 : 0.8F));
				r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
				//r.materials[0].SetColor("_SpecColor", new Color(1, 0.75F, 0.55F, 1));
			}
		}

	}
}

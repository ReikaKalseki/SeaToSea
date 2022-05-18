﻿using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class PressureCrystals : BasicCustomOre {
		
		public PressureCrystals(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			collectSound = "event:/loot/pickup_quartz";
		}
		
		protected override void prepareGameObject(GameObject go, Renderer r) {
			SBUtil.makeTransparent(r);
			r.sharedMaterial.SetFloat("_Fresnel", 0.65F);
			r.sharedMaterial.SetFloat("_Shininess", 15F);
			r.sharedMaterial.SetFloat("_SpecInt", 18F);
			r.materials[0].SetFloat("_Fresnel", 0.6F);
			r.materials[0].SetFloat("_Shininess", 15F);
			r.materials[0].SetFloat("_SpecInt", 18F);
			r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
		}
		
	}
}
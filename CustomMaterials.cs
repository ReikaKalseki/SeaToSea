using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Xml;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public static class CustomMaterials {
		
		private static readonly Dictionary<Materials, BasicCustomOre> mappings = new Dictionary<Materials, BasicCustomOre>();
				
		static CustomMaterials() {
			foreach (Materials m in Enum.GetValues(typeof(Materials))) {
				string id = Enum.GetName(typeof(Materials), m);
				SBUtil.log("Registering material "+id);
				Material attr = getMaterial(m);
				XMLLocale.LocaleEntry e = SeaToSeaMod.locale.getEntry(id);
				VanillaResources template = (VanillaResources)typeof(VanillaResources).GetField(attr.templateName).GetValue(null);
				BasicCustomOre item = (BasicCustomOre)Activator.CreateInstance(attr.itemClass, new object[]{id, e.name, e.desc, template});
				item.glowIntensity = attr.glow;
				if (m == Materials.PRESSURE_CRYSTALS) {
					//item.glowType = "EmissionLM";
					item.renderModify = r => {
						SBUtil.makeTransparent(r);
						r.sharedMaterial.SetFloat("_Fresnel", 0.65F);
						r.sharedMaterial.SetFloat("_Shininess", 15F);
						r.sharedMaterial.SetFloat("_SpecInt", 18F);
						r.materials[0].SetFloat("_Fresnel", 0.6F);
						r.materials[0].SetFloat("_Shininess", 15F);
						r.materials[0].SetFloat("_SpecInt", 18F);
						r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
					};
				}
				mappings[m] = item;
				item.Patch();	
				item.addPDAEntry(e.pda, m == Materials.PRESSURE_CRYSTALS ? 5 : 2);
				SBUtil.log(" > "+item);
			}
		}
		
		private static Material getMaterial(Materials key) {
			FieldInfo info = typeof(Materials).GetField(Enum.GetName(typeof(Materials), key));
			return (Material)Attribute.GetCustomAttribute(info, typeof(Material));
		}
		
		public static BasicCustomOre getItem(Materials key) {
			return mappings[key];
		}
		
		public enum Materials {
			[Material(typeof(BasicCustomOre), "URANIUM", 4F)]		VENT_CRYSTAL, //forms when superheated water is injected into cold water
			[Material(typeof(BasicCustomOre), "GOLD")]				PLATINUM,
			[Material(typeof(BasicCustomOre), "TITANIUM", 1.2F)]	PRESSURE_CRYSTALS,
			[Material(typeof(BasicCustomOre), "KYANITE", 0.75F)]	PHASE_CRYSTAL,	
			[Material(typeof(BasicCustomOre), "SILVER")]			IRIDIUM,
		}
		
		public class Material : Attribute {
			
			internal readonly Type itemClass;
			internal readonly string templateName;
			internal readonly float glow;
			
			public Material(Type item, string t, float g = 0) {
				itemClass = item;
				templateName = t;
				glow = g;
			}
		}
	}
}

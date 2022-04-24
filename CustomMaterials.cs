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
				mappings[m] = item;
				item.addPDAEntry(e.pda, m == Materials.PRESSURE_CRYSTALS ? 5 : 2);
				item.Patch();	
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
			[Material(typeof(BasicCustomOre), "URANIUM")]		MOUNTAIN_CRYSTAL,
			[Material(typeof(BasicCustomOre), "LITHIUM")]		PLATINUM,
			[Material(typeof(BasicCustomOre), "RUBY")]			PRESSURE_CRYSTALS,
			[Material(typeof(BasicCustomOre), "KYANITE")]		PHASE_CRYSTAL,		
		}
		
		public class Material : Attribute {
			
			internal readonly Type itemClass;
			internal readonly string templateName;
			
			public Material(Type item, string t) {
				itemClass = item;
				templateName = t;
			}
		}
	}
}

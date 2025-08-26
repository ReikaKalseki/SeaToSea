using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public static class CustomMaterials {

		private static readonly Dictionary<Materials, BasicCustomOre> mappings = new Dictionary<Materials, BasicCustomOre>();
		private static readonly Dictionary<TechType, BasicCustomOre> techs = new Dictionary<TechType, BasicCustomOre>();

		static CustomMaterials() {
			foreach (Materials m in Enum.GetValues(typeof(Materials))) {
				string id = Enum.GetName(typeof(Materials), m);
				SNUtil.log("Registering material " + id);
				Material attr = getMaterial(m);
				XMLLocale.LocaleEntry e = SeaToSeaMod.itemLocale.getEntry(id);
				VanillaResources template = (VanillaResources)typeof(VanillaResources).GetField(attr.templateName).GetValue(null);
				BasicCustomOre item = (BasicCustomOre)Activator.CreateInstance(attr.itemClass, new object[]{id, e.name, e.desc, template});
				item.glowIntensity = attr.glow;
				switch (m) { //since has no custom class
					case Materials.IRIDIUM:
						item.collectSound = "event:/loot/pickup_copper";
						break;
				}
				mappings[m] = item;
				item.Patch();
				techs[item.TechType] = item;
				item.addPDAEntry(e.pda, m == Materials.PRESSURE_CRYSTALS ? 5 : 2, e.getField<string>("header"));
				SNUtil.log(" > " + item);
			}
		}

		public static Material getMaterial(Materials key) {
			FieldInfo info = typeof(Materials).GetField(Enum.GetName(typeof(Materials), key));
			return (Material)Attribute.GetCustomAttribute(info, typeof(Material));
		}

		public static BasicCustomOre getItem(Materials key) {
			return mappings[key];
		}

		public static BasicCustomOre getItemByTech(TechType tt) {
			return techs.ContainsKey(tt) ? techs[tt] : null;
		}

		public static TechType getIngot(Materials key) {
			return C2CItems.getIngot(getItem(key).TechType).ingot;
		}

		public enum Materials {
			[Material(typeof(Azurite),          "URANIUM",  4F)]        VENT_CRYSTAL, //forms when superheated water is injected into cold water
			[Material(typeof(Platinum),         "GOLD")]                PLATINUM,
			[Material(typeof(PressureCrystals), "TITANIUM", 1.2F)]      PRESSURE_CRYSTALS,
			[Material(typeof(Avolite),          "KYANITE",  0.75F)]     PHASE_CRYSTAL,
			[Material(typeof(BasicCustomOre),   "SILVER")]              IRIDIUM,
			[Material(typeof(Calcite),          "MAGNETITE", 0.2F)]     CALCITE,
			[Material(typeof(Obsidian),         "KYANITE", 0.2F)]       OBSIDIAN,
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

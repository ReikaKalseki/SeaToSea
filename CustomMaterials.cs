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
		
		private static readonly string XML_FILE = "XML/items.xml";
		
		private static readonly XmlDocument xmlFile;
		
		static CustomMaterials() {
			xmlFile = loadXML();
			foreach (Materials m in Enum.GetValues(typeof(Materials))) {
				string id = Enum.GetName(typeof(Materials), m);
				SBUtil.log("Registering material "+id);
				Material attr = getMaterial(m);
				XmlElement e = getItemElement(id);
				VanillaResources template = (VanillaResources)typeof(VanillaResources).GetField(attr.templateName).GetValue(null);
				string name = e != null ? e.getProperty("name").Trim() : "#NULL";
				string desc = e != null ? e.getProperty("desc").Trim() : "#NULL";
				BasicCustomOre item = (BasicCustomOre)Activator.CreateInstance(attr.itemClass, new object[]{id, name, desc, template});
				mappings[m] = item;
				item.Patch();	
				SBUtil.log(" > "+item);
			}
		}
		
		public static XmlElement getItemElement(string id) {
			XmlNodeList matches = xmlFile.GetElementsByTagName(id);
			XmlElement e = matches.Count == 0 ? null : matches[0] as XmlElement;
			return e;
		}
		
		private static XmlDocument loadXML() {
			string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path = Path.Combine(loc, XML_FILE);
			XmlDocument doc = new XmlDocument();
			if (File.Exists(path)) {
				doc.Load(path);
			}
			else {
				SBUtil.log("Could not find XML file "+path+"!");
			}
			return doc;
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
			[Material(typeof(BasicCustomOre), "LARGE_RUBY")]	PRESSURE_CRYSTALS,
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

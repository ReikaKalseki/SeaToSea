using System;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class AllPlants : WorldGenerator {
		
		private double offsetX = 0;
		
		public AllPlants(Vector3 pos) : base(pos) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override void generate() {			
			GameObject rock = PlacedObject.createWorldObject("a474e5fa-1552-4cea-abdb-945f85ed4b1a");
			rock.transform.position = position;
			rock.transform.localScale = new Vector3(20, 1, 1);
			
			foreach (FieldInfo f in typeof(VanillaFlora).GetFields()) {
				if (f.IsStatic && f.FieldType == typeof(VanillaFlora)) {
					spawnPlant((VanillaFlora)f.GetValue(null));
					offsetX += 3;
				}
			}
		}
		
		private void spawnPlant(VanillaFlora f) {
			GameObject go = PlacedObject.createWorldObject(f.getRandomPrefab(false));;
			go.transform.position = new Vector3(position.x+offsetX, position.y+5, position.z);
		}
	}
}

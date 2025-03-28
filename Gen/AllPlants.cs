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
		
		private static double step = 25;
		private double offsetX = -80;
		private double offsetZ = -step;
		private int index = 0;
		
		public AllPlants(Vector3 pos) : base(pos) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override bool generate(List<GameObject> li) {			
			GameObject rock = ObjectUtil.createWorldObject("a474e5fa-1552-4cea-abdb-945f85ed4b1a");
			rock.transform.position = position;
			rock.transform.localScale = new Vector3(150, 1, 150);
			
			foreach (VanillaFlora vf in VanillaFlora.getAll()) {
				li.Add(spawnPlant(vf));
				if (vf.maximumSink > 0.01) {
					li.Add(spawnPlant(vf, vf.maximumSink));
				}
			}
			return true;
		}
		
		public override LargeWorldEntity.CellLevel getCellLevel() {
			return LargeWorldEntity.CellLevel.Global;
		}
		
		private GameObject spawnPlant(VanillaFlora f, double sink = 0) {
			offsetZ = ((index%5)-2)*step;
			GameObject go = ObjectUtil.createWorldObject(f.getRandomPrefab(false));
			double d = f.baseOffset < -99 ? 6 : f.baseOffset;
			d -= sink;
			go.transform.position = new Vector3(position.x+(float)offsetX, position.y+(float)d, position.z+(float)offsetZ);
			index++;
			if (index%5 == 4)
				offsetX += 8;
			return go;
		}
	}
}

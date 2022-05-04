/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{		
	internal class VoidChunkPlants : RandomPlant {
	
		private int mushrooms;
		
		public bool allowKelp = true;
		
		private List<GameObject> gennedMushrooms = new List<GameObject>();
		
		public VoidChunkPlants(Vector3 vec) : base(vec) {
			this.fuzz = new Vector3(1.2F, 0.05F, 1.2F);
			this.count = UnityEngine.Random.Range(1, 4); //1-3
			this.preferLit = true;
			
			plants.addEntry(VanillaFlora.GABE_FEATHER, 100);
			plants.addEntry(VanillaFlora.GHOSTWEED, 85);
			plants.addEntry(VanillaFlora.MEMBRAIN, 25);
			plants.addEntry(VanillaFlora.REGRESS, 10);
			plants.addEntry(VanillaFlora.BRINE_LILY, 50);
			plants.addEntry(VanillaFlora.BLOOD_KELP, 10);
			
			mushrooms = UnityEngine.Random.Range(0, 7); //0-6
		}
		
		public override void generate(List<GameObject> li) {
			base.generate(li);
			
			li.AddRange(gennedMushrooms);
		}
		
		protected override VanillaFlora selectPlant(VanillaFlora choice) {
			while (choice == VanillaFlora.BLOOD_KELP && !allowKelp)
				choice = plants.getRandomEntry();
			return choice;
		}
		
		protected override GameObject generatePlant(Vector3 vec, string type) {
			GameObject go = base.generatePlant(vec, type);
			if (!VanillaFlora.BRINE_LILY.includes(type)) {
				for (int i = 0; i < mushrooms; i++) {
					Vector3 vec2 = new Vector3(vec.x+UnityEngine.Random.Range(-1F, 1F), vec.y, vec.z+UnityEngine.Random.Range(-1F, 1F));
					int tries = 0;
					while ((SBUtil.objectCollidesPosition(go, vec2) || isColliding(vec2, gennedMushrooms)) && tries < 5) {
						vec2 = new Vector3(vec.x+UnityEngine.Random.Range(-1F, 1F), vec.y, vec.z+UnityEngine.Random.Range(-1F, 1F));
						tries++;
					}
					if (!SBUtil.objectCollidesPosition(go, vec2) && !isColliding(vec2, gennedMushrooms)) {
						if (validPlantPosCheck != null && !validPlantPosCheck(vec2+Vector3.up*0.2F, "mush"))
							continue;
						GameObject go2 = base.generatePlant(vec2, VanillaFlora.DEEP_MUSHROOM.getRandomPrefab(false));
						gennedMushrooms.Add(go2);
					}
				}
			}
			return go;
		}
		
		public override void loadFromXML(XmlElement e) {
			base.loadFromXML(e);
			
			mushrooms = e.getInt("mushrooms", mushrooms);
		}
		
		public override void saveToXML(XmlElement e) {
			base.saveToXML(e);
			
			e.addProperty("mushrooms", mushrooms);
		}
		
	}
}

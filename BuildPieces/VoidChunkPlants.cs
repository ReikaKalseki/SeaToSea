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
using System.Collections.ObjectModel;
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
		
		private static readonly Dictionary<VanillaFlora, VoidPlant> plantTypes = new Dictionary<VanillaFlora, VoidPlant>();
		
		static VoidChunkPlants() {
			addPlantType(new VoidPlant(VanillaFlora.GABE_FEATHER, 100));
			addPlantType(new VoidPlant(VanillaFlora.GHOSTWEED, 650, 5000, 700, 900, 50, 90));
			addPlantType(new VoidPlant(VanillaFlora.MEMBRAIN, 0, 800, 0, 800, 10, 20));
			addPlantType(new VoidPlant(VanillaFlora.REGRESS, 0, 700, 500, 700, 30, 10));
			addPlantType(new VoidPlant(VanillaFlora.BRINE_LILY, 50));
			addPlantType(new VoidPlant(VanillaFlora.AMOEBOID, 750, 5000, 750, 1000, 0, 15));
			addPlantType(new VoidPlant(VanillaFlora.BLOOD_KELP, 15));
			addPlantType(new VoidPlant(VanillaFlora.VIOLET_BEAU, 0, 600, 500, 600, 30, 10));
			/*
			List<VoidPlant> li = new List<VoidPlant>(plantTypes.Values);
			List<string> csv = new List<string>();
			for (int y = -450; y >= -1100; y -= 10) {
				SBUtil.log("@ Depth = "+(-y)+":");
				double sum = 0;
				string line = (-y)+"";
				foreach (VoidPlant p in li) {
					sum += p.getWeight(y);
				}
				foreach (VoidPlant p in li) {
					double wt = p.getWeight(y);
					double frac = wt/sum;
					double pct = frac*100;
					SBUtil.log(p.plant.getName()+" ["+p.minDepth+"-"+p.maxDepth+"]: wt="+wt+" > "+pct+"%");
					line = line+","+pct;
				}
				csv.Add(line);
			}
			string header = "depth";
			foreach (VoidPlant p in li) {
				header = header+","+p.plant.getName();
			}
			csv.Insert(0, header);
			SBUtil.log("CSV EXPORT");
			SBUtil.log(string.Join("\n", csv));
			SBUtil.log("========");*/
		}
		
		private static void addPlantType(VoidPlant vp) {
			plantTypes[vp.plant] = vp;
		}
		
		public static IEnumerable<VanillaFlora> getPlants() {
			return new ReadOnlyCollection<VanillaFlora>(new List<VanillaFlora>(plantTypes.Keys));
		}
		
		public VoidChunkPlants(Vector3 vec) : base(vec) {
			this.fuzz = new Vector3(1.2F, 0.05F, 1.2F);
			this.count = UnityEngine.Random.Range(1, 4); //1-3
			this.preferLit = true;
			
			foreach (VoidPlant p in plantTypes.Values) {
				double wt = p.getWeight(vec.y);
				if (wt > 0)
					plants.addEntry(p.plant, wt);
			}
			
			mushrooms = UnityEngine.Random.Range(0, 7); //0-6
		}
		
		public override bool generate(List<GameObject> li) {
			bool flag = base.generate(li);
			
			li.AddRange(gennedMushrooms);
			
			return flag;
		}
		
		protected override VanillaFlora selectPlant(VanillaFlora choice) {
			while (!isPlantAllowed(choice))
				choice = plants.getRandomEntry();
			return choice;
		}
		
		private bool isPlantAllowed(VanillaFlora vf) {
			if (vf == VanillaFlora.BLOOD_KELP && !allowKelp)
				return false;
			VoidPlant vp = plantTypes[vf];
			if (-position.y < vp.minDepth || -position.y > vp.maxDepth)
				return false;
			return true;
		}
		
		protected override GameObject generatePlant(Vector3 vec, string type) {
			//VoidSpike.LargeWorldLevelPrefab prefab = VoidSpike.getPrefab(type);
			GameObject go = base.generatePlant(vec, type);
			if (!VanillaFlora.BLOOD_KELP.includes(type) && !VanillaFlora.AMOEBOID.includes(type) && !VanillaFlora.BRINE_LILY.includes(type)) {
				for (int i = 0; i < mushrooms; i++) {
					Vector3 vec2 = new Vector3(vec.x+UnityEngine.Random.Range(-1F, 1F), vec.y, vec.z+UnityEngine.Random.Range(-1F, 1F));
					int tries = 0;
					while ((ObjectUtil.objectCollidesPosition(go, vec2) || isColliding(vec2, gennedMushrooms)) && tries < 5) {
						vec2 = new Vector3(vec.x+UnityEngine.Random.Range(-1F, 1F), vec.y, vec.z+UnityEngine.Random.Range(-1F, 1F));
						tries++;
					}
					if (!ObjectUtil.objectCollidesPosition(go, vec2) && !isColliding(vec2, gennedMushrooms)) {
						if (validPlantPosCheck != null && !validPlantPosCheck(vec2+Vector3.up*0.2F, "mush"))
							continue;
						GameObject go2 = base.generatePlant(vec2, VanillaFlora.DEEP_MUSHROOM.getRandomPrefab(true));
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
		
		private class VoidPlant {
			
			internal readonly VanillaFlora plant;
			internal readonly float minDepth;
			internal readonly float maxDepth;
			internal readonly float minDepthWeightPoint;
			internal readonly float maxDepthWeightPoint;
			private readonly double weightAtMin;
			private readonly double weightAtMax;
			
			internal VoidPlant(VanillaFlora vf, double wt) : this(vf, 0, 5000, wt) {
				
			}
			
			internal VoidPlant(VanillaFlora vf, float min, float max, double wt) : this(vf, min, max, min, max, wt, wt) {
				
			}
			
			internal VoidPlant(VanillaFlora vf, float min, float max, float minp, float maxp, double wtMin, double wtMax) {
				plant = vf;
				minDepth = min;
				maxDepth = max;
				minDepthWeightPoint = minp;
				maxDepthWeightPoint = maxp;
				weightAtMin = wtMin;
				weightAtMax = wtMax;
			}
			
			internal double getWeight(double y) {
				double depth = -y;
				if (depth < minDepth || depth > maxDepth)
					return 0;
				if (depth <= minDepthWeightPoint)
					return weightAtMin;
				if (depth >= maxDepthWeightPoint)
					return weightAtMax;
				return MathUtil.linterpolate(depth, minDepthWeightPoint, maxDepthWeightPoint, weightAtMin, weightAtMax);
			}
			
		}
		
	}
}

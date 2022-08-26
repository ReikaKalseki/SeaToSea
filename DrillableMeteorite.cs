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
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class DrillableMeteorite : Spawnable {
		
		private static readonly WeightedRandom<TechType> drops = new WeightedRandom<TechType>();
		
		private static readonly float DURATION = 200;
		
		static DrillableMeteorite() {
			drops.addEntry(TechType.Titanium, 400);
			drops.addEntry(TechType.Quartz, 250);
			drops.addEntry(TechType.Copper, 200);
			drops.addEntry(TechType.Nickel, 150);
			drops.addEntry(TechType.Lead, 150);
			drops.addEntry(TechType.Silver, 100);
			drops.addEntry(TechType.Gold, 50);
			drops.addEntry(TechType.UraniniteCrystal, 50);
			//drops.addEntry(TechType.Diamond, 25);
			drops.addEntry(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 10);
		}
	        
		internal DrillableMeteorite() : base("DrillableMeteorite", "Meteorite", "A large chunk of rock and metal originating from space. Rich in minerals.") {
			
	    }
		
		public void register() {
			Patch();
			SNUtil.addPDAEntry(this, 20, PDAManager.getPage("meteorite"));
		}
		
		public static GameObject getRandomResource() {
			return CraftData.GetPrefabForTechType(drops.getRandomEntry(), true);
		}
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(VanillaResources.LARGE_QUARTZ.prefab, true, false);
			if (world != null) {
				world.SetActive(false);
				world.EnsureComponent<TechTag>().type = TechType;
				world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				MeshRenderer[] r = world.GetComponentsInChildren<MeshRenderer>();
				for (int i = 1; i < r.Length; i++) {
					UnityEngine.Object.DestroyImmediate(r[i].gameObject);
				}
				SphereCollider sc = r[0].gameObject.EnsureComponent<SphereCollider>();
				sc.radius = 20F;
				sc.center = Vector3.zero;
				world.EnsureComponent<Meteorite>();
				Drillable dr = world.EnsureComponent<Drillable>();
				dr.Start();
				dr.primaryTooltip = "Metal-rich Meteorite";
				dr.secondaryTooltip = "Source of many mineral varieties.";
				dr.minResourcesToSpawn = 1;
				dr.maxResourcesToSpawn = 1;
				dr.deleteWhenDrilled = false;
				dr.kChanceToSpawnResources = 1;
				world.SetActive(true);
				dr.onDrilled += (d) => {
					//SNUtil.writeToChat("Finished drilling "+d.health.Length+"|"+string.Join(",", d.health));
					d.health[0] = DURATION;
					d.GetComponentsInChildren<MeshRenderer>(true)[0].gameObject.SetActive(true);
				};
				return world;
			}
			else {
				SNUtil.writeToChat("Could not fetch template GO for "+this);
				return null;
			}
	    }
		
		class Meteorite : MonoBehaviour {
			
			private Drillable drill;
			private GameObject innerObject;
			
			void Update() {
				if (!drill || !innerObject) {
					drill = gameObject.GetComponent<Drillable>();
					innerObject = drill.GetComponentsInChildren<MeshRenderer>(true)[0].gameObject;
				}
				if (drill.health[0] <= 0 || !innerObject.activeSelf) {
					drill.health[0] = DURATION;
					innerObject.SetActive(true);
				}
			}
			
		}
			
	}
}

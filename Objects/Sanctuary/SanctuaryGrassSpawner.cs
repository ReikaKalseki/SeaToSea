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
	
	public class SanctuaryGrassSpawner : Spawnable {
		
		internal static readonly Simplex3DGenerator densityNoise = (Simplex3DGenerator)new Simplex3DGenerator(23764311).setFrequency(0.25);
	        
		internal SanctuaryGrassSpawner() : base("SanctuaryGrassSpawner", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = new GameObject();
			go.EnsureComponent<CrashZoneSanctuaryGrassSpawnerTag>();
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			return go;
	    }
		
		class CrashZoneSanctuaryGrassSpawnerTag : MonoBehaviour {
			
			private float age;
			
			void Update() {
				if (Vector3.Distance(Player.main.transform.position, transform.position) < 100)
					age += Time.deltaTime;
				if (age < 2)
					return;
				RaycastHit? at = WorldUtil.getTerrainVectorAt(transform.position, 90);
				if (!at.HasValue) {
					//SNUtil.log("Grass spawner @ "+transform.position+" not finding ground");
					age = 0;
					return;
				}
				List<RaycastHit> li = WorldUtil.getTerrainMountedPositionsAround(at.Value.point, 24F, 900);
				if (li.Count < 30) {
					//SNUtil.log("Grass spawner @ "+at.Value.point+" found too few hits, only "+li.Count);
					age = 0;
					return;
				}
				int i = 0;
				foreach (RaycastHit hit in li) {
					if (Vector3.Angle(hit.normal, Vector3.up) >= 30)
						continue;
					GameObject go;
					if (UnityEngine.Random.Range(0F, 1F) <= 0.3F) {
						go = ObjectUtil.createWorldObject(SeaToSeaMod.sanctuaryGrassBump.ClassID);
						go.transform.position = hit.point;
						go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
						go.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0F, 360F), 0), Space.Self);	
						go.transform.localScale = new Vector3(UnityEngine.Random.Range(1.5F, 3.2F), 0.25F, UnityEngine.Random.Range(1.5F, 3.2F));
					}
					
					if (i >= 450 || densityNoise.getValue(hit.point) <= 0.2)
						continue;
					go = ObjectUtil.createWorldObject(SeaToSeaMod.crashSanctuaryGrass.ClassID);
					go.transform.position = hit.point;
					go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
					go.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0F, 360F), 0), Space.Self);	
					go.transform.position = go.transform.position+go.transform.up*-0.25F;
					
					i++;
				}
				
				li = WorldUtil.getTerrainMountedPositionsAround(at.Value.point, CrashZoneSanctuaryBiome.biomeRadius/2, 10);
				li.Clear();
				foreach (RaycastHit hit in li) {
					GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.sanctuaryCoral.ClassID);
					go.transform.position = hit.point;
					go.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
					go.transform.localScale = new Vector3(10, 1, 10);
				}
				
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			
		}
			
	}
}

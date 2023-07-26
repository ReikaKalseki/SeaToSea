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
	
	public class CrashZoneSanctuarySpawner : Spawnable {
		
		private static readonly WeightedRandom<PrefabReference> prefabs = new WeightedRandom<PrefabReference>();
		
		static CrashZoneSanctuarySpawner() {
			prefabs.addEntry(VanillaFlora.HORNGRASS, 60);
			prefabs.addEntry(VanillaFlora.REGRESS, 40);
			prefabs.addEntry(VanillaFlora.ACID_MUSHROOM, 120);
			prefabs.addEntry(VanillaFlora.BLUE_PALM, 60);
			prefabs.addEntry(VanillaFlora.GELSACK, 30);
			prefabs.addEntry(VanillaFlora.PAPYRUS, 40);
			prefabs.addEntry(VanillaFlora.SPOTTED_DOCKLEAF, 60);
			prefabs.addEntry(VanillaFlora.VEINED_NETTLE, 40);
			prefabs.addEntry(VanillaFlora.ROUGE_CRADLE, 10);
			prefabs.addEntry(VanillaResources.SHALE, 10);
			prefabs.addEntry(VanillaResources.SANDSTONE, 30);
			prefabs.addEntry(VanillaResources.LIMESTONE, 60);
		}
	        
		internal CrashZoneSanctuarySpawner() : base("CrashZoneSanctuarySpawner", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = new GameObject();
			go.EnsureComponent<CrashZoneSanctuarySpawnerTag>();
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			return go;
	    }
		
		class CrashZoneSanctuarySpawnerTag : MonoBehaviour {
			
			private float age;
			
			void Update() {/*
				if (CrashZoneSanctuaryBiome.instance.isInBiome(transform.position)) {
					string pfb = CrashZoneSanctuarySpawner.prefabs.getRandomEntry().getPrefabID();
					GameObject go = pfb == null ? null : ObjectUtil.lookupPrefab(pfb);
					if (go) {
						go = UnityEngine.Object.Instantiate(go);
						go.transform.position = transform.position;
						go.transform.rotation = transform.rotation;
						go.transform.SetParent(transform.parent);
					}
				}*/
				if (Vector3.Distance(Player.main.transform.position, transform.position) < 100)
					age += Time.deltaTime;
				if (age < 4)
					return;
				for (int i = 0; i < 2400; i++) {
					Vector3 pos = MathUtil.getRandomVectorAround(transform.position, new Vector3(CrashZoneSanctuaryBiome.biomeRadius, 0, CrashZoneSanctuaryBiome.biomeRadius)).setY(-300);
					if (!CrashZoneSanctuaryBiome.instance.isInBiome(pos))
						continue;
					Ray ray = new Ray(pos, Vector3.down);
					int found = UWE.Utils.RaycastIntoSharedBuffer(ray, 90, Voxeland.GetTerrainLayerMask());
					SNUtil.log("Ray at "+pos+" found: "+found+" : "+(found > 0 ? UWE.Utils.sharedHitBuffer[0].point.ToString() : "null"));
					if (found > 0) {
						RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
						if (hit.transform != null) {
							pos = hit.point;
							bool tilted = Vector3.Angle(transform.up, Vector3.up) >= 20;
							PrefabReference pfb = CrashZoneSanctuarySpawner.prefabs.getRandomEntry();
							if (tilted) {
								while (pfb is VanillaFlora) {
									pfb = CrashZoneSanctuarySpawner.prefabs.getRandomEntry();
								}
							}
							bool plant = pfb is VanillaFlora;
							GameObject go = pfb == null ? null : ObjectUtil.lookupPrefab(pfb.getPrefabID());
							if (go) {
								if (plant) {
									int nn = UnityEngine.Random.Range(1, 5);
									for (int n = 0; n < nn; n++) {
										Vector3 pos2 = MathUtil.getRandomVectorAround(pos, new Vector3(5, 0, 5)).setY(pos.y+5);
										ray = new Ray(pos2, Vector3.down);
										found = UWE.Utils.RaycastIntoSharedBuffer(ray, 15, Voxeland.GetTerrainLayerMask());
										if (found > 0 && Vector3.Angle(UWE.Utils.sharedHitBuffer[0].normal, Vector3.up) < 20) {
											go = UnityEngine.Object.Instantiate(go);
											go.transform.position = UWE.Utils.sharedHitBuffer[0].point;
											go.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360F), 0);
											go.transform.SetParent(transform.parent);
										}
									}
								}
								else {
									go = UnityEngine.Object.Instantiate(go);
									go.transform.position = pos;
									go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
									go.transform.SetParent(transform.parent);
								}
							}
						}
					}
				}
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			
		}
			
	}
}

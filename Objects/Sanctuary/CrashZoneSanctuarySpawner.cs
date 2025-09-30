using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class CrashZoneSanctuarySpawner : Spawnable {

		private static readonly WeightedRandom<SpawnedPlant> plants = new WeightedRandom<SpawnedPlant>();
		private static HashSet<string> plantIDs = new HashSet<string>();
		//private static readonly WeightedRandom<SpawnedPrefab> resources = new WeightedRandom<SpawnedPrefab>();

		static CrashZoneSanctuarySpawner() {
			addPlant(new SpawnedPlant(VanillaFlora.HORNGRASS, 2).setAngle(0.125F), 60);
			addPlant(new SpawnedPlant(VanillaFlora.ACID_MUSHROOM, 0.5F, 7.5F).setRadiusScale(0.33F).setAngle(0.75F).setModify(go => go.transform.Rotate(new Vector3(-90, 0, 0), Space.Self)), 120);
			addPlant(new SpawnedPlant(VanillaFlora.GELSACK, 1.2F, 1.5F).setRadiusScale(0.5F).setAngle(1).setModify(go => go.transform.Rotate(new Vector3(-90, 0, 0), Space.Self)), 30);
			addPlant(new SpawnedPlant(VanillaFlora.PAPYRUS, 2, 1.5F).setAngle(0.25F), 40);
			addPlant(new SpawnedPlant(VanillaFlora.SPOTTED_DOCKLEAF, 2, 1.9F).setRadiusScale(0.8F), 60);

			//resources.addEntry(new SpawnedPrefab(VanillaResources.SHALE, 0.8F), 10);
			//resources.addEntry(new SpawnedPrefab(VanillaResources.SANDSTONE, 0.6F), 30);
			//resources.addEntry(new SpawnedPrefab(VanillaResources.LIMESTONE, 0.4F), 60);
		}

		internal CrashZoneSanctuarySpawner() : base("CrashZoneSanctuarySpawner", "", "") {

		}

		private static void addPlant(SpawnedPlant p, double weight) {
			plants.addEntry(p, weight);
			plantIDs.AddRange(p.prefab.getPrefabs(true, true));
		}

		public static bool spawnsPlant(string id) {
			return plantIDs.Contains(id);
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

			void Update() {
				if (Vector3.Distance(Player.main.transform.position, transform.position) < 100)
					age += Time.deltaTime;
				if (age < 2)
					return;
				SNUtil.log("Spawning sanctuary plants @ " + transform.position);
				List<Vector3> ends = new List<Vector3>();
				//List<RaycastHit> terrainHits = new List<RaycastHit>();
				UnityEngine.Random.InitState(SNUtil.getWorldSeedInt());
				for (int i = 0; i < 60; i++) {
					Vector3 pos = MathUtil.getRandomVectorAround(transform.position, new Vector3(CrashZoneSanctuaryBiome.biomeRadius, 0, CrashZoneSanctuaryBiome.biomeRadius)).setY(-300);
					RaycastHit? root = WorldUtil.getTerrainVectorAt(pos, 90);
					if (!root.HasValue) {
						i--;
						//SNUtil.log("Skipped eye flame location: no terrain");
						continue;
					}
					pos = root.Value.point;
					if (Vector3.Angle(root.Value.normal, Vector3.up) > 20 || !CrashZoneSanctuaryBiome.instance.isInBiome(pos)) {
						i--;
						//SNUtil.log("Skipped eye flame location: "+Vector3.Angle(root.Value.normal, Vector3.up)+" & "+CrashZoneSanctuaryBiome.instance.isInBiome(pos));
						continue;
					}
					//terrainHits.Add(root.Value);
					bool close = false;
					foreach (Vector3 has in ends) {
						if (Vector3.Distance(has, pos) < 12) {
							close = true;
							break;
						}
					}
					if (close) {
						i--;
						//SNUtil.log("Skipped eye flame location: too close");
						continue;
					}
					ends.Add(pos);
					foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(pos, 2.5F)) {
						pi.gameObject.destroy();
					}
					GameObject go = ObjectUtil.createWorldObject(C2CItems.sanctuaryPlant.ClassID);
					go.transform.position = pos;
					go.transform.rotation = MathUtil.unitVecToRotation(root.Value.normal);
					go.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0F, 360F), 0), Space.Self);
					SpawnedPlant pfb = CrashZoneSanctuarySpawner.plants.getRandomEntry();
					int amt = (int)(UnityEngine.Random.Range(9, 19)*pfb.countScale*1.25F);
					List<RaycastHit> li = WorldUtil.getTerrainMountedPositionsAround(pos, 12F*pfb.radiusScale, amt);
					//SNUtil.log("Found sanctuary hits @ "+pos+" "+li.toDebugString());
					List<Vector3> spawned = new List<Vector3>();
					foreach (RaycastHit hit in li) {
						close = false;
						foreach (Vector3 has in spawned) {
							if (Vector3.Distance(has, hit.point) < pfb.minSeparation) {
								close = true;
								//SNUtil.log("too close to another point "+has+", cancelling "+hit.point);
								break;
							}
						}
						if (close)
							continue;
						pfb.spawn(hit).transform.SetParent(transform.parent);
						spawned.Add(hit.point);
						if (spawned.Count == 0) {
							age = 0;
							return;
						}
					}
					//SNUtil.log("Spawned sanctuary hits @ "+pos+" "+spawned.toDebugString());
				}/*
				foreach (RaycastHit hit in terrainHits) {
					Renderer[] rr = hit.transform.parent.GetComponentsInChildren<Renderer>();
					SNUtil.writeToChat(rr.toDebugString());
				}*/
				/* 
				for (float i = -CrashZoneSanctuaryBiome.biomeRadius; i <= CrashZoneSanctuaryBiome.biomeRadius; i += 0.25F) {
					for (float k = -CrashZoneSanctuaryBiome.biomeRadius; k <= CrashZoneSanctuaryBiome.biomeRadius; k += 0.25F) {
						Vector3 pos = MathUtil.getRandomVectorAround(CrashZoneSanctuaryBiome.biomeCenter+new Vector3(i, 30, k), 0.25F);
						if (CrashZoneSanctuaryBiome.instance.isInBiome(pos)) {
							RaycastHit? hit = WorldUtil.getTerrainVectorAt(pos, 90);
							if (hit.HasValue) {
								GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.crashSanctuaryGrass.ClassID);
								go.transform.position = hit.Value.point;
								go.transform.rotation = MathUtil.unitVecToRotation(hit.Value.normal);
								go.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0F, 360F), 0), Space.Self);
							}
						}
					}
				}*/
				gameObject.destroy();
			}

		}

		class SpawnedPlant {

			internal readonly VanillaFlora prefab;
			internal readonly float minSeparation;
			internal readonly float countScale;

			internal float angleFactor = 0;
			internal float radiusScale = 1;
			internal Action<GameObject> modify = null;

			internal SpawnedPlant(VanillaFlora pr, float r, float cs = 1) {
				prefab = pr;
				minSeparation = r;
				countScale = cs;
			}

			internal SpawnedPlant setModify(Action<GameObject> a) {
				modify = a;
				return this;
			}

			internal SpawnedPlant setAngle(float a) {
				angleFactor = a;
				return this;
			}

			internal SpawnedPlant setRadiusScale(float r) {
				radiusScale = r;
				return this;
			}

			internal GameObject spawn(RaycastHit hit) {
				GameObject go = ObjectUtil.lookupPrefab(prefab.getRandomPrefab(true)).clone();
				go.transform.position = hit.point;
				go.transform.rotation = angleFactor > 0 ? MathUtil.unitVecToRotation((hit.normal * angleFactor) + ((1 - angleFactor) * Vector3.up)) : Quaternion.identity;
				go.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0F, 360F), 0), Space.Self);
				if (modify != null)
					modify.Invoke(go);
				return go;
			}

		}

	}
}

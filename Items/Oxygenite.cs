using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	public class Oxygenite : BasicCustomOre {

		public static readonly List<PositionedPrefab> spawns = new List<PositionedPrefab>();

		public Oxygenite(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			collectSound = "event:/loot/pickup_quartz";
			inventorySize = new Vector2int(1, 2);
		}

		public override void prepareGameObject(GameObject go, Renderer[] r0) {
			base.prepareGameObject(go, r0);
			setupOxygeniteRender(go);
		}

		public static void setupOxygeniteRender(GameObject go, float lightRadius = 1) {
			foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
				//GameObject go = ;
				r.materials[0].SetFloat("_Fresnel", 1.0F);
				r.materials[0].SetFloat("_Shininess", 5.5F);
				r.materials[0].SetFloat("_SpecInt", 60F);
				r.materials[0].SetFloat("_IBLreductionAtNight", 0);
				RenderUtil.swapToModdedTextures(r, CustomMaterials.getItem(CustomMaterials.Materials.OXYGENITE));
			}
			Color c = new Color(0.5F, 1, 0.9F);
			Light l = go.addLight(1F, 4*lightRadius, c);
			l.type = LightType.Point;
			l.transform.localPosition = Vector3.up;
			l = go.addLight(2.5F, 1F* lightRadius, c);
			l.type = LightType.Point;
			l.transform.localPosition = Vector3.up;
			go.EnsureComponent<OxygeniteTag>();
		}
		/*
		public static void spawnAt(Vector3 from) {
			List<Vector3> spawned = new List<Vector3>();
			for (int i = 0; i < 300; i++) {
				Ray ray = new Ray(MathUtil.getRandomVectorAround(UnderwaterIslandsFloorBiome.biomeCenter, new Vector3(200, 0, 200)).setY(-400), Vector3.down);
				if (UWE.Utils.RaycastIntoSharedBuffer(ray, 200, Voxeland.GetTerrainLayerMask()) > 0) {
					RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
					if (hit.transform != null && Mathf.Abs(Vector3.Angle(hit.normal, Vector3.up)) < 30) {
						if (WorldUtil.getObjectsNearWithComponent<OxygeniteTag>(hit.point, 30).Count > 0 || spawns.Any(pfb => Vector3.Distance(pfb.position, hit.point) < 30))
							continue;
						GameObject go = ObjectUtil.createWorldObject(C2CItems.largeOxygenite.ClassID);
						go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
						go.transform.position = hit.point;
						spawned.Add(hit.point);
						spawns.Add(new PositionedPrefab(go.GetComponent<PrefabIdentifier>()));
					}
				}
			}
			SNUtil.writeToChat("Spawned oxygenite: "+spawned.toDebugString());
		}
		*/
		public static void dumpLocations() {
			string file = BuildingHandler.instance.dumpPrefabs("oxygeniteSpawns", spawns);
			SNUtil.writeToChat("Exported " + spawns.Count + " oxygenite to " + file);
		}
	}

	public class OxygeniteTag : MonoBehaviour {

		void Start() {
			
		}

	}
}

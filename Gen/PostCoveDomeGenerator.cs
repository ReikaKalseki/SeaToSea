using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

	public class PostCoveDomeGenerator : WorldGenerator {

		internal static readonly Spawnable coolResourceDome = new ResourceDome(false);
		internal static readonly Spawnable hotResourceDome = new ResourceDome(true);

		internal static readonly WeightedRandom<string> resourceTableCool = new WeightedRandom<string>();
		internal static readonly WeightedRandom<string> resourceTableHot = new WeightedRandom<string>();

		private Quaternion rotation;

		static PostCoveDomeGenerator() {
			resourceTableCool.addEntry(VanillaResources.RUBY.prefab, 50);
			resourceTableCool.addEntry(VanillaResources.DIAMOND.prefab, 30);
			resourceTableCool.addEntry(VanillaResources.QUARTZ.prefab, 70);
			resourceTableCool.addEntry(CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).ClassID, 20);

			resourceTableHot.addEntry(VanillaResources.RUBY.prefab, 20);
			resourceTableHot.addEntry(VanillaResources.DIAMOND.prefab, 50);
			resourceTableHot.addEntry(VanillaResources.QUARTZ.prefab, 60);
			resourceTableHot.addEntry(CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).ClassID, 20);
			resourceTableHot.addEntry(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).ClassID, 5);

			coolResourceDome.Patch();
			hotResourceDome.Patch();
		}

		class ResourceDome : Spawnable {

			private readonly bool isHot;

			internal ResourceDome(bool hot) : base("PostCoveResourceDome_" + hot, "", "") {
				isHot = hot;
			}

			public override GameObject GetGameObject() {
				GameObject go = ObjectUtil.createWorldObject(VanillaResources.SANDSTONE.prefab);
				GameObject mdl = ObjectUtil.lookupPrefab(VanillaFlora.AMOEBOID.getPrefabID()).getChildObject("lost_river_plant_04/lost_river_plant_04_membrane");
				go.EnsureComponent<ResourceDomeTag>().isHot = isHot;
				go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				go.EnsureComponent<TechTag>().type = TechType;
				go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
				Renderer[] rs = go.GetComponentsInChildren<Renderer>();
				foreach (Renderer r in rs) {
					Renderer r2 = r.setModel(mdl).GetComponentInChildren<Renderer>();
					r2.transform.localPosition = new Vector3(0, (-0.05F * 0) + 0.02F, 0);
					r2.transform.localEulerAngles = new Vector3(-90, 0, 0);
					RenderUtil.swapTextures(SeaToSeaMod.modDLL, r2, "Textures/Plants/PostCoveTree/Res_" + (isHot ? "Hot" : "Cold"));
					PostCoveDome.setupRenderGloss(r2);
					RenderUtil.disableTransparency(r2.materials[0]);
					r2.materials[0].SetFloat("_SpecInt", isHot ? 7.5F : 2F);
					RenderUtil.setEmissivity(r2, 2F);
				}
				go.removeComponent<ResourceTracker>();
				go.GetComponent<VFXSurface>().surfaceType = VFXSurfaceTypes.glass;
				return go;
			}

		}

		internal class ResourceDomeTag : MonoBehaviour {

			internal bool isHot;
			private BreakableResource res;

			private SkyApplier[] skies;
			private Renderer[] renderers;

			internal float growFade;

			void Start() {
				res = this.GetComponent<BreakableResource>();
				res.numChances = 0; //use own drop code
				res.hitsToBreak = UnityEngine.Random.Range(5, 9); //5-8 hits
				res.breakText = "BreakPostCoveDomeResource"; //locale key
				res.customGoalText = "BreakPostCoveDomeResource"; //StoryGoal
				res.defaultPrefab = ObjectUtil.lookupPrefab(SeaToSeaMod.geogelFogDrip.ClassID);//this is lead for sandstone, but we made it an FX
				this.GetComponentInChildren<Renderer>().transform.localScale = 0.3F * new Vector3(UnityEngine.Random.Range(0.9F, 1.1F), UnityEngine.Random.Range(0.9F, 1.1F), UnityEngine.Random.Range(0.6F, 1.4F));
				skies = this.GetComponentsInChildren<SkyApplier>(true);
				renderers = this.GetComponentsInChildren<Renderer>();
				this.InvokeRepeating("setupSky", 0, 1F);
				this.Invoke("refinePosition", 2.5F);
				this.Invoke("refinePosition", 5F);
				this.Invoke("refinePosition", 10F);
				isHot = transform.position.y < PostCoveDome.HOT_THRESHOLD;
			}

			void Update() {
				growFade = Mathf.Clamp01(growFade - Time.deltaTime);
				foreach (Renderer r in renderers) {
					r.transform.localPosition = Vector3.down * 0.25F * growFade;
				}
				res.enabled = growFade <= 0;
			}

			void refinePosition() {
				Vector3 pos = transform.position+(transform.up*5);
				Vector3 vec = -transform.up*15;
				Ray ray = new Ray(pos, vec);
				if (UWE.Utils.RaycastIntoSharedBuffer(ray, vec.magnitude, Voxeland.GetTerrainLayerMask()) > 0) {
					RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
					if (hit.transform != null) {
						transform.rotation = MathUtil.unitVecToRotation(hit.normal);
						transform.position = hit.point;
					}
				}
			}

			void setupSky() {
				int idx = WaterBiomeManager.main.GetBiomeIndex(isHot ? /*"ILZCorridor"*/"ILZChamber" : "LostRiver_TreeCove");
				foreach (SkyApplier sk in skies) {
					if (!sk)
						continue;
					sk.renderers = renderers;
					gameObject.setSky(WaterBiomeManager.main.biomeSkies[idx]);
				}
			}

			void OnBreakResource() {
				WeightedRandom<string> wr = isHot ? resourceTableHot : resourceTableCool;
				res.SpawnResourceFromPrefab(ObjectUtil.lookupPrefab(wr.getRandomEntry())); //use their spawn code
			}

		}

		public PostCoveDomeGenerator(Vector3 pos) : base(pos) {

		}

		public override void saveToXML(XmlElement e) {
			PositionedPrefab.saveRotation(e, rotation);
		}

		public override void loadFromXML(XmlElement e) {
			rotation = PositionedPrefab.readRotation(e);
		}

		public override bool generate(List<GameObject> li) {
			GameObject go = spawner(SeaToSeaMod.postCoveDome.ClassID);
			go.transform.position = position;
			go.transform.rotation = rotation;
			li.Add(go);

			HashSet<Vector3> placed = new HashSet<Vector3>();
			int failed = 0;
			for (int i = 0; i < 24; i++) {
				GameObject go2 = placeRandomResourceDome(go, placed, spawner);
				if (go2) {
					li.Add(go2);
					placed.Add(go2.transform.position);
				}
				else {
					i--;
					failed++;
					if (failed > 50)
						break;
					else
						continue;
				}
			}
			return go && placed.Count > 3;
		}

		public override LargeWorldEntity.CellLevel getCellLevel() {
			return LargeWorldEntity.CellLevel.Far;
		}

		public static GameObject placeRandomResourceDome(GameObject from, IEnumerable<Vector3> avoid, Func<string, GameObject> spawner) {
			Vector3 pos = MathUtil.getRandomVectorAround(from.transform.position + (from.transform.up * 5), 6);
			Vector3 vec = -from.transform.up * 15;
			Ray ray = new Ray(pos, vec);
			if (UWE.Utils.RaycastIntoSharedBuffer(ray, vec.magnitude, Voxeland.GetTerrainLayerMask()) > 0) {
				RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
				if (hit.transform != null) {
					if ((hit.point - from.transform.position).sqrMagnitude < 9)
						return null;
					foreach (Vector3 at in avoid) {
						if ((at - hit.point).sqrMagnitude < 0.5) {
							return null;
						}
					}
					Spawnable rt = from.transform.position.y < PostCoveDome.HOT_THRESHOLD ? hotResourceDome : coolResourceDome;
					GameObject go2 = spawner(rt.ClassID);
					go2.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
					go2.transform.position = hit.point;
					go2.transform.RotateAroundLocal(go2.transform.up, UnityEngine.Random.Range(0F, 360F));
					return go2;
				}
			}
			return null;
		}

	}
}

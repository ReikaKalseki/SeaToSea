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

	public class VentKelp : BasicCustomPlant, MultiTexturePrefab {

		public static float minTemperature = 25;
		public static float idealTemperature = 60;
		public static float maxTemperature = 150;

		internal static readonly Simplex3DGenerator noiseField = (Simplex3DGenerator)new Simplex3DGenerator(DateTime.Now.Ticks).setFrequency(0.1);
		internal static readonly SimplexNoiseGenerator heightNoiseField = (SimplexNoiseGenerator)new SimplexNoiseGenerator(873428712).setFrequency(1.0);

		private static readonly string CHILD_NAME = "column_plant_";
		private static readonly string CHILD_NAME_2 = "leaf_aux_plant_";

		public static readonly float GROWTH_TIME = 1800;

		private static bool leavesOnlyRendering;

		public VentKelp() : base(SeaToSeaMod.itemLocale.getEntry("VENT_KELP"), new FloraPrefabFetch(VanillaFlora.FERN_PALM), "afba45cf-00f9-4d80-a203-429d6ce7ff62", "Samples") {
			glowIntensity = 0.8F;
			finalCutBonus = 2;
		}

		internal static double getGrowthSpeed(float temp) {
			return temp <= idealTemperature ? MathUtil.linterpolate(temp, minTemperature, idealTemperature, 0, 1, true) : MathUtil.linterpolate(temp, idealTemperature, maxTemperature, 1, 0, true);
		}

		public override Vector2int SizeInInventory {
			get { return new Vector2int(1, 1); }
		}

		public override void prepareGameObject(GameObject go, Renderer[] r0) {
			base.prepareGameObject(go, r0);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
			GlowKelpTag g = go.EnsureComponent<GlowKelpTag>();
			float h = 0;/*
			int n = (int)Math.Min(9, 3+((1+heightNoiseField.getValue(go.transform.position))*8));
			for (int i = 0; i < n; i++) {
				string pfb = VanillaFlora.FERN_PALM.getRandomPrefab(false);
				string nm = CHILD_NAME+i;
				GameObject child = getOrCreateSubplant(pfb, go, h, nm);
				prepareSubplant(child);
				
				float dh = pfb.StartsWith("1d6d8", StringComparison.InvariantCultureIgnoreCase) ? 0.55F : 0.9F;//1.25F;
				h += dh;//*UnityEngine.Random.Range(0.2F, 1F);
			}
			*/
			float maxH = (float)Math.Min(9, 3+((1+heightNoiseField.getValue(go.transform.position))*8));//h;
			h = 0;
			int i0 = 0;
			while (h < maxH - 1) {
				string pfb = VanillaFlora.CAVE_BUSH.getRandomPrefab(false);
				string nm = CHILD_NAME_2+i0;
				GameObject child = this.getOrCreateSubplant(pfb, go, h, nm);
				this.prepareSubplant(child, true);
				h += UnityEngine.Random.Range(0.75F, 1.5F);
				i0++;
			}
			go.removeChildObject("land_plant_middle_03_01");
			go.removeChildObject("land_plant_middle_03_02");
			go.removeChildObject("coral_reef_plant_middle_12");
			go.layer = LayerID.Useable;
			foreach (Collider c in go.GetComponentsInChildren<Collider>(true)) {
				c.isTrigger = true;
			}
			go.EnsureComponent<SonarOnlyRenderer>();
		}

		private GameObject getOrCreateSubplant(string pfb, GameObject go, float h, string nm) {
			GameObject child = go.getChildObject(nm);
			if (!child) {
				child = ObjectUtil.createWorldObject(pfb);
				child.name = nm;
				child.transform.parent = go.transform;
				child.transform.localPosition = Vector3.up * h;
				child.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(0F, 360F), 0);
				child.transform.localScale = Vector3.one;
			}
			return child;
		}

		private void prepareSubplant(GameObject child, bool leavesOnly = false) {
			child.removeComponent<LargeWorldEntity>();
			child.removeComponent<TechTag>();
			child.removeComponent<PrefabIdentifier>();

			foreach (Renderer r in child.GetComponentsInChildren<Renderer>(true)) {
				leavesOnlyRendering = leavesOnly;
				if (leavesOnly) {
					r.materials[0].color = new Color(0, 0, 0, 0);
					//child.transform.localScale = new Vector3(1.1F, 1.5F, 1.1F);
					r.materials[0].DisableKeyword("MARMO_EMISSION");
					r.materials[0].DisableKeyword("MARMO_SPECMAP");
				}
				else {
					r.materials[0].EnableKeyword("FX_KELP");
					r.materials[0].SetVector("_Scale", new Vector4(0.7F, 0.4F, 0.8F, 0.4F));
					r.materials[0].SetVector("_Frequency", new Vector4(0.16F, 0.1F, 0.12F, 0.6F));
					r.materials[0].SetVector("_Speed", new Vector4(1F, 0.8F, 0.0F, 0.0F));
					r.materials[0].SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
					r.materials[0].SetFloat("_WaveUpMin", 0.4F);
					r.materials[0].SetFloat("_minYpos", 1F);
					r.materials[0].SetFloat("_maxYpos", 0F);
				}

				r.materials[1].EnableKeyword("FX_KELP");
				r.materials[1].SetFloat("_minYpos", 1F);
				r.materials[1].SetFloat("_maxYpos", 0F);
				if (leavesOnly) {
					r.materials[1].SetVector("_Scale", new Vector4(0.5F, 0.3F, 0.5F, 0.4F));
					r.materials[1].SetVector("_Frequency", new Vector4(0.16F, 0.1F, 0.12F, 0.6F));
					r.materials[1].SetVector("_Speed", new Vector4(0.5F, 0.6F, 0.0F, 0.0F));
					r.materials[1].SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
					r.materials[1].SetFloat("_WaveUpMin", 0.25F);
				}
				else {
					r.materials[1].SetVector("_Scale", new Vector4(1.1F, 0.6F, 1.3F, 0.6F));
					r.materials[1].SetVector("_Frequency", new Vector4(0.16F, 0.1F, 0.12F, 0.6F));
					r.materials[1].SetVector("_Speed", new Vector4(1F, 0.8F, 0.0F, 0.0F));
					r.materials[1].SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
					r.materials[1].SetFloat("_WaveUpMin", 0.4F);
				}
				r.materials[0].SetColor("_GlowColor", GlowKelpTag.idleColor);
				r.materials[1].SetColor("_GlowColor", GlowKelpTag.idleColor);
				RenderUtil.makeTransparent(r, leavesOnly ? new HashSet<int> { 0, 1 } : new HashSet<int> { 1 });
				RenderUtil.setEmissivity(r, 8, new HashSet<int> { 1 });
				RenderUtil.swapToModdedTextures(r, this);
			}

			Ecocean.PlantHidingCollider.addToObject(child.GetComponentInChildren<Collider>(), new Color(0.75F, 0.3F, 1F));
		}

		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}

		public Dictionary<int, string> getTextureLayers(Renderer r) {
			return leavesOnlyRendering ? new Dictionary<int, string>() { { 0, "" }, { 1, "Leaves" } } : new Dictionary<int, string>() { { 0, "" }, { 1, "" } };
		}
		/*
		public override float getGrowthTime() {
			return GROWTH_TIME;
		}*/
		/*
		public override void prepareGrowingPlant(GrowingPlant g) {
			g.gameObject.EnsureComponent<GrowingGlowKelp>();
		}*/

	}
	/*
	class GrowingGlowKelp : MonoBehaviour {
		
		private GrowingPlant plant;
		
		void Update() {
			if (!plant)
				plant = gameObject.GetComponent<GrowingPlant>();
			Planter p = gameObject.GetComponentInParent<Planter>();
			Vector3 pos = p.gameObject.transform.position;
			float temp = WaterTemperatureSimulation.main.GetTemperature(pos); //this method is patched to have the hooks for hotter ILZ and so on 
			float spd = (float)VentKelp.getGrowthSpeed(temp);
			//SNUtil.writeToChat(temp+"C @ "+pos+" > "+spd);
			if (spd <= 0) {
				plant.growthDuration = 99999;
				plant.timeStartGrowth = DayNightCycle.main.timePassedAsFloat;
			}
			else {
				plant.growthDuration = VentKelp.GROWTH_TIME/spd;
			}
		}
		
	}
	*/
	internal class GlowKelpTag : MonoBehaviour {

		internal static readonly Color idleColor = new Color(0.1F, 0, 0.5F, 1);
		internal static readonly Color activeColor = new Color(0.7F, 0.2F, 1, 1);

		private readonly List<KelpSegment> segments = new List<KelpSegment>();
		private GrownPlant grown;
		private WaterPark acu;
		private bool redoRenderers;
		private float creationTime = 999999;
		private float lastAreaCheckTime = -1;
		private float lastContinuityCheckTime = -1;

		private SonarOnlyRenderer sonarControl;

		private float intensity = 1;

		class KelpSegment {

			internal readonly Renderer renderer;
			//internal readonly Renderer sonarRenderer;
			internal readonly LiveMixin live;
			internal readonly GameObject obj;
			internal readonly int index;

			internal float sonarIntensity = 0;

			internal KelpSegment(Renderer r) {
				renderer = r;
				live = r.gameObject.FindAncestor<LiveMixin>();
				obj = r.transform.parent.gameObject;
				string n = obj.name;
				index = !string.IsNullOrEmpty(n) && n.Contains("leaf_aux") ? int.Parse(n.Substring(n.Length - 1)) : -1;
			}

		}

		void Start() {
			C2CItems.kelp.prepareGameObject(gameObject, null);
			creationTime = DayNightCycle.main.timePassedAsFloat;
			grown = gameObject.GetComponent<GrownPlant>();
			acu = gameObject.FindAncestor<WaterPark>();
			if (grown || acu) {
				gameObject.SetActive(true);
			}
			else {
				gameObject.transform.localScale = new Vector3(12, 12, 12);
				gameObject.transform.rotation = Quaternion.identity;
			}
		}

		void Update() {
			if (!sonarControl)
				sonarControl = this.GetComponent<SonarOnlyRenderer>();
			bool isNew = DayNightCycle.main.timePassedAsFloat-creationTime <= 0.1F;
			if (segments.Count == 0 || isNew || redoRenderers) {
				segments.Clear();
				foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>()) {
					//if (!r.gameObject.name.Contains("Sonar")) {
					KelpSegment s = new KelpSegment(r);
					segments.Add(s);
					//sonarControl.renderers.Add(s.sonarRenderer);
					//}
				}
			}
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = Time.deltaTime;
			if (time - lastAreaCheckTime >= 0.25) {
				lastAreaCheckTime = time;
				intensity = 1 - (float)((VentKelp.noiseField.getValue(gameObject.transform.position + (Vector3.down * DayNightCycle.main.timePassedAsFloat * 7.5F)) + 1) / 2D);
				if (grown || acu) {
					gameObject.transform.localScale = acu && acu.height == 1 ? new Vector3(1.5F, 1.75F, 1.5F) : new Vector3(2, 3.5F, 2);
					if (isNew) {
						foreach (KelpSegment s in segments) {
							if (!s.renderer)
								continue;
							s.renderer.materials[1].SetVector("_Scale", new Vector4(0.05F, 0.0F, 0.05F, 0.0F));
							s.renderer.materials[1].SetVector("_Frequency", new Vector4(0.8F, 0.8F, 0.8F, 0.8F));
							s.renderer.materials[1].SetVector("_Speed", new Vector4(0.2F, 0.2F, 0.2F, 0.2F));
							s.renderer.materials[1].SetFloat("_WaveUpMin", 0F);
						}
					}
				}
				else if (transform.position.y >= -400 || MathUtil.getDistanceToLineSegment(transform.position, UnderwaterIslandsFloorBiome.wreckCtrPos1, UnderwaterIslandsFloorBiome.wreckCtrPos2) <= 15) {
					//SNUtil.writeToChat("Destroying vent kelp @ "+transform.position.y);
					SNUtil.log("Destroying shallow/near-wreck vent kelp @ " + transform.position);
					gameObject.destroy();
					return;
				}
			}
			bool kill = false;
			bool cropped = false;
			float sonarDist = sonarControl.computeSonarDistance();
			foreach (KelpSegment s in segments) {
				if (!s.renderer)
					continue;
				s.sonarIntensity = sonarControl.isBlobVisible(s.renderer, sonarDist, 1.5F)
					? Mathf.Min(1, s.sonarIntensity + (dT * 3.2F))
					: Mathf.Max(0, s.sonarIntensity - (dT * 1F));

				if (s.sonarIntensity > 0) {
					s.renderer.materials[1].EnableKeyword("FX_BUILDING");
					s.renderer.materials[1].SetFloat("_Built", 0.25F);
					s.renderer.materials[1].SetVector("_BuildParams", new Vector4(1, 1, 0.25F, 0.1F));
					s.renderer.materials[1].SetColor("_BorderColor", new Color(4.25F * s.sonarIntensity, 0, 0, 1));
					s.renderer.materials[1].SetFloat("_NoiseThickness", 0.07F);
					s.renderer.materials[1].SetFloat("_NoiseStr", 0.15F);
				}
				else {
					s.renderer.materials[1].DisableKeyword("FX_BUILDING");
				}
				if ((grown || acu) && !isNew) {
					int limit = 2;
					if (acu)
						limit = Math.Max(1, acu.height - 1);
					if (s.index >= limit) {
						cropped = true;
						s.obj.destroy();
						SNUtil.log("Destroying extra farmed vent kelp @ " + transform.position + " #" + s.index);
						continue;
					}
				}
				foreach (Material m in s.renderer.materials) {
					m.SetColor("_GlowColor", Color.Lerp(idleColor, activeColor, (intensity * 1.5F) - 0.5F));
					if (acu) {
						Color c = new Color(0.44F, 0.28F, 0.56F);
						m.SetColor("_Color", c);
						m.SetColor("_SpecColor", c);
					}
				}
			}
			if (cropped) {
				segments.RemoveAll(s => !s.renderer);
			}
			//SNUtil.writeToChat(time.ToString("00.000")+" > "+(time-lastContinuityCheckTime).ToString("00.000"));
			else if (time - lastContinuityCheckTime >= 1 && !isNew) {
				lastContinuityCheckTime = time;
				kill |= segments.Count <= 0;
				foreach (KelpSegment s in segments) {
					if (!s.renderer) {
						kill = true;
						//SNUtil.writeToChat("Detected incomplete vent kelp @ "+transform.position+": "+s.obj.GetFullHierarchyPath());
						continue;
					}
					//float f = (float)Math.Abs(2*VentKelp.noiseField.getValue(r.gameObject.transform.position+Vector3.up*DayNightCycle.main.timePassedAsFloat*7.5F))-0.75F;
					if (!s.live || s.live.health <= 0) {
						kill = true;
						//SNUtil.writeToChat("Detected incomplete vent kelp @ "+transform.position+": "+s.obj.GetFullHierarchyPath());
						continue;
					}
					if (s.obj.transform.position.y >= -3) {
						if (grown)
							s.obj.GetComponentInParent<Planter>().RemoveItem(grown.seed);
						s.obj.destroy();
						SNUtil.log("Destroying surface vent kelp @ " + transform.position + " #" + s.index);
						redoRenderers = true;
						continue;
					}
				}
			}
			if (kill && !isNew) {/*
				Planter p = gameObject.GetComponentInParent<Planter>();
				GrownPlant g = gameObject.GetComponentInParent<GrownPlant>();
				if (p && g) {
					p.RemoveItem(p.GetSlotID(g.seed));
					p.storageContainer.container.DestroyItem(SeaToSeaMod.kelp.seed.TechType);
					//p.RemoveItem(g.seed);
				}
				gameObject.destroy();*/
				gameObject.GetComponentInParent<LiveMixin>().TakeDamage(99999F);
				SNUtil.log("Killing incomplete/killed vent kelp @ " + transform.position);
			}
		}

		public bool isFarmed() {
			return (bool)grown;
		}

		public WaterPark getACU() {
			return acu;
		}

	}
}

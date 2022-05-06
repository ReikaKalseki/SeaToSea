using System;
using System.Collections.Generic;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class VoidSpike : WorldGenerator {
		
		private static readonly string FLOATER = "37ea521a-6be4-437c-8ed7-6b453d9218a8";
		private static readonly string FLOATER_LIGHT = "923a14c0-a7a2-49bd-a6fd-915d661582ee";
		private static readonly float FLOATER_BASE_SCALE = 0.12F;
		
		private static readonly Spike[] spikes = new Spike[]{
			new Spike("282cdcbc-8670-4f9a-ae1d-9d8a09f9e880", 3.0),
			new Spike("f0438971-2761-412c-bc42-df80577de473", 3.3),
		};
		
		private static readonly VanillaFlora[] podPrefabs = new VanillaFlora[]{
			VanillaFlora.ANCHOR_POD_SMALL1,
			VanillaFlora.ANCHOR_POD_SMALL2,
			VanillaFlora.ANCHOR_POD_MED1,
			VanillaFlora.ANCHOR_POD_MED2,
			VanillaFlora.ANCHOR_POD_LARGE,
		};
		
		private static readonly WeightedRandom<VanillaFlora> plantPrefabs = new WeightedRandom<VanillaFlora>();
		private static readonly WeightedRandom<OreType> oreChoices = new WeightedRandom<OreType>();
		
		static VoidSpike() {
			plantPrefabs.addEntry(VanillaFlora.GABE_FEATHER, 100);
			plantPrefabs.addEntry(VanillaFlora.GHOSTWEED, 85);
			plantPrefabs.addEntry(VanillaFlora.MEMBRAIN, 25);
			plantPrefabs.addEntry(VanillaFlora.REGRESS, 10);
			plantPrefabs.addEntry(VanillaFlora.BRINE_LILY, 50);
			
			oreChoices.addEntry(new OreType(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType.ToString(), 825, 0.2), 100);
			oreChoices.addEntry(new OreType(VanillaResources.QUARTZ.prefab, 0, 0.05), 100);
			oreChoices.addEntry(new OreType(VanillaResources.DIAMOND.prefab, 0, -0.05), 25);
			oreChoices.addEntry(new OreType(VanillaResources.LARGE_DIAMOND.prefab, 650), 5);
			oreChoices.addEntry(new OreType(VanillaResources.LARGE_QUARTZ.prefab), 10);
			oreChoices.addEntry(new OreType(VanillaResources.URANIUM.prefab, 0, -0.04), 5);
		}
		
		public static bool isSpike(string pfb) {
			foreach (Spike s in spikes)
				if (s.prefab == pfb)
					return true;
			return false;
		}
		
		private float scale = 1;
		private Vector3 scaleVec;
		
		public bool hasFloater = false;
		public bool hasPod = false;
		public bool hasFlora = false;
		
		public double oreRichness = 1;
		public double plantRate = 1;
		
		public int podSizeDecr = 0;
		public Vector3 podOffset = Vector3.zero;
		public bool isAux = false;
		
		public Func<Vector3, string, bool> validPlantPosCheck = null;
		
		private Spike type;
		
		internal GameObject spike;
		internal GameObject pod;
		internal GameObject floater;
		internal GameObject floaterLight;
		
		private List<GameObject> plants = new List<GameObject>();
		private List<GameObject> resources = new List<GameObject>();
		
		public VoidSpike(Vector3 pos) : base(pos) {
			setScale(UnityEngine.Random.Range(0.75F, 2.5F));
			if (UnityEngine.Random.Range(0, 4) == 0) {
				hasFloater = true;
			}
			else {
				hasPod = true;
				hasFlora = true;
			}
		}
		
		public void setScale(float s) {
			scale = s;
			scaleVec = new Vector3(scale, scale, scale);
		}
		
		public float getScale() {
			return scale;
		}
		
		public override void loadFromXML(XmlElement e) {
			setScale((float)e.getFloat("scale", scale));
			oreRichness = e.getFloat("oreRichness", oreRichness);
			
			if (e.hasProperty("hasFloater"))
				hasFloater = e.getBoolean("hasFloater");
			if (e.hasProperty("hasFlora"))
				hasFloater = e.getBoolean("hasFlora");
			if (e.hasProperty("hasPod"))
				hasPod = e.getBoolean("hasPod");
		}
		
		public override void saveToXML(XmlElement e) {
			e.addProperty("scale", scale);
			e.addProperty("oreRichness", oreRichness);
			e.addProperty("hasFloater", hasFloater);
			e.addProperty("hasPod", hasPod);
			e.addProperty("hasFlora", hasFlora);
		}
		
		public bool intersects(Vector3 vec, double r = 0) {
			if (spike == null)
				return false;
			//vec = vec+Vector3.up*0.4F; //since we *want* the actual pos to slightly intersect
			return SBUtil.objectCollidesPosition(spike, vec) || isPointWithinBoundingCone(vec, r);
		}
		
		private bool isPointWithinBoundingCone(Vector3 vec, double dr) {
			double up = 0.0*scale;
			double down = 24*scale;
			if (vec.y > position.y+up)
				return false;
			if (vec.y < position.y-down)
				return false;
			double d = (position.y+up-vec.y)/(up+down);
			double r = type.radius*scale*1.3*(1-d);
			if (r <= 0)
				return false;
			r += dr;
			double dist = (vec.x-position.x)*(vec.x-position.x)+(vec.z-position.z)*(vec.z-position.z);
			//SBUtil.log("vec "+vec+" & pos "+position+", "+dist+" of "+r*r+" @ "+d+" > "+(dist <= r*r));
			return dist <= r*r;
		}
		
		public override void generate(List<GameObject> generated) {
			generateSpike();
			generateFlora();
			generateResources();
			collateGenerated(generated);
		}
		
		internal void collateGenerated(List<GameObject> generated) {
			generated.Add(spike);
			if (floater != null) {
				generated.Add(floater);
				generated.Add(floaterLight);
			}			
			if (pod != null) {
				generated.Add(pod);
			}
			generated.AddRange(plants);
			generated.AddRange(resources);
		}
			
		internal void generateSpike() {
			type = spikes[UnityEngine.Random.Range(0, spikes.Length)];
			spike = spawner(type.prefab);
			spike.transform.position = position;
			spike.transform.localScale = scaleVec;
			spike.transform.rotation = Quaternion.Euler(180, UnityEngine.Random.Range(0F, 360F), 0);
			if (hasFloater) {
				floater = spawner(FLOATER);
				floater.transform.position = position+Vector3.up*0.55F*scale;
				floater.transform.localScale = scaleVec*FLOATER_BASE_SCALE;
				floater.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0F, 360F), 0);
				floaterLight = spawner(FLOATER_LIGHT);
				floaterLight.transform.position = floater.transform.position+Vector3.up*1*scale;
			}			
			if (hasPod) {
				pod = spawnAnchorPod(scale >= 1.75, scale >= 1 && scale <= 1.75, scale <= 1);
			}
			//DestroyDetector dd = spike.EnsureComponent<DestroyDetector>();
			//dd.enabled = true;
			//dd.pos = spike.transform.position;
		}
		
		class DestroyDetector : MonoBehaviour {
			
			internal Vector3 pos;
			
			void OnDestroy() {
				SBUtil.log("Destroying spike @ "+pos);
				SBUtil.log("Trace "+System.Environment.StackTrace);
				//throw new Exception("Spike Destroy "+pos);
			}
			
		}
			
		internal void generateFlora() {
			if (hasFlora) {
				VoidChunkPlants vc = new VoidChunkPlants(position+Vector3.up*0.5F*scale);
				vc.count = (int)(vc.count*1.3*plantRate);
				vc.fuzz *= 2*scale;
				if (hasPod)
					vc.fuzz *= 0.8F;
				vc.fuzz.y *= 0.25F;
				vc.validPlantPosCheck = validPlantPosCheck;
				vc.allowKelp = !hasPod && !hasFloater && !isAux;
				vc.spawner = spawner;
				//vc.plantCallBackrotation = membrain;
				vc.generate(plants);
			}
		}
			
		internal void generateResources() {
			if (oreRichness > 0) {
				int n = (int)(8*oreRichness);
				//SBUtil.log("Attempting "+n+" ores.");
				for (int i = 0; i < n; i++) {
					float radius = UnityEngine.Random.Range(0, (float)(type.radius-0.15))*scale;
					float angle = UnityEngine.Random.Range(0, 2F*(float)Math.PI);
					float cos = (float)Math.Cos(angle);
					float sin = (float)Math.Sin(angle);
					Vector3 pos = new Vector3(position.x+radius*cos, position.y+0.55F*scale, position.z+radius*sin);
					//pos.y += (3.5F-radius);
					//SBUtil.log("Attempted ore @ "+pos);
					if ((validPlantPosCheck == null || validPlantPosCheck(pos+Vector3.up*0.15F, "ore")) && (floater == null || !SBUtil.objectCollidesPosition(floater, pos))) {
						OreType ore = oreChoices.getRandomEntry();
						while (pos.y > ore.maxY) {
							ore = oreChoices.getRandomEntry();
						}
						GameObject go = spawner(ore.prefab);
						bool large = ore.prefab == VanillaResources.LARGE_QUARTZ.prefab || ore.prefab == VanillaResources.LARGE_DIAMOND.prefab;
						pos += Vector3.up*(float)(ore.objOffset);
						go.transform.position = pos;//UnityEngine.Random.rotationUniform;
						go.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 30), UnityEngine.Random.Range(0, 360), 0);
						if (!large) {
							Pickupable p = go.EnsureComponent<Pickupable>();
							p.isPickupable = true;
							//p.SetTechTypeOverride();
						}
						resources.Add(go);
						//SBUtil.log("Success "+go+" @ "+pos);
					}
				}
			}
		}
		
		private GameObject spawnAnchorPod(bool allowLargeSize, bool allowMediumSize, bool allowSmallSize) {
			if (podSizeDecr > 0) {
				if (!allowLargeSize || podSizeDecr > 1) {
					allowMediumSize = false;
				}
				allowLargeSize = false;
			}
			int min = allowSmallSize ? 0 : (allowMediumSize ? 2 : podPrefabs.Length-1);
			int max = allowLargeSize ? podPrefabs.Length : (allowMediumSize ? podPrefabs.Length-1 : 2);
			VanillaFlora p = podPrefabs[UnityEngine.Random.Range(min, max)];
			GameObject go = spawner(p.getRandomPrefab(true));
			go.transform.position = MathUtil.getRandomVectorAround(position+Vector3.up*-0.4F*scale, 0.2F);
			go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up);
			setPlantHeight(position.y, p, go);
			return go;
		}
		
		private void setPlantHeight(double yRef, VanillaFlora p, GameObject go) {
			double maxSink = Math.Min(p.maximumSink*0.8, scale*8);
			float sink = UnityEngine.Random.Range(0F, (float)maxSink);
			if (p == VanillaFlora.BRINE_LILY)
				sink = (float)(p.maximumSink*0.95);
			double newY = yRef+p.baseOffset-sink;
			Vector3 pos = go.transform.position;
			go.transform.position = pos;
		}
		
		public override string ToString() {
			return base.ToString()+" , p="+hasPod+" f="+hasFloater+" flora="+hasFlora+" R="+oreRichness+" pr="+plantRate;
		}
		
		private class Spike {
			
			internal readonly string prefab;
			internal readonly double radius;
			
			internal Spike(string s, double r) {
				prefab = s;
				radius = r;
			}
			
		}
		
		private class OreType {
			
			internal readonly string prefab;
			internal readonly double maxY;
			internal readonly double objOffset;
			
			internal OreType(string s, double depth = 0, double off = 0) {
				prefab = s;
				maxY = -depth;
				objOffset = off;
			}
			
		}
	}
}

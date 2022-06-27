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
		
		private static readonly WeightedRandom<OreType> oreChoices = new WeightedRandom<OreType>();
		
		private static readonly Dictionary<string, LargeWorldLevelPrefab> prefabCache = new Dictionary<string, LargeWorldLevelPrefab>();
		
		private static readonly string FLOATER = createPrefabCopy("37ea521a-6be4-437c-8ed7-6b453d9218a8", LargeWorldEntity.CellLevel.Batch).ClassID;
		private static readonly string FLOATER_LIGHT = createPrefabCopy("923a14c0-a7a2-49bd-a6fd-915d661582ee", LargeWorldEntity.CellLevel.Batch).ClassID;
		private static readonly float FLOATER_BASE_SCALE = 0.12F;
		
		private static readonly OreSpawner ORE_SPAWNER = new OreSpawner();
		
		private static readonly Spike[][] spikes = new Spike[][]{
			createSpikes(true),
			createSpikes(false),
		};
		
		private static Spike[] createSpikes(bool center) {
			return new Spike[]{
				new Spike("282cdcbc-8670-4f9a-ae1d-9d8a09f9e880", 3.0, 16.2, center),
				new Spike("f0438971-2761-412c-bc42-df80577de473", 3.3, 24.4, center),
			};
		}
		
		private static LargeWorldLevelPrefab createPrefabCopy(string template, LargeWorldEntity.CellLevel lvl) {
			LargeWorldLevelPrefab get = prefabCache.ContainsKey(template) ? prefabCache[template] : null;
			if (get == null) {
				get = new LargeWorldLevelPrefab(template, lvl).registerPrefab();
				prefabCache[template] = get;
				SNUtil.log("Creating void version of "+template);
			}
			return get;
		}
		
		private static readonly VanillaFlora[] podPrefabs = new VanillaFlora[]{
			VanillaFlora.ANCHOR_POD_SMALL1,
			VanillaFlora.ANCHOR_POD_SMALL2,
			VanillaFlora.ANCHOR_POD_MED1,
			VanillaFlora.ANCHOR_POD_MED2,
			VanillaFlora.ANCHOR_POD_LARGE,
		};
		
		static VoidSpike() {	
		
		}
		
		public static void register() {			
			oreChoices.addEntry(new OreType(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).ClassID, 825, 0.2), 150);
			oreChoices.addEntry(new OreType(VanillaResources.QUARTZ.prefab, 0, 0.05), 100);
			oreChoices.addEntry(new OreType(VanillaResources.DIAMOND.prefab, 0, -0.05), 25);
			oreChoices.addEntry(new OreType(VanillaResources.LARGE_DIAMOND.prefab, 650), 5);
			oreChoices.addEntry(new OreType(VanillaResources.LARGE_QUARTZ.prefab), 10);
			oreChoices.addEntry(new OreType(VanillaResources.URANIUM.prefab, 0, -0.04), 5);
			
			ORE_SPAWNER.Patch();
		}
		
		public static bool isSpike(string pfb) {
			foreach (Spike[] s0 in spikes) {
				foreach (Spike s in s0)
					if (s.prefab.ClassID == pfb)
						return true;
			}
			return false;
		}
		
		public static LargeWorldLevelPrefab getRandomFloraPrefab(VanillaFlora vf) {
			return getPrefab(vf.getRandomPrefab(true));
		}
		
		public static LargeWorldLevelPrefab getPrefab(string s) {
			if (!prefabCache.ContainsKey(s)) {
				throw new Exception("Voidspike Prefabs did not contain '"+s+"': contains "+prefabCache.toDebugString());
			}
			return prefabCache[s];
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
		public bool needsCenterSpace = false;
		
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
			return ObjectUtil.objectCollidesPosition(spike, vec) || isPointWithinBoundingCone(vec, r);
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
			//SNUtil.log("vec "+vec+" & pos "+position+", "+dist+" of "+r*r+" @ "+d+" > "+(dist <= r*r));
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
			type = spikes[UnityEngine.Random.Range(0, spikes.Length)][isAux ? 1 : 0];
			spike = spawner(type.prefab.ClassID);
			spike.GetComponent<SpikeHook>().scale = scale;
			spike.transform.position = position;
			spike.transform.localScale = scaleVec;
			spike.transform.rotation = Quaternion.Euler(180, UnityEngine.Random.Range(0F, 360F), 0);
			//SNUtil.offsetColliders(spike, Vector3.down*3.5F);
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
		}
			
		internal void generateFlora() {
			if (hasFlora) {
				VoidChunkPlants vc = new VoidChunkPlants(position+Vector3.up*0.5F*scale);
				vc.count = (int)(vc.count*1.3*plantRate);
				vc.fuzz *= 2*scale;
				if (hasPod)
					vc.fuzz *= 0.8F;
				vc.fuzz.y *= 0.25F;
				vc.validPlantPosCheck = isValidPlantPos;
				vc.allowKelp = !hasPod && !hasFloater && !isAux && !needsCenterSpace;
				vc.spawner = spawner;
				//vc.plantCallBackrotation = membrain;
				vc.generate(plants);
			}
		}
		
		internal bool isValidPlantPos(Vector3 vec, string why) {
			if (needsCenterSpace && (Vector3.Distance(vec._X0Z(), position._X0Z()) <= 1.5 || why.ToLowerInvariant().Contains("membrain")))
				return false;
			if (validPlantPosCheck != null && !validPlantPosCheck(vec, why))
				return false;
			return true;
		}
			
		internal void generateResources() {
			if (oreRichness > 0) {
				int n = (int)(9*oreRichness);//was 8 when not regenning
				//SNUtil.log("Attempting "+n+" ores.");
				for (int i = 0; i < n; i++) {
					float radius = UnityEngine.Random.Range(0, (float)(type.radius-0.15))*scale;
					float angle = UnityEngine.Random.Range(0, 2F*(float)Math.PI);
					float cos = (float)Math.Cos(angle);
					float sin = (float)Math.Sin(angle);
					Vector3 pos = new Vector3(position.x+radius*cos, position.y+0.55F*scale, position.z+radius*sin);
					//pos.y += (3.5F-radius);
					//SNUtil.log("Attempted ore @ "+pos);
					if ((validPlantPosCheck == null || validPlantPosCheck(pos+Vector3.up*0.15F, "ore")) && (floater == null || !ObjectUtil.objectCollidesPosition(floater, pos))) {
						OreType ore = oreChoices.getRandomEntry();
						while (pos.y > ore.maxY) {
							ore = oreChoices.getRandomEntry();
						}
						GameObject go = spawner(ore.isLarge ? ore.ore : ORE_SPAWNER.ClassID);
						go.transform.position = pos;
						go.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 30), UnityEngine.Random.Range(0, 360), 0);//UnityEngine.Random.rotationUniform;
						if (!ore.isLarge) {
							Pickupable p = go.EnsureComponent<Pickupable>();
							p.isPickupable = true;
							//p.SetTechTypeOverride();
						}
						resources.Add(go);
						//SNUtil.log("Success "+go+" @ "+pos);
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
			else if (podSizeDecr < 0) {
				allowLargeSize = true;
				allowMediumSize = podSizeDecr >= -1;
				allowSmallSize = false;
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
			
			internal readonly SpikePrefab prefab;
			internal readonly double radius;
			internal readonly double height;
			
			internal Spike(string s, double r, double h, bool c) {
				prefab = (SpikePrefab)(new SpikePrefab(s, c).registerPrefab());
				radius = r;
				height = h;
			}
			
		}
		
		public class LargeWorldLevelPrefab : GenUtil.CustomPrefabImpl {
			
			internal readonly LargeWorldEntity.CellLevel level;
	       
			internal LargeWorldLevelPrefab(string template, LargeWorldEntity.CellLevel lvl) : base("void_"+template, template) {
				level = lvl;
		    }
	
			public override void prepareGameObject(GameObject go, Renderer r) {
				LargeWorldEntity lw = go.EnsureComponent<LargeWorldEntity>();
				lw.cellLevel = level;
			}
			
			internal LargeWorldLevelPrefab registerPrefab() {
				Patch();
				return this;
			}
		}
		
		public sealed class OreSpawner : Spawnable {
	        
	        public OreSpawner() : base("voidspike_ore_spawner", "", "") {
				
	        }
			
	        public override GameObject GetGameObject() {
				GameObject go = new GameObject();
				go.name = "Void Spike Ore Spawner";
				go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				go.EnsureComponent<TechTag>().type = TechType;
				go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
				go.EnsureComponent<OreSpawnerController>().speed = UnityEngine.Random.Range(0.5F, 2F);
				SNUtil.writeToChat("Fetching spawner prefab");
				return go;
	        }
			
		}
		
		class OreSpawnerController : MonoBehaviour {
			
			internal float speed = 0;
			
			private Transform currentOre;
			private float despawnTime = 0;
			private float nextTime = 0;
			private float lastSpawnTime = 0;
			
			private bool hasSpawned = false;
			
			private Renderer[] currentRenderers = null;
			
			private static readonly float LIFESPAN_MIN = 45;
			private static readonly float LIFESPAN_MAX = 90;
			private static readonly float GAP_MIN = 30;
			private static readonly float GAP_MAX = 60;
			
			void Start() {
				//SNUtil.writeToChat("Started ore spawner @ "+transform.position);
				if (speed <= 0.1) {
					speed = UnityEngine.Random.Range(0.5F, 2F);
				}
				if (currentOre == null || !currentOre.gameObject.activeInHierarchy) {
					Transform t = findOldOre();
					SNUtil.writeToChat("Spawner @ "+transform.position+" found: "+(t == null ? "none" : ""+t.gameObject.GetComponentInParent<Pickupable>().GetTechType()));
					setCurrentOre(t);//transform.Find("spawned_ore");
				}
				if (lastSpawnTime <= 0) {
					float time = DayNightCycle.main.timePassedAsFloat;
					lastSpawnTime = time;
					if (currentOre == null) {
						nextTime = time+UnityEngine.Random.Range(GAP_MIN, GAP_MAX)/speed;
						if (!hasSpawned)
							nextTime *= 0.25F;
					}
					else {
						despawnTime = time+UnityEngine.Random.Range(LIFESPAN_MIN, LIFESPAN_MAX)/speed;
					}
				}
			}
			
			private Transform findOldOre() {
				Collider[] near = Physics.OverlapSphere(gameObject.transform.position, 0.05F);
				foreach (Collider c in near) {
					SNUtil.writeToChat("Check "+c+" in "+c.gameObject);
					if (c.gameObject == gameObject)
						continue;
					if (c.gameObject.GetComponentInParent<Pickupable>() != null)
						return c.gameObject.transform;
				}
				return null;
			}
			
			void Update() {
				float time = DayNightCycle.main.timePassedAsFloat;
				//SNUtil.writeToChat(time+": ["+currentOre+"] > "+nextTime+"/"+lastSpawnTime+" > "+despawnTime);
				if (currentOre != null && currentOre.gameObject.activeInHierarchy) {
					float f = (float)MathUtil.linterpolate(time, lastSpawnTime, despawnTime, 0, 1);
					if (f >= 1) {
						destroyCurrent();
					}
					else {
						float f2 = f < 0.1 ? 1-f*10 : f;
						foreach (Renderer r in currentRenderers) {
							foreach (Material m in r.materials) {
								m.SetFloat(ShaderPropertyID._Built, 0.75F-f2*0.3125F);
							}
						}
					}
				}
				else if (time >= nextTime) {
					spawn(time);
				}
			}
			
			void OnDestroy() {
				destroyCurrent();
			}
			
			void OnDisable() {
				destroyCurrent();
			}
			
			private void destroyCurrent() {
				if (currentOre == null)
					return;
				//SNUtil.writeToChat("Deleted @ "+gameObject.transform.position+": "+currentOre.gameObject.GetComponentInParent<Pickupable>().GetTechType());
				UnityEngine.Object.Destroy(currentOre.gameObject);
				currentOre = null;
				currentRenderers = null;
			}
			
			private GameObject spawn(float time) {
				OreType ore = oreChoices.getRandomEntry();
				while (ore.isLarge || transform.position.y > ore.maxY) {
					ore = oreChoices.getRandomEntry();
				}
										
				GameObject go = ObjectUtil.createWorldObject(ore.ore);
				go.transform.position = gameObject.transform.position+Vector3.up*(float)(ore.objOffset);
				go.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360F), 0);
				//go.name = "spawned_ore";
				//go.transform.parent = gameObject.transform;
				
				setCurrentOre(go.transform);
				currentRenderers = currentOre.gameObject.GetComponentsInChildren<Renderer>();
				
				foreach (Renderer r in currentRenderers) {
					foreach (Material m in r.materials) {
						m.EnableKeyword("FX_BUILDING");
						//material2.SetTexture(ShaderPropertyID._EmissiveTex, this._EmissiveTex);
						m.SetFloat(ShaderPropertyID._Cutoff, 0.5F);
						m.SetColor(ShaderPropertyID._BorderColor, new Color(0f, 0f, 0f, 1f));
						m.SetVector(ShaderPropertyID._BuildParams, new Vector4(2f, 0.7f, 3f, -0.5f)); //last arg is speed, +ve is down
						m.SetFloat(ShaderPropertyID._NoiseStr, 0.25f);
						m.SetFloat(ShaderPropertyID._NoiseThickness, 0.49f);
						//m.SetFloat(ShaderPropertyID._BuildLinear, 1f);
						m.SetFloat(ShaderPropertyID._MyCullVariable, 0f);
					}
				}
				
				//SNUtil.writeToChat("Spawned @ "+gameObject.transform.position+": "+go.GetComponentInChildren<Pickupable>().GetTechType());
				
				despawnTime = time+UnityEngine.Random.Range(LIFESPAN_MIN, LIFESPAN_MAX)/speed;
				nextTime = despawnTime+UnityEngine.Random.Range(GAP_MIN, GAP_MAX)/speed;
				lastSpawnTime = time;
				
				hasSpawned = true;
				return go;
			}
			
			private void setCurrentOre(Transform t) {
				currentOre = t;
				if (t != null) {
					t.gameObject.GetComponentInChildren<Pickupable>().pickedUpEvent.AddHandler(this, pp => setCurrentOre(null));
				}
			}
			
		}
		
		public sealed class SpikePrefab : LargeWorldLevelPrefab {
			
			internal readonly bool isCenter;
	       
			internal SpikePrefab(string template, bool c) : base(template, LargeWorldEntity.CellLevel.Batch) {
				isCenter = c;
		    }
	
			public override void prepareGameObject(GameObject go, Renderer r) {
				base.prepareGameObject(go, r);
				go.EnsureComponent<SpikeHook>();
				if (isCenter) {
					go.EnsureComponent<SpikeClusterHook>();
				}
			}
		}
		
		private class SpikeClusterHook : MonoBehaviour {
			
			void Update() { //TODO spawn fish
				
			}
			
		}
		
		private class SpikeHook : MonoBehaviour {
			
			internal float scale;
		  
			private void Start() {
				if (scale <= 0.01)
					scale = gameObject.transform.localScale.x;
				//SNUtil.log("Fixing spike colliders @ "+gameObject.transform.position+": "+scale);
				fixColliders();
			}
		
			private void fixColliders() {
				//SNUtil.log("Spike "+this+" has colliders: ");
				//bool trigger = false;
				foreach (Collider c in gameObject.GetAllComponentsInChildren<Collider>()) {
					//trigger |= c.isTrigger;
					//SNUtil.log(c.name+" @ "+c.bounds+" = "+c.GetType());
					if (c is SphereCollider) {
						//SNUtil.log("R="+((SphereCollider)c).radius+", C="+((SphereCollider)c).center);
						((SphereCollider)c).radius = ((SphereCollider)c).radius*0.95F;
						((SphereCollider)c).center = ((SphereCollider)c).center+Vector3.up*0.25F*scale;
					}
					//UnityEngine.Object.Destroy(c);
				}/*
				MeshCollider mc = spike.AddComponent<MeshCollider>();
				mc.enabled = true;
				mc.convex = true;
				mc.isTrigger = false;//trigger;
				mc.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;
				mc.sharedMesh*/
				
				//SNUtil.visualizeColliders(spike);
				
				//Bounds render = spike.GetComponentInChildren<Renderer>().bounds;
				//BoxCollider box = spike.AddComponent<BoxCollider>();
				//box.center = Vector3.zero+Vector3.up*(float)(scale*type.height/2D)*0.965F;
				//box.size = new Vector3((float)type.radius*1.2F, (float)type.height, (float)type.radius*1.2F)*scale;
				
				//box.center = -(render.center-spike.transform.position);
				//box.size = render.extents;
			}
		}
		
		private class OreType {
			
			internal readonly string ore;
			internal readonly double maxY;
			internal readonly double objOffset;
			
			internal readonly bool isLarge;
			
			internal OreType(string s, double depth = 0, double off = 0) {
				ore = s;
				maxY = -depth;
				objOffset = off;
				
				isLarge = s == VanillaResources.LARGE_QUARTZ.prefab || s == VanillaResources.LARGE_DIAMOND.prefab;
			}
			
		}
	}
}

using System;

using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class AvoliteSpawner {
		
		public static readonly AvoliteSpawner instance = new AvoliteSpawner();
		
		private readonly WeightedRandom<TechType> itemChoicesLoose = new WeightedRandom<TechType>();
		private readonly WeightedRandom<TechType> itemChoicesBox = new WeightedRandom<TechType>();
		
		private readonly TechType avolite = CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType;
		
		private readonly Vector3 eventCenter = new Vector3(215, 425.6F, 2623.6F);
		private readonly Vector3 eventUITargetLocation = new Vector3(297.2F, 3.5F, 1101);
		private readonly Vector3 mountainCenter = new Vector3(356.3F, 29F, 1039.4F);
		private readonly Vector3 biomeCenter = new Vector3(966, 0, 1336);
		
		private readonly Dictionary<Vector3, TechType> boxes = new Dictionary<Vector3, TechType>();
		private readonly Dictionary<Vector3, string> looseItems = new Dictionary<Vector3, string>();
		private readonly Dictionary<TechType, SunbeamSpawnPlaceholder> prefabs = new Dictionary<TechType, SunbeamSpawnPlaceholder>();
		
		//private int boxCount = UnityEngine.Random.Range(8, 14); //8-13
		private int looseCount = UnityEngine.Random.Range(18, 31); //18-30
		private int scrapCount = UnityEngine.Random.Range(45, 71); //45-70
		
		private SignalManager.ModSignal signal;
		
		private int gennedAvolite;
		
		private AvoliteSpawner() {
			addItem(TechType.NutrientBlock, 250, true, false);
			addItem(TechType.DisinfectedWater, 200, true, false);
			addItem(TechType.Battery, 40, true, false);
			//addItem(TechType.Beacon, 40, true, false);
			
			addItem(TechType.PowerCell, 15, true, true);
			addItem(TechType.FireExtinguisher, 25, true, true);
			//addItem(avolite, 20, true, true);
			
			addItem(TechType.EnameledGlass, 100, false, true);
			addItem(TechType.Titanium, 150, false, true);
			addItem(TechType.ComputerChip, 50, false, true);
			addItem(TechType.WiringKit, 75, false, true);
			addItem(TechType.CopperWire, 150, false, true);
			
			signal = SignalManager.createSignal(SeaToSeaMod.signals.getEntry("sunbeamdrops"));
			signal.pdaEntry.addSubcategory("Sunbeam");		
		}
		
		public void register() {
			signal.register();	
			
			foreach (SunbeamSpawnPlaceholder sb in prefabs.Values) {
				sb.Patch();
				GenUtil.registerOreWorldgen(sb, false, BiomeType.Mountains_Rock, 1, 1F);
				GenUtil.registerOreWorldgen(sb, false, BiomeType.Mountains_Sand, 1, 1F);
			}
		}
		
		private void activateSignal(GameObject tie) {
			signal.build(tie, tie.transform.position);
			signal.activate();
		}
		
		private void addItem(TechType item, double wt, bool box, bool loose) {
			if (box && false)
				itemChoicesBox.addEntry(item, wt);
			if (loose || true)
				itemChoicesLoose.addEntry(item, wt);
			
			if (!prefabs.ContainsKey(item)) {
				SunbeamSpawnPlaceholder sb = new SunbeamSpawnPlaceholder(item);
				prefabs[item] = sb;
			}
		}
		
		public string getPlaceholderSpawnID(TechType tech) {
			return prefabs[tech].ClassID;
		}
		
		[Obsolete]
		public void doSpawn() {
			GameObject tie = null;
			
			for (int i = 0; i < 0/*boxCount*/; i++) {
				GameObject box = spawnBox();
				if (box != null) {
					SBUtil.setCrateItem(box.EnsureComponent<SupplyCrate>(), generateRandomItem(true));
					applyPhysics(box);
				}
			}
			for (int i = 0; i < looseCount; i++) {
				Vector3 pos = getRandomPosition();
				GameObject go = SBUtil.dropItem(pos, generateRandomItem(false));
				applyPhysics(go);
			}
			while (gennedAvolite < 6) {
				Vector3 pos = getRandomPosition();
				GameObject go = SBUtil.dropItem(pos, avolite);
				applyPhysics(go);
				gennedAvolite++;
			}
			for (int i = 0; i < scrapCount+1; i++) {
				Vector3 pos = getRandomPosition();
				VanillaResources mtl = null;
				switch(UnityEngine.Random.Range(0, 4)) {
					case 0:
						mtl = VanillaResources.SCRAP1;
						break;
					case 1:
						mtl = VanillaResources.SCRAP2;
						break;
					case 2:
						mtl = VanillaResources.SCRAP3;
						break;
					case 3:
						mtl = VanillaResources.SCRAP4;
						break;
				}
				GameObject go = SBUtil.createWorldObject(mtl.prefab);
				applyPhysics(go);
				go.transform.position = pos;
				go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up);
				tie = go;
			}
			
			activateSignal(tie);
			tie.transform.position = getRandomPosition(0);
			applyPhysics(tie, 0);
			
			tie.EnsureComponent<DebrisSignal>().Invoke("destroy", 30);
			
			PDAManager.getPage("sunbeamdebrishint").unlock();
		}
		
		private void applyPhysics(GameObject go, int fuzz = 200) {
			//go.transform.position = MathUtil.getRandomVectorAround(eventUITargetLocation+Vector3.up*200, 9);//go.transform.position+dist*0.1F;
			Vector3 target = MathUtil.getRandomVectorAround(biomeCenter, fuzz);//MathUtil.getRandomVectorAround(mountainCenter+new Vector3(150, 0, 150), new Vector3(150, 30, 150));
			Vector3 dist = (target-go.transform.position)._X0Z();
			SBUtil.applyGravity(go);
			go.transform.rotation = UnityEngine.Random.rotationUniform;
			go.GetComponent<Rigidbody>().velocity = dist*0.04F;
			go.GetComponent<WorldForces>().aboveWaterGravity *= 0.5F;
			go.GetComponent<WorldForces>().underwaterDrag = 1.5F;
		}
		
		private GameObject spawnBox() {
			Vector3 pos = getRandomPosition();
			GameObject box = SBUtil.createWorldObject("8c21d402-1767-4266-ada6-b3e40c798e9f"); //powercell
			if (box != null)
				box.transform.position = pos;
			return box;
		}
		
		private TechType generateRandomItem(bool box) {
			TechType ret = (box ? itemChoicesBox : itemChoicesLoose).getRandomEntry();
			if (ret == avolite)
				gennedAvolite++;
			return ret;
		}
		
		private Vector3 getRandomPosition(int fuzz = 20) {
			Vector3 vec = eventCenter*0.5F+eventUITargetLocation*0.5F;
			vec.y = eventCenter.y;
			return MathUtil.getRandomVectorAround(vec, fuzz);
		}
		
		public class TriggerCallback : MonoBehaviour {
			
			void trigger() {
				instance.doSpawn();
			}
			
		}
		
		class DebrisSignal : MonoBehaviour {
			
			void Start() {
				
			}
			
			void destroy() {
				instance.signal.deactivate();
				UnityEngine.Object.Destroy(gameObject);
			}
			
		}
		
		class AvoSpawnTrigger : MonoBehaviour {
			
			internal TechType itemToSpawn;
			
			void Start() {
				
			}
			
			void Update() {
				if (Story.StoryGoalManager.main.IsGoalComplete("SunbeamCheckPlayerRange")) {
					Player p = Player.main;
					if (p != null && p.isActiveAndEnabled) {
						if (Vector3.Distance(p.transform.position, gameObject.transform.position) <= 32) {
							convert();
						}
					}
				}
			}
			
			private void convert() {
				GameObject drop = SBUtil.dropItem(gameObject.transform.position+Vector3.up*0.5F, itemToSpawn);
				SBUtil.applyGravity(drop);
				drop.transform.rotation = UnityEngine.Random.rotationUniform;
				UnityEngine.Object.Destroy(gameObject);
				SBUtil.writeToChat("Converted to "+itemToSpawn+" @ "+drop.transform.position);
			}
			
		}
		
		public sealed class SunbeamSpawnPlaceholder : Spawnable {
			
			private readonly TechType itemToSpawn;
			
			internal SunbeamSpawnPlaceholder(TechType spawn) : base("sb_spawn_hook_"+spawn, "sb_spawn_hook_"+spawn, "") {
				itemToSpawn = spawn;
			}
			
			public override GameObject GetGameObject() {
				GameObject prefab;
				if (UWE.PrefabDatabase.TryGetPrefab(VanillaResources.SALT.prefab, out prefab)) {
					GameObject world = UnityEngine.Object.Instantiate(prefab);
					world.SetActive(false);
					foreach (Component c in world.GetComponentsInChildren<Component>()) {
						if (c is Transform || c is Renderer || c is Rigidbody)
							continue;
						UnityEngine.Object.Destroy(c);
					}
					Renderer r = world.GetComponentInChildren<Renderer>();
					r.enabled = false;
					if (itemToSpawn == TechType.None) {
						SBUtil.log("Cannot delegate spawn of none!");
					}
					world.EnsureComponent<AvoSpawnTrigger>().itemToSpawn = itemToSpawn == TechType.None ? TechType.BloodOil : itemToSpawn;
					return world;
				}
				else {
					SBUtil.writeToChat("Could not fetch template GO for "+this);
					return null;
				}
			}
			
		}
	}
	
}

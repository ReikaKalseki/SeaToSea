using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Json;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	public static class C2CUtil {

		//exclusion radius, target count, max range
		internal static readonly Dictionary<Vector3, Tuple<float, int, float>> mercurySpawners = new Dictionary<Vector3, Tuple<float, int, float>>() {
			{ new Vector3(908.7F, -235.1F, 615.7F), Tuple.Create(2F, 4, 32F) },
			{ new Vector3(904.3F, -247F, 668.8F), Tuple.Create(1F, 3, 32F) },
			{ new Vector3(915.1F, -246.8F, 651.2F), Tuple.Create(2F, 6, 32F) },
			{ new Vector3(1273, -290, 604.3F), Tuple.Create(2F, 3, 32F) },
			{ new Vector3(1254, -293.3F, 606.3F), Tuple.Create(2F, 3, 32F) },
			{ new Vector3(1239, -286.4F, 617), Tuple.Create(2F, 3, 32F) },
			{ new Vector3(1245, -308.2F, 555.8F), Tuple.Create(2F, 3, 32F) },
			{ new Vector3(-1216, -299.1F, 510.3F), Tuple.Create(2F, 3, 32F) },
			{ new Vector3(1278, -276.4F, 497.5F), Tuple.Create(2F, 3, 32F) },
			{ new Vector3(1228, -275.6F, 483.9F), Tuple.Create(2F, 3, 32F) }
		};

		internal static readonly Dictionary<Vector3, Tuple<float, int, float>> calciteSpawners = new Dictionary<Vector3, Tuple<float, int, float>>() {
			{ new Vector3(-993.1F, -630.4F, -618.2F), Tuple.Create(4F, 3, 24F) },
			{ new Vector3(-983.3F, -623.9F, -561.1F), Tuple.Create(4F, 5, 32F) },
			{ new Vector3(-666.8F, -688.0F, -42.14F), Tuple.Create(4F, 6, 48F) },
			{ new Vector3(-674.2F, -622.0F, -221.6F), Tuple.Create(4F, 6, 48F) },
			{ new Vector3(-719.4F, -673.7F, -39.6F), Tuple.Create(4F, 5, 48F) },
			{ new Vector3(-864.5F, -672.7F, -128.6F), Tuple.Create(4F, 8, 48F) },
		};

		internal static readonly Dictionary<Vector3, Tuple<float, int, float>> extraNickelSpawners = new Dictionary<Vector3, Tuple<float, int, float>>() {
			{ new Vector3(-648, -578, 1038), Tuple.Create(8F, 2, 100F) },
			{ new Vector3(-725, -578, 1031), Tuple.Create(8F, 2, 100F) },
			{ new Vector3(-788, -582, 1057), Tuple.Create(8F, 2, 100F) },
			{ new Vector3(-1081, -641, -501), Tuple.Create(8F, 2, 90F) },
			{ new Vector3(-692, -652, -769), Tuple.Create(8F, 2, 60F) },
			{ new Vector3(-742, -653, -759), Tuple.Create(8F, 2, 60F) },
			{ new Vector3(-1107, -663, -209), Tuple.Create(8F, 2, 80F) },
			{ new Vector3(-1119, -667, -87), Tuple.Create(8F, 2, 80F) },
		};

		private static readonly HashSet<string> floaterRocks = new HashSet<string>() {
			"44396d05-0910-4b4d-a046-119fab3512a5",
			"7637d968-4878-46a5-adf5-aa9e21fe3ddc",
			"9a9cdb4e-f110-412d-b16b-b9ace904b569",
			"a7b35deb-1ac7-4fb8-8393-c0252cbf6d23",
			"d4ad48a9-67fa-4b34-8447-5cd6a69d1270",
			"e3d778b5-a81e-4b64-8dd6-910fb22772db",
			"f895696c-cdc6-4427-a87f-2b62666ea0cb"
		};

		private static readonly HashSet<string> auroraFires = new HashSet<string>() {
			"14bbf7f0-4276-48bf-868b-317b366edd16",
			"3877d31d-37a5-4c94-8eef-881a500c58bc",
			"afe53ea1-d2a8-4f76-8ffb-d41ff6046b52"
		};

		private static readonly Dictionary<string, Color> auroraPrawnFireColors = new Dictionary<string, Color> {
			{ "xFireFlame", new Color(0, 0.67F, 2) },
			{ "xFireCurl", new Color(1, 1, 1) },
			{ "xAmbiant_Sparks", new Color(0, 1, 1) },
			{ "xAmbiant_Ashes", new Color(0.1F, 0.1F, 1) },
			{ "x_Fire_CrossPlanes", new Color(0.67F, 0.43F, 1) },
			{ "x_Fire_GroundPlane", new Color(0.24F, 0.57F, 1) },
			{ "x_SmokeLight_Cylindrical", new Color(0.67F, 0.72F, 0.97F) },
			{ "x_Fire_Cylindrical", new Color(0.24F, 0.51F, 1) },
		};

		static C2CUtil() {

		}

		public static bool checkConditionsAndShowPDAAndFirstVoicelogIfNot(params Tuple<bool, string, PDAMessages.Messages>[] checks) {
			foreach (Tuple<bool, string, PDAMessages.Messages> check in checks) {
				if (!checkConditionAndShowPDAAndVoicelogIfNot(check.Item1, check.Item2, check.Item3))
					return false;
			}
			return true;
		}

		public static bool checkConditionAndShowPDAAndVoicelogIfNot(bool check, string page, PDAMessages.Messages msg) {
			if (check) {
				return true;
			}
			else {
				MoraleSystem.instance.shiftMorale(-10);
				if (PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(msg).key)) {
					if (!string.IsNullOrEmpty(page))
						PDAManager.getPage(page).unlock(false);
				}
				return false;
			}
		}

		public static bool playerCanHeal() {
			Player ep = Player.main;
			return !EnvironmentalDamageSystem.instance.isPlayerRecoveringFromPressure() && (!ep.IsSwimming() || ep.GetDepth() < EnvironmentalDamageSystem.depthDamageStart || LiquidBreathingSystem.instance.hasLiquidBreathing());
		}
		/*
		public static GameObject createMergedPropGun(bool toInv = false) {
			GameObject prop = ObjectUtil.createWorldObject(TechType.PropulsionCannon);
			GameObject repl = ObjectUtil.lookupPrefab(TechType.RepulsionCannon);
			prop.EnsureComponent<RepulsionCannon>().copyObject(repl.GetComponent<RepulsionCannon>());
			prop.EnsureComponent<PropGunTypeSwapper>().applyMode();
			if (toInv)
				Inventory.main.Pickup(prop.GetComponent<Pickupable>());
			else
				prop.SetActive(true);
			return prop;
		}
		
		public class PropGunTypeSwapper : MonoBehaviour {
			
			public bool isPropMode = true;
			
			public void applyMode() {
				if (isPropMode) {
					GetComponent<PropulsionCannon>().enabled = true;
					GetComponent<PropulsionCannonWeapon>().enabled = true;
					GetComponent<RepulsionCannon>().enabled = false;
					//RenderUtil.swapTextures(GetComponentInChildren<Renderer>().materials[0]);
				}
				else {
					GetComponent<PropulsionCannon>().enabled = false;
					GetComponent<PropulsionCannonWeapon>().enabled = false;
					GetComponent<RepulsionCannon>().enabled = true;
				}
			}
			
		}*/
		/*
	public static bool hasNoGasMask() {
   		return Inventory.main.equipment.GetCount(TechType.Rebreather) == 0 && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0;
	}*/

		public static void generateLavaCastleAzurite() {
			List<GameObject> azurite = new List<GameObject>();
			string azur = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).ClassID;
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
				if (pi.ClassId == "407e40cf-69f2-4412-8ab6-45faac5c4ea2") {
					for (int ang = 0; ang < 360; ang += 10) {
						float a = UnityEngine.Random.Range(ang - 5F, ang + 5F);
						float r = 16;
						Vector3 dt = new Vector3(Mathf.Cos(a) * r, -UnityEngine.Random.Range(0, UnityEngine.Random.Range(25, 40)), Mathf.Sin(a) * r);
						Vector3 vec = pi.transform.position + dt;
						Ray ray = new Ray(vec, -dt.setY(0));
						if (UWE.Utils.RaycastIntoSharedBuffer(ray, 24, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore) > 0) {
							RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
							if (hit.transform != null) {
								bool flag = true;
								foreach (PrefabIdentifier pi2 in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(hit.point, 9F)) {
									if (pi2.ClassId == azur) {
										flag = false;
										break;
									}
								}
								if (!flag)
									continue;
								GameObject go = ObjectUtil.createWorldObject(azur);
								go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
								go.transform.Rotate(Vector3.up * UnityEngine.Random.Range(0F, 360F), Space.Self);
								go.transform.position = hit.point;
								azurite.Add(go);
							}
						}
					}
				}
			}

			string path = BuildingHandler.instance.getDumpFile("lavacastle_vents");
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);

			foreach (GameObject go in azurite) {
				PositionedPrefab pfb = new PositionedPrefab(go.GetComponent<PrefabIdentifier>());
				XmlElement e = doc.CreateElement("customprefab");
				pfb.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}

			doc.Save(path);
		}

		public static void generateLRNestPlants() {
			Vector3 p1 = new Vector3(-786, -762.6F, -321);
			Vector3 p2 = new Vector3(-801, -764.9F, -280);
			Vector3 p3 = new Vector3(-788, -751.6F, -321);

			List<GameObject> plants = new List<GameObject>();

			for (float f = 0; f <= 1; f += 0.05F) {
				Vector3 vec = Vector3.Lerp(p1, p2, f);
				for (int i = 0; i < 9; i++) {
					Vector3 rot = UnityEngine.Random.rotationUniform.eulerAngles.normalized;
					Ray ray = new Ray(vec, rot);
					if (UWE.Utils.RaycastIntoSharedBuffer(ray, 6, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore) > 0) {
						RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
						if (hit.transform != null) {
							bool flag = true;
							foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(hit.point, 0.2F)) {
								if (pi.ClassId == SeaToSeaMod.lrNestGrass.ClassID) {
									flag = false;
									break;
								}
							}
							if (!flag)
								continue;
							GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.lrNestGrass.ClassID);
							go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
							go.transform.position = hit.point;
							plants.Add(go);
						}
					}
				}
			}

			for (int i = 0; i < 9; i++) {
				Vector3 rot = UnityEngine.Random.rotationUniform.eulerAngles.normalized;
				Ray ray = new Ray(p3, rot);
				if (UWE.Utils.RaycastIntoSharedBuffer(ray, 18, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore) > 0) {
					RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
					SNUtil.writeToChat(i + ": " + hit.transform);
					if (hit.transform != null && hit.normal.y > -0.7F) {
						bool flag = true;
						foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(hit.point, 0.2F)) {
							if (pi.ClassId == SeaToSeaMod.lrNestGrass.ClassID) {
								flag = false;
								break;
							}
						}
						if (!flag)
							continue;
						GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.lrNestGrass.ClassID);
						go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
						go.transform.position = hit.point;
						plants.Add(go);
					}
				}
			}

			string path = BuildingHandler.instance.getDumpFile("lr_nest2");
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);

			foreach (GameObject go in plants) {
				PositionedPrefab pfb = new PositionedPrefab(go.GetComponent<PrefabIdentifier>());
				XmlElement e = doc.CreateElement("customprefab");
				pfb.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}

			doc.Save(path);
		}

		public static void resizeCyclopsStorage(SubRoot sub) { //vanilla is 3x6
			int amt = sub && sub.upgradeConsole && sub.upgradeConsole.modules != null ? sub.upgradeConsole.modules.GetCount(C2CItems.cyclopsStorage.TechType) : 0;
			int slots = 18; //18 vanilla base
			if (QModManager.API.QModServices.Main.ModPresent("MoreCyclopsUpgrades"))
				slots += 6 + (amt * 6) + (amt / 2 * 12); //https://i.imgur.com/JUr54tB.png
			else
				slots += (18 * amt) + (18 * (amt / 2)); //https://i.imgur.com/K5UaRHZ.png
														//int w = Math.Min(3+amt, 6);
														//int h = 6+amt*2;
			int w = 3;
			int h = slots / w;
			while (w < 6 && h >= 9) {
				w++;
				while (slots % w != 0)
					w++;
				h = slots / w;
			}

			foreach (CyclopsLocker cl in sub.GetComponentsInChildren<CyclopsLocker>()) {
				StorageContainer sc = cl.GetComponent<StorageContainer>();
				sc.Resize(w, h);
			}
		}

		public static void setupDeathScreen() {
			uGUI.main.respawning.Hide();
			DamageFX.main.ClearHudDamage();
			Player.main.SuffocationReset();
			IngameMenu.main.gameObject.SetActive(true);
			IngameMenu.main.ChangeSubscreen("QuitConfirmation");
			UnityEngine.UI.Text txt = IngameMenu.main.currentScreen.getChildObject("Header").GetComponent<UnityEngine.UI.Text>();
			txt.text = "You died. Please reload your save.";
			txt.fontSize = 20;
			IngameMenu.main.currentScreen.getChildObject("ButtonNo").SetActive(false);
			GameObject yes = IngameMenu.main.currentScreen.getChildObject("ButtonYes");
			yes.GetComponentInChildren<UnityEngine.UI.Text>().text = "Main Menu";
			yes.transform.localPosition = new Vector3(0, yes.transform.localPosition.y, yes.transform.localPosition.z);
		}

		public static void swapRepulsionCannons() {
			InventoryItem ii = Inventory.main.quickSlots.heldItem;
			TechType tt = ii == null || !ii.item ? TechType.None : ii.item.GetTechType();
			if (ii != null && (tt == TechType.PropulsionCannon || tt == TechType.RepulsionCannon)) {
				TechType to = tt == TechType.PropulsionCannon ? TechType.RepulsionCannon : TechType.PropulsionCannon;
				int selSlot = InventoryUtil.getActiveQuickslot();
				//TechType batt = TechType.None;
				float battCh = -1;
				Pickupable batt = null;
				PlayerTool pt = ii.item.GetComponent<PlayerTool>();
				EnergyMixin e = pt.energyMixin;
				if (e) {
					/*
					IBattery ib = e.battery;
					if (ib is Battery)
						batt = ((Battery)ib).GetComponent<Pickupable>();
						*/
					//batt = (e.batterySlot.storedItem == null ? TechType.None : e.batterySlot.storedItem.item.GetTechType());
					batt = e.batterySlot.storedItem.item;
					battCh = e.charge / e.capacity;
					e.batterySlot.RemoveItem();
				}
				Inventory.main.container.forceRemoveItem(ii);
				InventoryUtil.addItem(to);
				InventoryItem put = Inventory.main.container.getItem(to);
				if (put != null) {
					PlayerTool pt2 = put.item.GetComponent<PlayerTool>();
					if (batt/* != TechType.None*/) {
						//pt2.energyMixin.gameObject.EnsureComponent<DelayedBatterySwapCallback>().init(batt, battCh, e).Invoke("apply", 1.5F);
						pt2.energyMixin.batterySlot.AddItem(batt);
						pt2.energyMixin.RestoreBattery();
					}
					if (selSlot >= 0)
						Inventory.main.quickSlots.Select(selSlot);
					SNUtil.writeToChat("Swapped to " + Language.main.Get(to) + (batt/* != TechType.None*/ ? ", with battery '" + Language.main.Get(batt.GetTechType()) + "' (" + (battCh * 100F).ToString("0.0") + "% full)" : ""));
				}
				else {
					SNUtil.writeToChat("Swapped (pro/re)pulsion gun but not found in inventory afterwards?!");
					if (batt/* != TechType.None*/)
						//InventoryUtil.addItem(batt);
						Inventory.main.Pickup(batt);
				}
			}
			else {
				SNUtil.writeToChat("Found no (pro/re)pulsion gun to swap");
			}
		}

		public static void cleanup() {
			int ptc = 0;
			foreach (PlatinumTag pt in UnityEngine.Object.FindObjectsOfType<PlatinumTag>()) {
				pt.gameObject.destroy();
				ptc++;
			}
			SNUtil.writeToChat("Removed " + ptc + " platinum.");
		}


		public static void onSkyApplierSpawn(SkyApplier pk) {
			if (C2CHooks.skipSkyApplierSpawn)
				return;
			GameObject go = pk.gameObject;
			if (go.name.StartsWith("Seamoth", StringComparison.InvariantCultureIgnoreCase) && go.name.EndsWith("Arm(Clone)", StringComparison.InvariantCultureIgnoreCase))
				return;
			//if (DIHooks.isWorldLoaded())
			//	LifeformScanningSystem.instance.onObjectCreated(go);
			if (go.name.StartsWith("ExplorableWreck", StringComparison.InvariantCultureIgnoreCase)) {
				go.EnsureComponent<ImmuneToPropulsioncannon>(); //also implements IObstacle to prevent building
			}
			PrefabIdentifier pi = go.FindAncestor<PrefabIdentifier>();
			if (SNUtil.match(pi, "d79ab37f-23b6-42b9-958c-9a1f4fc64cfd") && Vector3.Distance(C2CHooks.fcsWreckOpenableDoor, go.transform.position) <= 0.5) {
				new WreckDoorSwaps.DoorSwap(go.transform.position, "Handle").applyTo(go);
			}
			else if (SNUtil.match(pi, "055b3160-f57b-46ba-80f5-b708d0c8180e") && Vector3.Distance(C2CHooks.fcsWreckBlockedDoor, go.transform.position) <= 0.5) {
				new WreckDoorSwaps.DoorSwap(go.transform.position, "Blocked").applyTo(go);
			}
			else if (SNUtil.match(pi, VanillaCreatures.SEA_TREADER.prefab)) {
				//go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
				go.EnsureComponent<C2CTreader>();
			}
			else if (SNUtil.match(pi, VanillaCreatures.CAVECRAWLER.prefab)) {
				go.EnsureComponent<C2Crawler>();
			}
			else if (SNUtil.match(pi, VanillaCreatures.REAPER.prefab)) {
				go.EnsureComponent<C2CReaper>();
			}
			else if (SNUtil.match(pi, VanillaCreatures.GHOST_LEVIATHAN.prefab) || SNUtil.match(pi, VanillaCreatures.GHOST_LEVIATHAN_BABY.prefab)) {
				go.EnsureComponent<GhostGelTracker>().setup();
			}
			else if (DEIntegrationSystem.instance.isLoaded() && !go.GetComponent<WaterParkCreature>() && SNUtil.match(go, DEIntegrationSystem.instance.getThalassacean(), DEIntegrationSystem.instance.getLRThalassacean())) {
				go.EnsureComponent<DEIntegrationSystem.C2CThalassacean>();
			}
			else if (DEIntegrationSystem.instance.isLoaded() && !go.GetComponent<WaterParkCreature>() && SNUtil.match(go, DEIntegrationSystem.instance.getGulper())) {
				go.EnsureComponent<DEIntegrationSystem.C2CGulper>();
			}
			else if (SNUtil.match(pi, "61ac1241-e990-4646-a618-bddb6960325b")) {
				if (Vector3.Distance(go.transform.position, Player.main.transform.position) <= 80 && go.transform.position.y < -200) {
					PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.TreaderPooPrompt).key);
				}
			}/*
			else if (SNUtil.match(pi, "e1d8b721-0edb-466e-93d3-074dc90d57f2")) {
	    		if (go.transform.position.y < -1200) {
	    			go.EnsureComponent<TechTag>().type = SeaToSeaMod.prisonPipeRoomTank;
		    	}
	    	}*/
			else if (go.isVent()) {
				go.EnsureComponent<C2CVentInteraction>();
			}
			else if (VanillaFlora.BRAIN_CORAL.includes(pi)) {
				go.EnsureComponent<C2CBrainCoral>();
			}
			else if (SNUtil.match(pi, "407e40cf-69f2-4412-8ab6-45faac5c4ea2")) {
				//SNUtil.log("Initialized lava castle smoke");
				GameObject root = go.getChildObject("CollisionHolder");
				if (!root) {
					root = new GameObject("CollisionHolder");
					root.transform.SetParent(go.transform);
				}
				root.EnsureComponent<TechTag>().type = SeaToSeaMod.lavaCastleSmoker;
				root.layer = LayerID.Useable;
				Utils.ZeroTransform(root.transform);
				Bounds bounds = go.GetComponent<Renderer>().bounds;
				Vector3 lim = bounds.max;
				Vector3 dxz = (lim-go.transform.position).setY(0).Rotated(0, -45, 0, go.transform.position);
				float dy = lim.y-go.transform.position.y;
				int n = 32;
				for (int i = 0; i < n; i++) {
					string nm = "smokeColumnVolumePoint#"+i;
					GameObject sph = root.getChildObject(nm);
					if (!sph) {
						sph = GameObject.CreatePrimitive(PrimitiveType.Sphere).setName(nm);
						sph.transform.SetParent(root.transform);
					}
					float f = i/(float)n;
					Utils.ZeroTransform(sph.transform);
					//sph.transform.position = Vector3.Lerp(gg.transform.position, lim, f);
					Vector3 offset = i == 0 ? Vector3.one*12 : Vector3.zero;
					sph.transform.position = go.transform.position + offset + dxz * f + Vector3.up * dy * Mathf.Pow(f, 0.475F);
					sph.transform.localScale = Vector3.one * ((1 + f) * 12 + 2) * 2;
					sph.layer = LayerID.Useable;
					sph.GetComponent<Collider>().isTrigger = true;
					sph.GetComponent<Renderer>().enabled = false;
					sph.EnsureComponent<LavaCastleSmokeVolumeTrigger>();

					GameObject sparkle = sph.getChildObject("Sparkle");
					if (!sparkle) {
						sparkle = ObjectUtil.lookupPrefab("505e7eff-46b3-4ad2-84e1-0fadb7be306c").getChildObject("Leviathan_enzymeBall_anim/root/xBubbles").clone().setName("Sparkle");
						sparkle.transform.SetParent(sph.transform);
					}
					Utils.ZeroTransform(sparkle.transform);
					sparkle.transform.localScale = Vector3.one * 8;
					sparkle.transform.localPosition = MathUtil.getRandomVectorAround(Vector3.zero, 0.9F);
					foreach (Renderer r in sparkle.GetComponentsInChildren<Renderer>()) {
						foreach (Material m in r.materials)
							m.SetColor("_Color", new Color(1.5F, 0.9F, 0.6F, 1F));
					}
					sparkle.GetComponent<ParticleSystem>().destroy(); //remove bubbles
					sparkle.GetComponent<ParticleSystemRenderer>().destroy();
					foreach (ParticleSystem p in sparkle.GetComponentsInChildren<ParticleSystem>()) {
						ParticleSystem.MainModule main = p.main;
						main.simulationSpeed = UnityEngine.Random.Range(0.2F, 0.3F) * 1.25F;
						main.startSize = 8 * 1.5F;
						main.startLifetime = 1.2F; //from 0.75
						p.emissionRate = UnityEngine.Random.Range(0.15F, 0.25F) * 0.25F;
						p.Play();
					}
				}


			}/*
	    	else if (SNUtil.match(pi, "SeaVoyager")) {
	    		go.EnsureComponent<C2CVoyager>();
	    	}*/
			else if (SNUtil.match(pi, "172d9440-2670-45a3-93c7-104fee6da6bc")) {
				if (Vector3.Distance(go.transform.position, C2CHooks.lostRiverCachePanel) < 2) {
					Renderer r = go.getChildObject("Precursor_Lab_infoframe/Precursor_Lab_infoframe_glass").GetComponent<Renderer>();
					r.materials[0].SetColor("_Color", new Color(1, 1, 1, /*0.43F*/0.24F));
					r.materials[0].SetColor("_SpecColor", new Color(0.38F, 1, 0.52F, 1));
					RenderUtil.setGlossiness(r.materials[0], 50, 0, 0);
					GameObject copy = r.gameObject.clone();
					copy.transform.SetParent(r.transform.parent);
					copy.transform.position = r.transform.position;
					copy.transform.rotation = r.transform.rotation;
					copy.transform.localScale = r.transform.localScale;
					Renderer r2 = copy.GetComponent<Renderer>();
					r2.materials[0].shader = Shader.Find("UWE/Marmoset/IonCrystal");
					r2.materials[0].SetInt("_ZWrite", 1);
					r2.materials[0].SetColor("_DetailsColor", Color.white);
					r2.materials[0].SetColor("_SquaresColor", new Color(1, 4, 1.5F, 2));
					r2.materials[0].SetFloat("_SquaresTile", 200F);
					r2.materials[0].SetFloat("_SquaresSpeed", 12F);
					r2.materials[0].SetFloat("_SquaresIntensityPow", 20F);
					r2.materials[0].SetVector("_NoiseSpeed", new Vector4(1, 1, 1, 1));
					r2.materials[0].SetVector("_FakeSSSparams", new Vector4(1, 15, 1, 1));
					r2.materials[0].SetVector("_FakeSSSSpeed", new Vector4(1, 1, 1, 1));
					RenderUtil.setGlossiness(r2.materials[0], 0, 0, 0);
					r.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, -709.79F);
					r2.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, -709.80F);
					GenericHandTarget ht = go.EnsureComponent<GenericHandTarget>();
					ht.onHandHover = new HandTargetEvent();
					ht.onHandClick = new HandTargetEvent();
					ht.onHandHover.AddListener(hte => {
						if (!KnownTech.knownTech.Contains(C2CItems.treatment.TechType)) {
							HandReticle.main.targetDistance = 15;
							HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
							HandReticle.main.SetInteractText("LostRiverCachePanel");
						}
					});
					ht.onHandClick.AddListener(hte => {
						if (!KnownTech.knownTech.Contains(C2CItems.treatment.TechType)) {
							KnownTech.Add(C2CItems.treatment.TechType);
							SNUtil.triggerTechPopup(C2CItems.treatment.TechType);
						}
					});
				}
			}/*
	    	else if (SNUtil.match(pi, VanillaCreatures.GHOST_LEVIATHAN && pi.GetComponentInChildren<GhostLeviatanVoid>()) {
	    		***
	    	}*/
			else if (pi && auroraFires.Contains(pi.ClassId) && EnvironmentalDamageSystem.instance.isPositionInAuroraPrawnBay(pi.transform.position)) {
				blueAuroraPrawnFire(go);
			}
			else if (SNUtil.match(pi, "b86d345e-0517-4f6e-bea4-2c5b40f623b4") && pi.transform.parent && pi.transform.parent.name.Contains("ExoRoom_Weldable")) {
				GameObject inner = go.getChildObject("Starship_doors_manual_01/Starship_doors_automatic");
				StarshipDoorLocked d = go.transform.parent.GetComponentInChildren<StarshipDoorLocked>();
				Renderer r = inner.GetComponentInChildren<Renderer>();
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/", new Dictionary<int, string>() { {
						0,
						"FireDoor"
					}, {
						1,
						"FireDoor"
					}
				});
				d.lockedTexture = (Texture2D)r.materials[0].GetTexture(Shader.PropertyToID("_Illum")); //replace all since replaced the base texture too
				d.unlockedTexture = TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/FireDoor2_Illum");
				//WeldableWallPanelGeneric panel = go.transform.parent.GetComponentInChildren<WeldableWallPanelGeneric>();
				PrawnBayDoorTriggers pt = /*panel.sendMessageFrom*/go.transform.parent.gameObject.EnsureComponent<PrawnBayDoorTriggers>();
				pt.door = d.GetComponent<StarshipDoor>();
				GenericHandTarget ht = inner.EnsureComponent<GenericHandTarget>();
				pt.hoverHint = ht;
				ht.onHandHover = new HandTargetEvent();
				ht.onHandHover.AddListener(hte => {
					HandReticle.main.SetIcon(HandReticle.IconType.Info, 1f);
					HandReticle.main.SetInteractText("PrawnBayDoorHeatWarn");
					HandReticle.main.SetTargetDistance(8);
				});
				Vector3 p1 = new Vector3(991.1F, 1F, -3.2F);
				Vector3 p2 = new Vector3(991.7F, 1F, -2.8F);/*
				GameObject rippleHolder = new GameObject("ripples");
				rippleHolder.transform.parent = go.transform.parent;
				rippleHolder.transform.localPosition = Vector3.zero;
				GameObject vent = ObjectUtil.lookupPrefab("5bbd405c-ca10-4da8-832b-87558c42f4dc");
				GameObject bubble = vent.getChildObject("xThermalVent_Dark_Big/xBubbles");
				int n = 5;
				for (int i = 0; i <= n; i++) {
					GameObject p = bubble.clone();
					p.transform.parent = rippleHolder.transform;
					p.transform.position = Vector3.Lerp(p1, p2, i/(float)n);
					p.GetComponentInChildren<Renderer>().materials[0].color = new Color(-8, -8, -8, 0.3F);
				}*/
				GameObject fire = ObjectUtil.createWorldObject("3877d31d-37a5-4c94-8eef-881a500c58bc");
				fire.transform.parent = go.transform;
				fire.transform.position = Vector3.Lerp(p1, p2, 0.5F) + new Vector3(1.3F, -0.05F, -1.7F);
				fire.transform.localScale = new Vector3(1.8F, 1, 1.8F);
				blueAuroraPrawnFire(fire);
				//fire.removeComponent<VFXExtinguishableFire>();
				LiveMixin lv = fire.GetComponent<LiveMixin>();
				lv.invincible = true;
				lv.data.maxHealth = 40000;
				lv.health = lv.data.maxHealth;
			}
			else if (pi && SeaToSeaMod.lrCoralClusters.Contains(pi.ClassId)) {
				string name = go.name;
				go.EnsureComponent<TechTag>().type = C2CItems.brineCoral;
				//if (!hard) require azurite battery if not set from 1600 to 750
				//	go.EnsureComponent<Rigidbody>().mass = 750;
				GameObject pfb = ObjectUtil.lookupPrefab(VanillaResources.LARGE_QUARTZ.prefab);
				go.makeMapRoomScannable(C2CItems.brineCoral);
				Drillable d = go.EnsureComponent<Drillable>();
				d.copyObject(pfb.GetComponent<Drillable>());
				go.name = name;
				/*
	    		d.deleteWhenDrilled = true;
	    		d.kChanceToSpawnResources = 1;
	    		d.lootPinataOnSpawn = true;
	    		d.minResourcesToSpawn = 1;
	    		d.maxResourcesToSpawn = 2;*/
				d.resources = new Drillable.ResourceType[] { new Drillable.ResourceType() {
						techType = C2CItems.brineCoralPiece.TechType,
						chance = 1
					}
				};
				bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
				d.kChanceToSpawnResources = 1;
				d.minResourcesToSpawn = hard ? 1 : 2;
				d.maxResourcesToSpawn = hard ? 3 : 4;
				go.EnsureComponent<BrineCoralTag>();
				d.onDrilled += (dr) => {
					dr.GetComponent<BrineCoralTag>().onDrilled();
				};
			}
			else if (SNUtil.match(pi, "58247109-68b9-411f-b90f-63461df9753a") && Vector3.Distance(C2CHooks.deepDegasiTablet, go.transform.position) <= 0.2) {
				GameObject go2 = ObjectUtil.createWorldObject(C2CItems.brokenOrangeTablet.ClassID);
				go2.transform.position = go.transform.position;
				go2.transform.rotation = go.transform.rotation;
				go.destroy(false);
			}
			else if (pi && (pi.ClassId == "92fb421e-a3f6-4b0b-8542-fd4faee4202a" || pi.classId == "53ffa3e8-f2f7-43b8-a5c7-946e766aff64")) {
				for (int i = 0; i < C2CHooks.purpleTabletsToBreak.Count; i++) {
					Vector3 pos = C2CHooks.purpleTabletsToBreak[i];
					if (Vector3.Distance(pos, go.transform.position) <= 0.2) {
						GameObject go2 = ObjectUtil.createWorldObject(i == 2 ? SeaToSeaMod.purpleTabletPartA.ClassID : "83b61f89-1456-4ff5-815a-ecdc9b6cc9e4");
						go2.transform.position = go.transform.position;
						go2.transform.rotation = go.transform.rotation;
						go.destroy(false);
					}
				}
			}
			else if (SNUtil.match(pi, "83b61f89-1456-4ff5-815a-ecdc9b6cc9e4")) { //broken purple tablet
				GameObject light = ObjectUtil.lookupPrefab("53ffa3e8-f2f7-43b8-a5c7-946e766aff64").GetComponentInChildren<Light>().gameObject;
				Vector3 rel = light.transform.localPosition;
				light = light.clone();
				light.transform.parent = pi.transform;
				light.transform.localPosition = rel;
				Light l = light.GetComponent<Light>();
				l.intensity *= 0.67F;
				//l.intensity = 0.4F;
				//l.range = 18F;
				//l.color = new Color(0.573, 0.404, 1.000, 1.000);
				l.shadows = LightShadows.Soft;
				FlickeringLight f = light.EnsureComponent<FlickeringLight>();
				f.dutyCycle = 0.5F;
				f.updateRate = 0.3F;
				f.fadeRate = 5F;
				go.EnsureComponent<TabletFragmentTag>();
				/*
				GameObject models = pi.gameObject.getChildObject("precursor_key_cracked_01");
				MeshRenderer[] parts = models.GetComponentsInChildren<MeshRenderer>();
				SNUtil.log("Checking fragmentation of purple tablet @ "+go.transform.position+": "+parts.Length+" in map "+purpleTabletsToRemoveParts.toDebugString());
				if (parts.Length == 2) {
					foreach (KeyValuePair<Vector3, bool[]> kvp in purpleTabletsToRemoveParts) {
		    			if (Vector3.Distance(kvp.Key, go.transform.position) <= 0.2) {
							SNUtil.log("Found match "+kvp.Value.toDebugString());
							for (int i = 0; i < parts.Length; i++) {
								parts[i].gameObject.SetActive(i >= kvp.Value.Length || kvp.Value[i]);
							}
							break;
		    			}
					}
				}*/
			}
			else if (SNUtil.match(pi, "1c34945a-656d-4f70-bf86-8bc101a27eee")) {
				go.EnsureComponent<C2CMoth>();
				go.EnsureComponent<BrightLightController>().setLightValues(120, 1.75F, 135, 180, 2.5F).setPowerValues(0.15F, 0.5F);
				go.EnsureComponent<SeamothTetherController>();
				//go.EnsureComponent<VoidSpikeLeviathanSystem.SeamothStealthManager>();
			}
			else if (SNUtil.match(pi, "ba3fb98d-e408-47eb-aa6c-12e14516446b")) { //prawn
				TemperatureDamage td = go.EnsureComponent<TemperatureDamage>();
				td.minDamageTemperature = 350;
				td.baseDamagePerSecond = Mathf.Max(10, td.baseDamagePerSecond) * 0.33F;
				td.onlyLavaDamage = false;
				td.InvokeRepeating("UpdateDamage", 1f, 1f);
				//go.removeComponent<ImmuneToPropulsioncannon>();
				go.EnsureComponent<BrightLightController>().setLightValues(120, 1.6F, 120, 150, 2.25F).setPowerValues(0.25F, 0.67F);
			}
			else if (SNUtil.match(pi, "8b113c46-c273-4112-b7ef-65c50d2591ed")) { //rocket
				go.EnsureComponent<C2CRocket>();
			}
			else if (SNUtil.match(pi, "d4be3a5d-67c3-4345-af25-7663da2d2898")) { //cuddlefish
				Pickupable p = go.EnsureComponent<Pickupable>();
				p.isPickupable = true;
				p.overrideTechType = TechType.Cutefish;
			}
			/*
	    	else if (SNUtil.match(pi, auroraStorageModule.prefabName && Vector3.Distance(auroraStorageModule.position, go.transform.position) <= 0.2) {
	    		go.transform.position = auroraCyclopsModule.position;
	    		go.transform.rotation = auroraCyclopsModule.rotation;
	    	}
	    	else if (SNUtil.match(pi, auroraCyclopsModule.prefabName && Vector3.Distance(auroraCyclopsModule.position, go.transform.position) <= 0.2) {
	    		go.transform.position = auroraStorageModule.position;
	    		go.transform.rotation = auroraStorageModule.rotation;
	    	}*/
			else if (SNUtil.match(pi, C2CHooks.auroraDepthModule.prefabName) && Vector3.Distance(C2CHooks.auroraDepthModule.position, go.transform.position) <= 0.2) {
				GameObject go2 = ObjectUtil.createWorldObject(SeaToSeaMod.brokenAuroraDepthModule.ClassID);
				go2.transform.position = go.transform.position;
				go2.transform.rotation = go.transform.rotation;
				go.destroy(false);
			}
			else if (SNUtil.match(pi, "bc9354f8-2377-411b-be1f-01ea1914ec49") && Vector3.Distance(C2CHooks.auroraRepulsionGunTerminal, go.transform.position) <= 0.2) {
				pi.GetComponent<StoryHandTarget>().goal = SeaToSeaMod.auroraTerminal;
			}
			else if (pi && pi.GetComponent<BlueprintHandTarget>()) {
				DamagedDataboxSystem.instance.onDataboxSpawn(go);
				go.EnsureComponent<ImmuneToPropulsioncannon>();
			}
			else if (pi && (pi.ClassId == VanillaResources.MAGNETITE.prefab || pi.ClassId == VanillaResources.LARGE_MAGNETITE.prefab)) {
				go.EnsureComponent<Magnetic>();
			}
			else if (SNUtil.match(pi, "160e99a7-cb46-409d-98e2-360a76ff92da")) {
				go.EnsureComponent<C2CStasisRifle>();
			}

			SubRoot sub = (bool)pi ? pi.GetComponent<SubRoot>() : go.GetComponent<SubRoot>();
			if (sub) {
				go.EnsureComponent<Magnetic>();
				if (sub.isCyclops)
					go.EnsureComponent<BrightLightController>().setLightValues(0, 0, 135, 200, 2.0F).setPowerValues(0, /*0.4F1.6F*/0.6F);
			}
			if (go.GetComponent<BaseCell>() || go.GetComponent<Constructable>() || go.FindAncestor<Vehicle>()) {
				go.EnsureComponent<Magnetic>();
			}
			if (go.GetComponent<MeleeAttack>()) {
				go.EnsureComponent<AttackRelay>();
			}
			if (pi && !floaterRocks.Contains(pi.ClassId) && CraftData.GetTechType(go) != TechType.FloatingStone && go.GetComponent<Drillable>()) {
				Rigidbody rb = go.FindAncestor<Rigidbody>();
				if (rb)
					rb.mass = Mathf.Max(2400, rb.mass);
			}
			if (pi)
				KeypadCodeSwappingSystem.instance.handleDoor(pi);

			WeldableWallPanelGeneric panel = go.GetComponent<WeldableWallPanelGeneric>();
			if (panel && panel.liveMixin)
				panel.liveMixin.data.canResurrect = true;
		}

		public static void onFireSpawn(VFXExtinguishableFire fire) {/*
	    	SNUtil.log("Spawned fire "+fire+" @ "+fire.transform.position);
	    	PrefabIdentifier pi = fire.gameObject.FindAncestor<PrefabIdentifier>();
	    	SNUtil.log("pi: "+(pi ? pi.classId : "null"));
	    	if (pi && auroraFires.Contains(pi.ClassId)) {
	    		blueAuroraPrawnFire(pi.gameObject);
	    	}*/
			fire.gameObject.EnsureComponent<AuroraFireChecker>();
		}

		private static void blueAuroraPrawnFire(GameObject fire) {
			fire.EnsureComponent<AuroraFireBluer>();
		}

		private class AuroraFireChecker : MonoBehaviour {

			void Update() {
				PrefabIdentifier pi = gameObject.FindAncestor<PrefabIdentifier>();
				if (pi) {
					if (auroraFires.Contains(pi.ClassId) && EnvironmentalDamageSystem.instance.isPositionInAuroraPrawnBay(pi.transform.position)) {
						pi.gameObject.EnsureComponent<AuroraFireBluer>();
					}
					this.destroy(false);
				}
			}

		}

		private class AuroraFireBluer : MonoBehaviour {

			private float age;

			void Update() {
				age += Time.deltaTime;
				bool flag = false;
				//SNUtil.log("Trying to blue prawn bay fire "+gameObject.name+" @ "+transform.position);
				foreach (Renderer r in this.GetComponentsInChildren<Renderer>(true)) {
					if (!r || r.name == null || r.materials == null || r.materials.Length == 0)
						continue;
					//SNUtil.log("Checking renderer "+r.name+" in "+r.gameObject.GetFullHierarchyPath());
					if (auroraPrawnFireColors.ContainsKey(r.name)) {
						foreach (Material m in r.materials) {
							//SNUtil.log("Setting color to "+auroraPrawnFireColors[r.name]);
							if (!m)
								continue;
							m.color = auroraPrawnFireColors[r.name];
							flag = true;
						}
					}
				}
				Light l = this.GetComponentInChildren<Light>();
				if (l)
					l.color = new Color(0.55F, 0.67F, 1F);
				if (flag && age >= 0.5F) {
					Light l2 = gameObject.addLight(0.4F, 32F, l.color).setName("BlueFireLight");
					//SNUtil.log("Bluing complete. Destroying component.");
					this.destroy(false);
				}
			}

		}

		class PrawnBayDoorTriggers : MonoBehaviour {

			internal GenericHandTarget hoverHint;

			internal StarshipDoor door;

			private bool wasOpen;

			public void UnlockDoor() {
				if (hoverHint)
					hoverHint.destroy();
			}

			private void Update() {
				if (door && door.doorOpen && !wasOpen) {
					wasOpen = true;
					EnvironmentalDamageSystem.instance.triggerAuroraPrawnBayWarning();
					Player.main.liveMixin.TakeDamage(5, Player.main.transform.position, DamageType.Heat, gameObject);
				}
			}

		}

		class AttackRelay : MonoBehaviour {

			void OnMeleeAttack(GameObject target) {
				if (target == Player.main.gameObject)
					LifeformScanningSystem.instance.onObjectSeen(gameObject, false);
			}

		}
	}
}

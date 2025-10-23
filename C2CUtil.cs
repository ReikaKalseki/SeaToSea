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

		internal static readonly HashSet<BiomeBase> safeBiomes = new HashSet<BiomeBase>();
		//safe at surface

		private static readonly SoundManager.SoundData rescueSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "rescuewarp", "Sounds/rescue.ogg", SoundManager.soundMode3D, s => {
			SoundManager.setup3D(s, 64);
		}, SoundSystem.masterBus);

		private static UnityEngine.UI.Button rescuePDAButton;

		static C2CUtil() {
			safeBiomes.Add(VanillaBiomes.SHALLOWS);
			safeBiomes.Add(VanillaBiomes.KELP);
			safeBiomes.Add(VanillaBiomes.REDGRASS);
			safeBiomes.Add(VanillaBiomes.MUSHROOM);
			safeBiomes.Add(VanillaBiomes.GRANDREEF);
			safeBiomes.Add(VanillaBiomes.SPARSE);
			safeBiomes.Add(VanillaBiomes.TREADER);
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

		public static void createRescuePDAButton() {
			if (rescuePDAButton)
				return;
			rescuePDAButton = SNUtil.createPDAUIButtonUnderTab<uGUI_InventoryTab>(TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/RescueUIBtn"), requestRescue).setName("RescueButton");
			rescuePDAButton.transform.localPosition = new Vector3(-12, rescuePDAButton.transform.localPosition.y, rescuePDAButton.transform.localPosition.z);
			rescuePDAButton.GetComponent<UnityEngine.UI.Image>().color = new Color(1.5F, 0.5F, 0.5F, 1);

		}

		public static void requestRescue() {
			Player.main.GetPDA().Close();
			Player.main.gameObject.EnsureComponent<TeleportCallback>().StartCoroutine("raiseConfirmationDialog");
		}

		public static bool rescue() {
			if (!Player.main.currentSub)
				return false;
			HashSet<TechType> allowedReturn = new HashSet<TechType>() {
				//TechType.LaserCutter, you do not need this to return
				TechType.Builder,
				TechType.Welder,
				TechType.Scanner,
				TechType.Knife,
				TechType.HeatBlade,
				//TechType.StasisRifle,
				TechType.AirBladder,
				TechType.Seaglide,
				TechType.Fins,
				TechType.UltraGlideFins,
				TechType.SwimChargeFins,
				TechType.ReinforcedDiveSuit,
				TechType.ReinforcedGloves,
				TechType.RadiationSuit,
				TechType.RadiationHelmet,
				TechType.RadiationGloves,
				TechType.Stillsuit,
				TechType.Tank,
				TechType.DoubleTank,
				TechType.PlasteelTank,
				TechType.HighCapacityTank,
				TechType.Rebreather,
				C2CItems.sealSuit.TechType,
				C2CItems.sealGloves.TechType,
				C2CItems.liquidTank.TechType,
				C2CItems.rebreatherV2.TechType,
			};
			foreach (TechType has in Inventory.main.container.GetItemTypes()) {
				if (!allowedReturn.Contains(has))
					return false;
			}

			float dur = 60 * 20; //20 min
			Drunk.add(dur).intensity = 0.5F; //only slow player and make them woozy at 50% power
			HealthModifier.add(2.5F, dur); //2.5x damage for the 20 min
			EnvironmentalDamageSystem.instance.setRecoveryWarning(dur);
			//O2ConsumptionRateModifier.add(1.5F, dur); //x1.5 O2 use for the 20 min

			uGUI_PlayerDeath.main.SendMessage("TriggerDeathVignette", uGUI_PlayerDeath.DeathTypes.FadeToBlack, SendMessageOptions.RequireReceiver);
			Player.main.gameObject.SendMessage("EnableHeadCameraController", null, SendMessageOptions.RequireReceiver);
			Player.main.GetPDA().Close();
			//if (Player.main.deathMusic)
			//	Player.main.deathMusic.StartEvent();
			uGUI.main.overlays.Set(0, 1f);
			MainCameraControl.main.enabled = false;
			Player.main.playerController.inputEnabled = false;
			Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
			Player.main.GetPDA().SetIgnorePDAInput(true);
			Player.main.playerController.SetEnabled(false);
			Player.main.FreezeStats();
			Player.main.gameObject.EnsureComponent<TeleportCallback>().StartCoroutine("triggerTeleportCutscene");
			return true;
		}

		class TeleportCallback : MonoBehaviour {

			private IEnumerator raiseConfirmationDialog() {
				yield return new WaitForSeconds(0.67F);
				GameObject root = IngameMenu.main.gameObject.clone().setName("RescueConfirmation");
				root.removeChildObject("PleaseWait");
				root.removeChildObject("Options");
				root.removeChildObject("Feedback");
				root.removeChildObject("Developer");
				root.removeChildObject("PleaseWait");
				root.removeChildObject("QuitConfirmationWithSaveWarning");
				root.removeChildObject("Legend");
				root.removeChildObject("Main");
				//GameObject main = root.getChildObject("Main");
				GameObject go = root.getChildObject("QuitConfirmation");
				root.removeComponent<IngameMenu>();
				root.removeComponent<LanguageUpdater>();
				go.removeComponent<IngameMenuQuitConfirmation>();
				uGUI_InputGroup grp = go.EnsureComponent<uGUI_InputGroup>();
				grp.Select(false);
				UWE.FreezeTime.Begin("RescueConfirm", true);
				UnityEngine.UI.Text txt = go.getChildObject("Header").GetComponent<UnityEngine.UI.Text>();
				txt.text = "Are you sure you want to do this?";
				txt.fontSize = 20;
				GameObject yes = go.getChildObject("ButtonYes");
				UnityEngine.UI.Button b = yes.GetComponentInChildren<UnityEngine.UI.Button>();
				GameObject yesBtn = b.gameObject;
				UnityEngine.UI.Image img = b.image;
				b.destroy();
				b = yesBtn.EnsureComponent<UnityEngine.UI.Button>();
				b.image = img;
				root.transform.SetParent(IngameMenu.main.gameObject.transform.parent);
				root.transform.localPosition = IngameMenu.main.gameObject.transform.localPosition;
				root.SetActive(true);
				go.SetActive(true);
				b.onClick.AddListener(() => {
					this.unlockUI(grp);
					if (!rescue()) {
						if (Player.main.currentSub)
							SNUtil.writeToChat("You can only carry certified low-power Alterra equipment during the emergency rescue warp.");
						else
							SNUtil.writeToChat("Rescue warp can only be initiate from inside an Alterra seabase or mobile base platform.");
					}
				});
				go.getChildObject("ButtonNo").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
					this.unlockUI(grp);
				});
				yield break;
			}

			private void unlockUI(uGUI_InputGroup grp) {
				grp.Deselect();
				UWE.FreezeTime.End("RescueConfirm");
				grp.transform.parent.gameObject.destroy(false);
			}

			private IEnumerator triggerTeleportCutscene() {
				yield return new WaitForSeconds(1F);
				SoundManager.playSoundAt(rescueSound, Player.main.transform.position, false, 40, 1);
				yield return new WaitForSeconds(2F);
				UWE.Utils.EnterPhysicsSyncSection();
				base.gameObject.SendMessage("DisableHeadCameraController", null, SendMessageOptions.RequireReceiver);
				uGUI.main.respawning.Show();
				Player.main.ToNormalMode(true);
				if (AtmosphereDirector.main) {
					AtmosphereDirector.main.ResetDirector();
				}

				yield return new WaitForSeconds(1f);
				while (!LargeWorldStreamer.main.IsWorldSettled()) {
					yield return UWE.CoroutineUtils.waitForNextFrame;
				}

				Vector3 pos = getRandomSafePosition();
				Player.main.SetPosition(pos);

				uGUI.main.respawning.Hide();
				if (Player.main.liveMixin) {
					Player.main.liveMixin.health = 5;
				}
				Player.main.oxygenMgr.AddOxygen(1000f);
				DamageFX.main.ClearHudDamage();
				Player.main.SuffocationReset();
				yield return null;
				Player.main.precursorOutOfWater = false;
				Player.main.SetDisplaySurfaceWater(true);
				Player.main.UnfreezeStats();
				Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
				Player.main.GetPDA().SetIgnorePDAInput(false);
				Player.main.playerController.inputEnabled = true;
				Player.main.playerController.SetEnabled(true);
				Player.main.SetCurrentSub(null);
				yield return new WaitForSeconds(1f);
				UWE.Utils.ExitPhysicsSyncSection();
				SNUtil.writeToChat("You wake up an unknown amount of time later.");
				yield break;
			}
		}

		public static Vector3 getRandomSafePosition() { //surface in any of the following biomes: shallows, kelp, red grass, grand reef, sparse reef, mushroom forests
			Vector3 range = new Vector3(1500, 0, 1500);
			Vector3 ctr = Vector3.down;
			Vector3 rand = MathUtil.getRandomVectorAround(ctr, range);
			BiomeBase bb = BiomeBase.getBiome(rand);
			while (!safeBiomes.Contains(bb)) {
				rand = MathUtil.getRandomVectorAround(ctr, range);
				bb = BiomeBase.getBiome(rand);
			}
			return rand;
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
			SNUtil.writeToChat("Removed "+ptc+" platinum.");
		}
	}
}

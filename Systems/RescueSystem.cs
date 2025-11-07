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
	public class RescueSystem {

		internal static readonly HashSet<BiomeBase> safeBiomes = new HashSet<BiomeBase>();
		//safe at surface

		private static readonly SoundManager.SoundData rescueSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "rescuewarp", "Sounds/rescue.ogg", SoundManager.soundMode3D, s => {
			SoundManager.setup3D(s, 64);
		}, SoundSystem.masterBus);

		private static UnityEngine.UI.Button rescuePDAButton;

		static RescueSystem() {
			safeBiomes.Add(VanillaBiomes.SHALLOWS);
			safeBiomes.Add(VanillaBiomes.KELP);
			safeBiomes.Add(VanillaBiomes.REDGRASS);
			safeBiomes.Add(VanillaBiomes.MUSHROOM);
			safeBiomes.Add(VanillaBiomes.GRANDREEF);
			safeBiomes.Add(VanillaBiomes.SPARSE);
			safeBiomes.Add(VanillaBiomes.TREADER);
		}
		public static void createRescuePDAButton() {
			if (rescuePDAButton)
				return;
			rescuePDAButton = SNUtil.createPDAUIButtonUnderTab<uGUI_InventoryTab>(TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/RescueUIBtn"), requestRescue).setName("RescueButton");
			rescuePDAButton.transform.localPosition = new Vector3(-12, rescuePDAButton.transform.localPosition.y, rescuePDAButton.transform.localPosition.z);
			rescuePDAButton.GetComponent<UnityEngine.UI.Image>().color = new Color(1.5F, 0.5F, 0.5F, 1);

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
				this.destroy(false);
				yield break;
			}
		}


	}
}

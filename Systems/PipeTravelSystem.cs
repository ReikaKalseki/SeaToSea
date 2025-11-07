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
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	public class PipeTravelSystem {

		private static readonly List<string> impactSounds = new List<string>();
		private static readonly List<string> hurtSounds = new List<string>();

		private static readonly SoundManager.SoundData travelInitSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "venttravel", "Sounds/venttravel.ogg", SoundManager.soundMode3D, s => {
			SoundManager.setup3D(s, 64);
		}, SoundSystem.masterBus);

		static PipeTravelSystem() {
			impactSounds.Add("event:/sub/seamoth/impact_solid_medium");
			impactSounds.Add("event:/sub/seamoth/impact_solid_hard");

			//hurtSounds.Add("event:/player/Pain");
			hurtSounds.Add("event:/player/Pain_no_tank_light");
			hurtSounds.Add("event:/player/Pain_no_tank");
			hurtSounds.Add("event:/player/Pain_surface");
			hurtSounds.Add("event:/player/Pain_surface");
			hurtSounds.Add("event:/player/Pain_surface"); //3x rate
			//hurtSounds.Add("event:/player/damage");
			//hurtSounds.Add("event:/player/heat_damage");
			//hurtSounds.Add("event:/player/hungry_damage");
			//hurtSounds.Add("event:/player/thirsty_damage");
		}

		public static void requestTravel(PrefabIdentifier vent) {
			Player.main.gameObject.EnsureComponent<PipeTravelCallback>().initiateTravel(vent);
		}

		public static void beginTravel() {
			uGUI_PlayerDeath.main.SendMessage("TriggerDeathVignette", uGUI_PlayerDeath.DeathTypes.FadeToBlack, SendMessageOptions.RequireReceiver);
			Player.main.gameObject.SendMessage("EnableHeadCameraController", null, SendMessageOptions.RequireReceiver);
			Player.main.GetPDA().Close();
			uGUI.main.overlays.Set(0, 1f);
			MainCameraControl.main.enabled = false;
			Player.main.playerController.inputEnabled = false;
			Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
			Player.main.GetPDA().SetIgnorePDAInput(true);
			Player.main.playerController.SetEnabled(false);
			Player.main.FreezeStats();
			Player.main.gameObject.EnsureComponent<PipeTravelCallback>().StartCoroutine("triggerTravelCutscene");
		}

		class PipeTravelCallback : MonoBehaviour {

			private PrefabIdentifier startPosition;
			private Vector3 targetPosition;

			private List<float> hitTimes = new List<float>();

			private static readonly float TOTAL_SECONDS = 9;

			public void initiateTravel(PrefabIdentifier vent) {
				startPosition = vent;
				targetPosition = vent.ClassId == ObjectUtil.PRISON_VENT ? Vector3.zero : Creature.prisonAquriumBounds.center;

				int hits = UnityEngine.Random.Range(4, 8); //4-7
				float lastT = 0;
				for (int i = 0; i < hits; i++) {
					float t = TOTAL_SECONDS * i / hits;
					t = Mathf.Clamp(UnityEngine.Random.Range(t-1F, t+1F), lastT+0.5F, TOTAL_SECONDS-0.5F);
					lastT = t;
					hitTimes.Add(t);
				}

				StartCoroutine("asyncTravelInit");
			}

			private void addTargetButton(GameObject menu, GameObject btnRef, string name, Vector3 pos) {
				GameObject go = btnRef.clone().setName("Btn_"+name);
				go.transform.SetParent(menu.transform);
				go.transform.position = btnRef.transform.position;
				go.transform.rotation = btnRef.transform.rotation;
				go.transform.localScale = btnRef.transform.localScale;
				go.SetActive(false);
				go.GetComponentInChildren<Text>().text = name;
				UnityEngine.UI.Button b = go.GetComponent<UnityEngine.UI.Button>();
				b.destroy();
				b = go.EnsureComponent<UnityEngine.UI.Button>();
				UnityEngine.UI.Button bRef = btnRef.GetComponent<UnityEngine.UI.Button>();
				b.image = bRef.image;
				go.SetActive(true);
				b.copySprites(bRef);
				b.onClick.AddListener(() => {
					targetPosition = pos;
					unlockUI(menu.GetComponent<uGUI_InputGroup>());
					beginTravel();
				});
			}

			private IEnumerator asyncTravelInit() {
				yield return new WaitForSeconds(0.1F);
				if (targetPosition.magnitude < 1F) {
					GameObject root = IngameMenu.main.gameObject.clone().setName("PipeTravelTargetSelection");
					root.transform.SetParent(IngameMenu.main.gameObject.transform.parent);
					root.transform.localPosition = IngameMenu.main.gameObject.transform.localPosition;

					root.removeChildObject("PleaseWait");
					root.removeChildObject("QuitConfirmationWithSaveWarning");
					root.removeChildObject("QuitConfirmation");
					root.removeChildObject("Options");
					root.removeChildObject("Feedback");
					root.removeChildObject("Developer");
					root.removeChildObject("Legend");
					root.removeComponent<IngameMenu>();
					root.removeComponent<LanguageUpdater>();
					GameObject main = root.getChildObject("Main");
					GameObject go = main.getChildObject("ButtonLayout");

					GameObject btnRef = go.getChildObject("ButtonBack");

					uGUI_InputGroup grp = go.EnsureComponent<uGUI_InputGroup>();
					grp.Select(false);

					addTargetButton(go, btnRef, "Sparse Reef Fissures", WorldUtil.SPARSE_VENT);
					addTargetButton(go, btnRef, "Mushroom Forest", WorldUtil.MUSHROOM_VENT);
					addTargetButton(go, btnRef, "Grand Reef", WorldUtil.GRANDREEF_VENT);
					addTargetButton(go, btnRef, "Underwater Islands", WorldUtil.UNDERISLAND_VENT);
					addTargetButton(go, btnRef, "Underwater Mountains", WorldUtil.MOUNTAIN_VENT);
					addTargetButton(go, btnRef, "The Dunes", WorldUtil.DUNES_VENT);

					go.removeChildObject("ButtonSave");
					go.removeChildObject("ButtonDeveloper");
					go.removeChildObject("ButtonOptions");
					go.removeChildObject("ButtonHelp");
					go.removeChildObject("ButtonFeedback");
					go.removeChildObject("ButtonQuitToMainMenu");
					btnRef.destroy();

					UnityEngine.UI.Text txt = main.getChildObject("Header").GetComponent<UnityEngine.UI.Text>();
					txt.text = "Choose destination";

					root.SetActive(true);
					main.SetActive(true);
					go.SetActive(true);
					yield return new WaitForSeconds(0.1F);
					root.SetActive(true);
					main.SetActive(true);
					go.SetActive(true);
					UWE.FreezeTime.Begin("PipeFastTravelSelect", true);
				}
				else {
					beginTravel();
				}
				yield break;
			}

			private void unlockUI(uGUI_InputGroup grp) {
				grp.Deselect();
				UWE.FreezeTime.End("PipeFastTravelSelect");
				grp.transform.parent.parent.gameObject.destroy(false);
			}

			private IEnumerator triggerTravelCutscene() {
				yield return new WaitForSeconds(0.5F);
				SoundManager.playSoundAt(travelInitSound, Player.main.transform.position, false, 40, 1);
				yield return new WaitForSeconds(1F);
				UWE.Utils.EnterPhysicsSyncSection();
				gameObject.SendMessage("DisableHeadCameraController", null, SendMessageOptions.RequireReceiver);
				uGUI.main.respawning.Show();
				Player.main.ToNormalMode(true);
				if (AtmosphereDirector.main) {
					AtmosphereDirector.main.ResetDirector();
				}

				float current = 0;
				for (int i = 0; i < hitTimes.Count; i++) {
					float time = hitTimes[i];
					yield return new WaitForSeconds(time-current);
					SoundManager.playSoundAt(SoundManager.buildSound(impactSounds.getRandomEntry()), Player.main.transform.position, false, -1F, 2);
					float del = UnityEngine.Random.Range(0.1F, 0.33F);
					yield return new WaitForSeconds(del);
					SoundManager.playSoundAt(SoundManager.buildSound(hurtSounds.getRandomEntry()), Player.main.transform.position, false, -1F, 1F);
					current = time+del;
				}

				yield return new WaitForSeconds(TOTAL_SECONDS-current);

				while (!LargeWorldStreamer.main.IsWorldSettled()) {
					yield return UWE.CoroutineUtils.waitForNextFrame;
				}

				Player.main.SetPosition(targetPosition+Vector3.up*4);
				Player.main.rigidBody.velocity = Vector3.up * 16;

				uGUI.main.respawning.Hide();
				DamageFX.main.ClearHudDamage();
				Player.main.SuffocationReset();
				yield return null;
				Player.main.precursorOutOfWater = false;
				Player.main.SetDisplaySurfaceWater(true);
				Player.main.UnfreezeStats();
				Player.main.liveMixin.TakeDamage(5);
				MoraleSystem.instance.shiftMorale(-20);
				Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
				Player.main.GetPDA().SetIgnorePDAInput(false);
				Player.main.playerController.inputEnabled = true;
				Player.main.playerController.SetEnabled(true);
				Player.main.SetCurrentSub(null);
				yield return new WaitForSeconds(1f);
				UWE.Utils.ExitPhysicsSyncSection();
				yield return new WaitForSeconds(2);
				if (AtmosphereDirector.main) {
					AtmosphereVolume vol = WorldUtil.getClosest<AtmosphereVolume>(Player.main.transform.position);
					if (vol)
						AtmosphereDirector.main.AddVolume(vol);
				}
				//SNUtil.writeToChat("Teleport complete");
				this.destroy(false);
				yield break;
			}
		}


	}
}

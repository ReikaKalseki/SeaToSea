using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Json;
using SMLHelper.V2.Utility;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	internal static class C2CUtil {
    
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
		
		static C2CUtil() {
			safeBiomes.Add(VanillaBiomes.SHALLOWS);
			safeBiomes.Add(VanillaBiomes.KELP);
			safeBiomes.Add(VanillaBiomes.REDGRASS);
			safeBiomes.Add(VanillaBiomes.MUSHROOM);
			safeBiomes.Add(VanillaBiomes.GRANDREEF);
			safeBiomes.Add(VanillaBiomes.SPARSE);
			safeBiomes.Add(VanillaBiomes.TREADER);
		}
		
		public static bool checkConditionAndShowPDAAndVoicelogIfNot(bool check, string page, PDAMessages.Messages msg) {
			if (check) {
				return true;
			}
			else {
				if (PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(msg).key)) {
					if (!string.IsNullOrEmpty(page))
						PDAManager.getPage(page).unlock(false);
				}
				return false;
			}
		}
   
		public static bool playerCanHeal() {
			Player ep = Player.main;
			if (EnvironmentalDamageSystem.instance.isPlayerRecoveringFromPressure())
				return false;
			if (ep.IsSwimming() && ep.GetDepth() >= EnvironmentalDamageSystem.depthDamageStart && !LiquidBreathingSystem.instance.hasLiquidBreathing())
				return false;
			return true;
		}
    
		internal static void relockRecipes() {
			foreach (TechType tt in C2CRecipes.getRemovedVanillaUnlocks()) {
				KnownTech.knownTech.Remove(tt);
			}
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
				TechType.StasisRifle,
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
		
			private IEnumerator triggerTeleportCutscene() {
				yield return new WaitForSeconds(3F);
				UWE.Utils.EnterPhysicsSyncSection();
				base.gameObject.SendMessage("DisableHeadCameraController", null, SendMessageOptions.RequireReceiver);
				uGUI.main.respawning.Show();
				Player.main.ToNormalMode(true);
				if (AtmosphereDirector.main) {
					AtmosphereDirector.main.ResetDirector();
				}
			
				yield return new WaitForSeconds(1f);
				LargeWorldStreamer streamer = LargeWorldStreamer.main;
				while (!streamer.IsWorldSettled()) {
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
			if (!safeBiomes.Contains(bb)) {
				rand = MathUtil.getRandomVectorAround(ctr, range);
				bb = BiomeBase.getBiome(rand);
			}
			return rand;
		}
		/*
	public static bool hasNoGasMask() {
   		return Inventory.main.equipment.GetCount(TechType.Rebreather) == 0 && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0;
	}*/
		/*
   public static void generateLavaCastleAzurite() {
   	List<GameObject> azurite = new List<GameObject>();
   	string azur = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).ClassID;
   	foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
   		if (pi.ClassId == "407e40cf-69f2-4412-8ab6-45faac5c4ea2") {
   			for (int ang = 0; ang < 360; ang += 10) {
   				float a = UnityEngine.Random.Range(ang-5F, ang+5F);
   				float r = 16;
   				Vector3 dt = new Vector3(Mathf.Cos(a)*r, -UnityEngine.Random.Range(0, UnityEngine.Random.Range(25, 40)), Mathf.Sin(a)*r);
   				Vector3 vec = pi.transform.position+dt;
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
							go.transform.Rotate(Vector3.up*UnityEngine.Random.Range(0F, 360F), Space.Self);
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
   }*/
	   
		public static void generateLRNestPlants() {
			Vector3 p1 = new Vector3(-786, -762.6F, -321);
			Vector3 p2 = new Vector3(-801, -764.9F, -280);
		   	
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
	   	
			string path = BuildingHandler.instance.getDumpFile("lr_nest");
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			
			CustomEgg egg = CustomEgg.getEgg(TechType.SpineEel);
			
			foreach (GameObject go in plants) {
				PositionedPrefab pfb = new PositionedPrefab(go.GetComponent<PrefabIdentifier>());
				XmlElement e = doc.CreateElement("customprefab");
				pfb.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}
			
			doc.Save(path);
		}
		
	}
}

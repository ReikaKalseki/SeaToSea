using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using FMOD;
using FMOD.Studio;
using FMODUnity;

namespace ReikaKalseki.SeaToSea
{
		internal class C2CReaper : MonoBehaviour {
		
			private static float[] defaultGlows;
			private static Texture[] defaultTextures;
			
			private static Texture flatTexture = TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/reapersonarglow");
		
			private Renderer renderer;
			private FMOD_CustomLoopingEmitter roar1;
			private FMOD_CustomLoopingEmitterWithCallback roar2;
			
			private float forcedGlowFactor;
        	
			void Start() {
				//base.InvokeRepeating("tick", 0f, 1);
			}
			
			void Update() {
				if (!MainCamera.camera)
					return;
				//SNUtil.log("A");
				if (!renderer) {
					renderer = ObjectUtil.getChildObject(gameObject, "reaper_leviathan/Reaper_Leviathan_geo").GetComponentInChildren<Renderer>();
					//SNUtil.log(""+renderer);
					if (!renderer)
						return;
					defaultGlows = new float[renderer.materials.Length];
					defaultTextures = new Texture2D[renderer.materials.Length];
					for (int i = 0; i < renderer.materials.Length; i++) {
						defaultGlows[i] = renderer.materials[i].GetFloat("_GlowStrength");
						defaultTextures[i] = renderer.materials[i].GetTexture("_Illum");
					}
				}
				if (!roar1) {
					foreach (FMOD_CustomLoopingEmitter em in GetComponents<FMOD_CustomLoopingEmitter>()) {
						if (em.asset != null && em.asset.path.Contains("idle")) {
							roar1 = em;
							break;
						}
					}
					roar2 = GetComponent<FMOD_CustomLoopingEmitterWithCallback>();
				}
				//SNUtil.log("B");
				float distq = (transform.position-MainCamera.camera.transform.position).sqrMagnitude;
				float f = Mathf.Clamp01((distq-14400F)/(78400F));
				float glow = 40000*f*f;
				//SNUtil.writeToChat(distq.ToString("000.0")+">"+f.ToString("0.000")+">"+glow.ToString("0000000.0")+"@"+forcedGlowFactor.ToString("0.000"));
				//SNUtil.log("C");
				for (int i = 0; i < renderer.materials.Length; i++) {
					RenderUtil.setEmissivity(renderer.materials[i], Mathf.Lerp(defaultGlows[i], glow, forcedGlowFactor), "GlowStrength");
					renderer.materials[i].SetTexture("_Illum", glow*forcedGlowFactor > 0 ? flatTexture : defaultTextures[i]);
				}
				//SNUtil.log("D");
				if (isInVehicleWithSonar()) {
					float dT = Time.deltaTime;
					if (isRoaring(roar1) || isRoaring(roar2)) {
						forcedGlowFactor = Mathf.Min(1, forcedGlowFactor+2.5F*dT);
					}
					else {
						forcedGlowFactor = Mathf.Max(0, forcedGlowFactor-0.2F*dT);
					}
				}
				else {
					forcedGlowFactor = 0;
				}
			}
			
			private bool isRoaring(FMOD_CustomEmitter emit) {/*
				if (!emit._playing || !emit.evt.hasHandle())
					return false;/*
				int ms;
				if (emit.evt.getTimelinePosition(out ms) == FMOD.RESULT.OK) {
					SNUtil.writeToChat(ms+"ms for "+emit.GetType().Name);
					return ms >= 0 && ms <= 1500;
				}*//*
				return false;*/
					return emit.playing;
			}
			
			private bool isInVehicleWithSonar() {
				if (Player.main) {
					Vehicle v = Player.main.GetVehicle();
					if (v && InventoryUtil.vehicleHasUpgrade(v, TechType.SeamothSonarModule))
						return true;
					SubRoot sub = Player.main.currentSub;
					if (sub && sub.isCyclops && InventoryUtil.cyclopsHasUpgrade(sub, TechType.CyclopsSonarModule))
						return true;
				}
				return false;
			}

			private void OnKill() {
				UnityEngine.Object.Destroy(this);
			}
			
			void OnDisable() {
				//base.CancelInvoke("tick");
			}
			
			internal void fireRoar() {
				forcedGlowFactor = 1;
			}
			
		}
}

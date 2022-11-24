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
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{
		internal class C2CMoth : MonoBehaviour {
		
			private static readonly SoundManager.SoundData donePurgingSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "doneheatsink", "Sounds/doneheatsink.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 40);}, SoundSystem.masterBus);
			private static readonly SoundManager.SoundData meltingSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "seamothmelt", "Sounds/seamothmelt2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 120);}, SoundSystem.masterBus);
			private static readonly SoundManager.SoundData ejectionPrepareSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "heatsinkEjectPrepare", "Sounds/heatsinkejectprepare.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 120);}, SoundSystem.masterBus);
		
			internal static bool useSeamothVehicleTemperature = true;
			
			public static bool temperatureDebugActive = false;
		
			private static readonly float TICK_RATE = 0.1F;
			private static readonly float HOLD_LOW_TIME = 30F;
			
			public static float getOverrideTemperature(float temp) {
				if (!useSeamothVehicleTemperature)
					return temp;
				Player ep = Player.main;
				if (!ep)
					return temp;
				Vehicle v = ep.GetVehicle();
				if (!v)
					return temp;
				return getOverrideTemperature(v, temp);
			}
			
			public static float getOverrideTemperature(Vehicle v, float temp) {
				if (!useSeamothVehicleTemperature)
					return temp;
				if (v is SeaMoth) {
					C2CMoth cm = v.GetComponent<C2CMoth>();
					if (cm)
						return cm.vehicleTemperature;
				}
				return temp;
			}
		
			private SeaMoth seamoth;
			private TemperatureDamage temperatureDamage;
			private VFXVehicleDamages damageFX;
			
			private float baseDamageAmount;
			
			private float vehicleTemperature = 0;
			
			private float purgePower = -1;
			private float holdTempLowTime = 0;
			
			private float lastMeltSound = -1;
			private float lastPreEjectSound = -1;
			
			private float lastTickTime = -1;
        	
			void Start() {
				useSeamothVehicleTemperature = false;
				vehicleTemperature = WaterTemperatureSimulation.main.GetTemperature(transform.position);
				useSeamothVehicleTemperature = true;
			}
			
			void Update() {
				float time = DayNightCycle.main.timePassedAsFloat;
				float dT = time-lastTickTime;
				if (dT >= TICK_RATE) {
					tick(time, Mathf.Min(1, dT));
					lastTickTime = time;
				}
			}
			
			internal void purgeHeat(float charge) {
				purgePower = charge;
				
				//Invoke("fireHeatsink", 1.5F);
				SoundManager.playSoundAt(donePurgingSound, transform.position, false, 40, 0.67F);
			}
			
			internal void fireHeatsink() {
				GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.ejectedHeatSink.ClassID);
				go.transform.position = seamoth.transform.position+seamoth.transform.forward*4;
				go.GetComponent<Rigidbody>().AddForce(seamoth.transform.forward*10, ForceMode.VelocityChange);
				go.GetComponent<HeatSinkTag>().onFired(purgePower);
			}
			
			internal bool isPurgingHeat() {
				return purgePower > 0;
			}
			
			public float getTemperature() {
				return vehicleTemperature;
			}
			
			internal void onHitByLavaBomb(LavaBombTag bomb) {
				vehicleTemperature = Mathf.Max(vehicleTemperature, bomb.getTemperature());
			}

			internal void tick(float time, float tickTime) {
				if (!seamoth)
					seamoth = gameObject.GetComponent<SeaMoth>();
				if (!temperatureDamage) {
					temperatureDamage = gameObject.GetComponent<TemperatureDamage>();
					baseDamageAmount = temperatureDamage.baseDamagePerSecond;
				}
				if (!damageFX)
					damageFX = gameObject.GetComponent<VFXVehicleDamages>();
				
				if (isPurgingHeat()) {
					vehicleTemperature -= tickTime*150*(0.2F+0.8F*purgePower);
					if (vehicleTemperature <= 5) {
						vehicleTemperature = 5;
						holdTempLowTime += tickTime;
						if (time-lastPreEjectSound >= 0.75F) {
							float f = holdTempLowTime/(HOLD_LOW_TIME*purgePower);
							SoundManager.playSoundAt(ejectionPrepareSound, Player.main.transform.position, false, -1, f*f);
							lastPreEjectSound = time;
						}
						if (holdTempLowTime >= HOLD_LOW_TIME*purgePower) {
							fireHeatsink();
							purgePower = -1;
						}
					}
					else {
						holdTempLowTime = 0;
					}
					if (temperatureDebugActive)
						SNUtil.writeToChat("Purging: "+purgePower.ToString("0.000")+" > "+vehicleTemperature.ToString("0000.00")+" > "+holdTempLowTime.ToString("00.00"));
				}
				else {
					holdTempLowTime = 0;
					useSeamothVehicleTemperature = false;
					float Tamb = temperatureDamage.GetTemperature();// this will call WaterTempSim, after the lava checks in DI
					useSeamothVehicleTemperature = true;
					float dT = Tamb-vehicleTemperature;
					float excess = Mathf.Clamp01((vehicleTemperature-400)/400F);
					float f0 = dT > 0 ? 4F : 25F-15*excess;
					float f1 = dT > 0 ? 5F : 1F+1.5F*excess;
					float speed = seamoth.useRigidbody.velocity.magnitude;
					if (speed >= 2) {
						f0 /= 1+(speed-2)/5F;
					}
					float qDot = tickTime*Math.Sign(dT)*Mathf.Min(Math.Abs(dT), Mathf.Max(f1, Math.Abs(dT)/f0));
					vehicleTemperature += qDot;
					if (temperatureDebugActive)
						SNUtil.writeToChat(Tamb+" > "+dT+" > "+speed.ToString("00.0")+" > "+f0.ToString("00.0000")+" > "+qDot.ToString("00.0000")+" > "+vehicleTemperature.ToString("0000.00"));
				}
				float factor = 1+Mathf.Max(0, vehicleTemperature-250)/25F;
				float f2 = Mathf.Min(40, Mathf.Pow(factor, 2.5F));
				temperatureDamage.baseDamagePerSecond = baseDamageAmount*f2;
				//SNUtil.writeToChat(vehicleTemperature+" > "+factor.ToString("00.0000")+" > "+f2.ToString("00.0000")+" > "+temperatureDamage.baseDamagePerSecond.ToString("0000.00"));
				if (vehicleTemperature >= 90) {
					damageFX.OnTakeDamage(new DamageInfo{damage = 1, type = DamageType.Heat});
					if (time-lastMeltSound >= 0.5F && UnityEngine.Random.Range(0F, 1F) <= 0.25F) {
						SoundManager.playSoundAt(meltingSound, Player.main.transform.position, false, -1, 0.125F+Mathf.Clamp01((vehicleTemperature-90)/100F)*0.125F);
						lastMeltSound = time;
					}
				}
			}
			
		}
}

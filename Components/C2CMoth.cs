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
using FMOD;
using FMODUnity;

namespace ReikaKalseki.SeaToSea
{
		internal class C2CMoth : MonoBehaviour {
		
			private static readonly SoundManager.SoundData startPurgingSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "startheatsink", "Sounds/startheatsink2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 40);}, SoundSystem.masterBus);
			private static readonly SoundManager.SoundData meltingSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "seamothmelt", "Sounds/seamothmelt2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 120);}, SoundSystem.masterBus);
			//private static readonly SoundManager.SoundData ejectionPrepareSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "heatsinkEjectPrepare", "Sounds/heatsinkejectprepare.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 120);}, SoundSystem.masterBus);
		
			internal static bool useSeamothVehicleTemperature = true;
			
			public static bool temperatureDebugActive = false;
		
			private static readonly float TICK_RATE = 0.1F;
			private static readonly float HOLD_LOW_TIME = 30.8F;
			
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
			
			private float holdTempLowTime = 0;
			
			private float temperatureAtPurge = -1;
			
			private float lastMeltSound = -1;
			//private float lastPreEjectSound = -1;
			
			private Channel? heatsinkSoundEvent;
			
			private float lastTickTime = -1;
			
			private float speedBonus;
        	
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
			
			internal void purgeHeat() {
				temperatureAtPurge = vehicleTemperature;
				SNUtil.log("Starting heat purge ("+temperatureAtPurge+") @ "+DayNightCycle.main.timePassedAsFloat, SeaToSeaMod.modDLL);
				//Invoke("fireHeatsink", 1.5F);
				heatsinkSoundEvent = SoundManager.playSoundAt(startPurgingSound, transform.position, false, -1, 0.67F);
			}
			
			internal void fireHeatsink(float time) {
				SNUtil.log("Heat purge complete @ "+time+" ("+holdTempLowTime+"/"+HOLD_LOW_TIME+"), firing heatsink", SeaToSeaMod.modDLL);
				GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.ejectedHeatSink.ClassID);
				go.transform.position = seamoth.transform.position+seamoth.transform.forward*4;
				go.GetComponent<Rigidbody>().AddForce(seamoth.transform.forward*-20, ForceMode.VelocityChange);
				go.GetComponent<HeatSinkTag>().onFired(Mathf.Clamp01((temperatureAtPurge/250F)*0.25F+0.75F));
			}
			
			internal void applySpeedBoost() {
				speedBonus = 1;
			}
			
			internal bool isPurgingHeat() {
				return temperatureAtPurge >= 0;
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
				
				if (speedBonus > 0)
					speedBonus *= 0.925F;
				
				if (heatsinkSoundEvent != null && heatsinkSoundEvent.Value.hasHandle()) {
					ATTRIBUTES_3D attr = transform.position.To3DAttributes();
					heatsinkSoundEvent.Value.set3DAttributes(ref attr.position, ref attr.velocity, ref attr.forward);
				}
				
				if (isPurgingHeat()) {
					vehicleTemperature -= tickTime*150;
					if (vehicleTemperature <= 5) {
						vehicleTemperature = 5;
						holdTempLowTime += tickTime;
						if (holdTempLowTime >= HOLD_LOW_TIME) {
							fireHeatsink(time);
							temperatureAtPurge = -1;
						}
					}
					else {
						holdTempLowTime = 0;
					}
					if (temperatureDebugActive)
						SNUtil.writeToChat("Purging: "+vehicleTemperature.ToString("0000.00")+" > "+holdTempLowTime.ToString("00.00"));
				}
				else {
					holdTempLowTime = 0;
					useSeamothVehicleTemperature = false;
					float Tamb = temperatureDamage.GetTemperature();// this will call WaterTempSim, after the lava checks in DI
					if (seamoth.docked)
						Tamb = 25;
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
				if (vehicleTemperature >= 90 && seamoth.GetPilotingMode()) {
					damageFX.OnTakeDamage(new DamageInfo{damage = 1, type = DamageType.Heat});
					if (time-lastMeltSound >= 0.5F && !seamoth.docked && UnityEngine.Random.Range(0F, 1F) <= 0.25F) {
						SoundManager.playSoundAt(meltingSound, Player.main.transform.position, false, -1, 0.125F+Mathf.Clamp01((vehicleTemperature-90)/100F)*0.125F);
						lastMeltSound = time;
					}
				}
			}
			
		}
}

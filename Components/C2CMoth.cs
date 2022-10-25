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

namespace ReikaKalseki.SeaToSea
{
		internal class C2CMoth : MonoBehaviour {
		
			private static readonly SoundManager.SoundData donePurgingSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "doneheatsink", "Sounds/doneheatsink.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 40);}, SoundSystem.masterBus);
			private static readonly SoundManager.SoundData meltingSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "seamothmelt", "Sounds/seamothmelt.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 120);}, SoundSystem.masterBus);
		
			internal static bool useSeamothVehicleTemperature = true;
		
			private static readonly float TICK_RATE = 0.1F;
			private static readonly float HOLD_LOW_TIME = 6F;
			
			public static float getOverrideTemperature(float temp) {
				if (!useSeamothVehicleTemperature)
					return temp;
				Player ep = Player.main;
				if (!ep)
					return temp;
				Vehicle v = ep.GetVehicle();
				if (!v)
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
			
			private bool purging = false;
			private int holdTempLowTime = 0;
        	
			void Start() {
				base.InvokeRepeating("tick", 0f, TICK_RATE);
				useSeamothVehicleTemperature = false;
				vehicleTemperature = WaterTemperatureSimulation.main.GetTemperature(transform.position);
				useSeamothVehicleTemperature = true;
			}
			
			void Update() {
				
			}

			private void OnKill() {
				UnityEngine.Object.Destroy(this);
			}
			
			void OnDisable() {
				base.CancelInvoke("tick");
			}
			
			internal void purgeHeat() {
				purging = true;
			}
			
			internal bool isPurgingHeat() {
				return purging;
			}
			
			public float getTemperature() {
				return vehicleTemperature;
			}

			internal void tick() {
				if (!seamoth)
					seamoth = gameObject.GetComponent<SeaMoth>();
				if (!temperatureDamage) {
					temperatureDamage = gameObject.GetComponent<TemperatureDamage>();
					baseDamageAmount = temperatureDamage.baseDamagePerSecond;
				}
				if (!damageFX)
					damageFX = gameObject.GetComponent<VFXVehicleDamages>();
				
				if (purging) {
					vehicleTemperature -= TICK_RATE*150;
					if (vehicleTemperature <= 5) {
						vehicleTemperature = 5;
						holdTempLowTime++;
						if (holdTempLowTime >= HOLD_LOW_TIME/TICK_RATE) {
							purging = false;
							SoundManager.playSoundAt(donePurgingSound, transform.position, false, 40, 0.67F);
						}
					}
					else {
						holdTempLowTime = 0;
					}
				}
				else {
					holdTempLowTime = 0;
					useSeamothVehicleTemperature = false;
					float Tamb = WaterTemperatureSimulation.main.GetTemperature(transform.position);
					useSeamothVehicleTemperature = true;
					float dT = Tamb-vehicleTemperature;
					float f0 = dT > 0 ? 4F : 25F;
					float f1 = dT > 0 ? 5F : 1F;
					float qDot = TICK_RATE*Math.Sign(dT)*Mathf.Min(Math.Abs(dT), Mathf.Max(f1, Math.Abs(dT)/f0));
					vehicleTemperature += qDot;
					//SNUtil.writeToChat(Tamb+" > "+dT+" > "+qDot.ToString("00.0000")+" > "+vehicleTemperature.ToString("0000.00"));
				}
				float factor = 1+Mathf.Max(0, vehicleTemperature-250)/25F;
				float f2 = Mathf.Min(40, Mathf.Pow(factor, 2.5F));
				temperatureDamage.baseDamagePerSecond = baseDamageAmount*f2;
				//SNUtil.writeToChat(vehicleTemperature+" > "+factor.ToString("00.0000")+" > "+f2.ToString("00.0000")+" > "+temperatureDamage.baseDamagePerSecond.ToString("0000.00"));
				if (vehicleTemperature >= 90) {
					damageFX.OnTakeDamage(new DamageInfo{damage = 1, type = DamageType.Heat});
					//SoundManager.playSoundAt(meltingSound, Player.main.transform.position, false, -1, Mathf.Clamp01((vehicleTemperature-90)/100F));
				}
			}
			
		}
}

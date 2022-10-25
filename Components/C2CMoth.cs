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
		
			internal static bool useSeamothVehicleTemperature = true;
		
			private static readonly float TICK_RATE = 0.1F;
			
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
						if (holdTempLowTime >= 1.5F/TICK_RATE) //1.5s
							purging = false;
					}
				}
				else {
					useSeamothVehicleTemperature = false;
					float Tamb = WaterTemperatureSimulation.main.GetTemperature(transform.position);
					useSeamothVehicleTemperature = true;
					float dT = Tamb-vehicleTemperature;
					float qDot = TICK_RATE*Math.Sign(dT)*Mathf.Min(Math.Abs(dT), Mathf.Max(1, Math.Abs(dT)/25F));
					vehicleTemperature += qDot;
					//SNUtil.writeToChat(Tamb+" > "+dT+" > "+qDot.ToString("00.0000")+" > "+vehicleTemperature.ToString("0000.00"));
				}
				temperatureDamage.baseDamagePerSecond = baseDamageAmount*Mathf.Max(0, 1+(vehicleTemperature-100)/100F);
				if (vehicleTemperature >= 90) {
					damageFX.OnTakeDamage(new DamageInfo{damage = 1, type = DamageType.Heat});
				}
			}
			
		}
}

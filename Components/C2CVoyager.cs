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
		[Obsolete]
		internal class C2CVoyager : MonoBehaviour {
		
			private static readonly float MIN_VOID_TIME = 10;
		
			private LiveMixin[] damageable;
			private Rigidbody body;
			
			private float voidTime = 0;
			
			void Update() {
				if (damageable == null)
					damageable = GetComponentsInChildren<LiveMixin>();
				if (!body)
					body = GetComponent<Rigidbody>();
				
				if (isInVoid())
					voidTime += Time.deltaTime;
				else
					voidTime = 0;
				
				if (voidTime >= MIN_VOID_TIME) {
					float f = (float)MathUtil.linterpolate(voidTime-MIN_VOID_TIME, 0, 30, 0, 5, true);
					f *= (float)MathUtil.linterpolate(-transform.position.y, 0, 25, 1, 0, true);
					body.AddForce(Vector3.down*Time.deltaTime*f, ForceMode.VelocityChange);
				}
			}
        	
			void Start() {
				damageable = GetComponentsInChildren<LiveMixin>();
				base.InvokeRepeating("tick", 0f, 0.5F);
			}

			private void OnKill() {
				UnityEngine.Object.Destroy(this);
			}
			
			void OnDisable() {
				base.CancelInvoke("tick");
			}

			internal void tick() {
				if (voidTime >= MIN_VOID_TIME) {
					if (damageable != null) {
						LiveMixin lv = damageable.GetRandom<LiveMixin>();
						lv.TakeDamage(Mathf.Clamp(UnityEngine.Random.Range(15F, 25F), lv.maxHealth*0.1F, lv.maxHealth*0.25F), lv.transform.position, DamageType.Cold, gameObject);
					}
				}
			}
			
			private bool isInVoid() {
				return VanillaBiomes.VOID.isInBiome(transform.position.setY(-5));
			}
			
		}
}

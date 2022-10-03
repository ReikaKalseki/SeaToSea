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
		internal class C2CTreader : MonoBehaviour {
        	
			void Start() {
				base.InvokeRepeating("tick", 0f, 1);
			}
			
			void Update() {
				
			}

			private void OnKill() {
				UnityEngine.Object.Destroy(this);
			}
			
			void OnDisable() {
				base.CancelInvoke("tick");
			}

			internal void tick() {
				Player ep = Player.main;
				if (ep) {
					float dist = Vector3.Distance(ep.transform.position, transform.position+transform.forward*12+transform.up*0);
					if (dist <= 12)  {
						int amt = Inventory.main.GetPickupCount(TechType.SeaTreaderPoop);
						if (amt > 0) {
							float df = Mathf.Clamp01(1.5F/dist);
							float chance = Mathf.Clamp(0.25F*amt, 0, 0.8F)*df;
							SNUtil.writeToChat(dist+" x "+amt+" > "+df+" > "+chance);
							if (chance > 0 && UnityEngine.Random.Range(0F, 1F) <= chance) {
								gameObject.GetComponent<SeaTreaderMeleeAttack>().OnAttackTriggerEnter(Player.main.GetComponentInChildren<Collider>());
							}
						}
					}
				}
			}
			
		}
}

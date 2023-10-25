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
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class LRNestGrass : Spawnable {
	        
	    internal LRNestGrass() : base("LRNestGrass", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject("449f060e-1f82-4efa-a5e8-c4145a851a8f");
			ObjectUtil.removeComponent<LiveMixin>(go);
			ObjectUtil.removeComponent<Collider>(go);
			ObjectUtil.removeComponent<Rigidbody>(go);
			ObjectUtil.removeComponent<BloodGrass>(go);
			go.transform.localScale = new Vector3(2, 3, 2);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
			Renderer r = go.GetComponentInChildren<MeshRenderer>();
			r.material.SetColor("_Color", new Color(0.4F, 1, 0.85F, 1));
			r.material.SetColor("_SpecColor", new Color(0.15F, 1, 0.75F, 1));
			r.material.SetVector("_ObjectUp", new Color(0.0F, 0.4F, 1F, 0));
			go.EnsureComponent<LRNestGrassTag>();
			
			return go;
	    }
		
		class LRNestGrassTag : MonoBehaviour {
			
			private ParticleSystem particles;
			
			//private float nextCheckTime = -1;
			
			private SphereCollider aoe;
			
			void Update() {
				/*
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time >= nextCheckTime) {
					if ((Player.main.transform.position-transform.position).sqrMagnitude <= UnityEngine.Random.Range(200F, 300F)) {
						particles.Play(true);
					}
					nextCheckTime = time+UnityEngine.Random.Range(0.5F, 1F);
				}*/
				
				if (!particles) {
					GameObject child = ObjectUtil.getChildObject(gameObject, "xBloodGrassSmoke");
					//SNUtil.writeToChat(child ? child.ToString() : "no fx");
					if (child) {
						particles = child.GetComponent<ParticleSystem>();
						particles.transform.localPosition = Vector3.up*0.15F;
						foreach (ParticleSystem pp in particles.GetComponentsInChildren<ParticleSystem>(true)) {
							ParticleSystem.MainModule main = pp.main;
							main.duration *= 2;
							main.startColor = new Color(0.4F, 1, 0.5F, 1);
						}
					}
				}
			}
			
			void Start() {
				aoe = gameObject.EnsureComponent<SphereCollider>();
				aoe.center = Vector3.zero;
				aoe.radius = 1;
				aoe.isTrigger = true;
				gameObject.layer = LayerID.Player;
			}
			
		    void OnTriggerEnter(Collider other) {
				if (!particles || other.isTrigger)
					return;
				LiveMixin lv = other.gameObject.FindAncestor<LiveMixin>();
				if (lv) {
					particles.Play();
					if (lv.GetComponent<Player>() || lv.GetComponent<Vehicle>()) {
						//lv.TakeDamage(10, transform.position, DamageType.Acid, gameObject);
						DamageOverTime dot = lv.gameObject.EnsureComponent<NestGrassAcid>();
						dot.doer = gameObject;
						dot.ActivateInterval(0.25F);
					}
				}
		    }
			
		}
		
		class NestGrassAcid : DamageOverTime {
			
			NestGrassAcid() : base() {
				damageType = DamageType.Acid;
				totalDamage = 30;
				duration = 10;
			}
			
		}
			
	}
}

﻿using System;
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
	internal class BrineCoralTag : MonoBehaviour, IPropulsionCannonAmmo {
		
		public static Color particleColor = new Color(0.05F, 0.33F, 0.05F);
		
		private Drillable resource;
		
		private bool isInBrine;
		
		private float timeOutOfBrine;
		
		private float lastBrineCheck = -1;
		
		private GameObject particleHolder;
		private ParticleSystem[] particles;
		
		void Update() {
			if (!resource)
				resource = GetComponent<Drillable>();
			if (!particleHolder) {
				particleHolder = ObjectUtil.getChildObject(gameObject, "dissolveFX");
				if (!particleHolder) {
					particleHolder = UnityEngine.Object.Instantiate(ObjectUtil.lookupPrefab("bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782").GetComponent<GasPod>().gasEffectPrefab);
					ObjectUtil.removeChildObject(particleHolder, "xflash");
					ObjectUtil.removeComponent<VFXUnparentAfterSeconds>(particleHolder);
					ObjectUtil.removeComponent<VFXDestroyAfterSeconds>(particleHolder);
					particleHolder.transform.SetParent(transform);
					Utils.ZeroTransform(particleHolder.transform);
				}
				particles = particleHolder.GetComponentsInChildren<ParticleSystem>();
				Renderer[] r0 = particleHolder.GetComponentsInChildren<Renderer>();
				foreach (ParticleSystem pp in particles) {
					ParticleSystem.MainModule main = pp.main;
					main.startColor = Color.white.ToAlpha(main.startColor.color.a);
					main.loop = true;
				}
				foreach (Renderer r in r0) {
					r.materials[0].SetColor("_Color", particleColor);
				}
			}
			
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastBrineCheck >= 1) {
				if (transform.position.y >= -500) {
					UnityEngine.Object.Destroy(gameObject);
					return;
				}
				isInBrine = false;
				foreach (RaycastHit hit in Physics.SphereCastAll(transform.position+Vector3.up, 2, Vector3.up, 0.1F, 1, QueryTriggerInteraction.Collide)) {
					if (hit.transform && hit.transform.GetComponent<AcidicBrineDamageTrigger>()) {
						isInBrine = true;
						break;
					}
				}
				lastBrineCheck = time;
			}
			foreach (ParticleSystem pp in particles) {
				if (isInBrine)
					pp.Stop(true, ParticleSystemStopBehavior.StopEmitting);
				else
					pp.Play();
			}
			if (isInBrine) {
				timeOutOfBrine = 0;
			}
			else {
				timeOutOfBrine += Time.deltaTime;
				if (timeOutOfBrine >= 10) { //10s grace period, and will reset this grace period if back in brine
					resource.kChanceToSpawnResources = Mathf.Max(0.2F, resource.kChanceToSpawnResources-Time.deltaTime/30F); //10s and then 30s drop, to 0 at 40s
				}
			}
		}
		
		public void onDrilled() {
			//SNUtil.writeToChat("Drilled "+gameObject.name+" @ "+transform.position+", exo="+resource.drillingExo);
			if (resource.drillingExo) { //need to manually do drops in this case for some reason
				float drops = UnityEngine.Random.Range(resource.minResourcesToSpawn, (float)resource.maxResourcesToSpawn) * resource.kChanceToSpawnResources;
				int n = (int)drops;
				if (UnityEngine.Random.Range(0F, 1F) < (drops - (int)drops))
					n++;
				Vector3 pos = resource.drillingExo.transform.position + new Vector3(0f, 0.8f, 0f);
				for (int i = 0; i < n; i++) {
					Pickupable pp = ObjectUtil.createWorldObject(C2CItems.brineCoralPiece.ClassID).GetComponent<Pickupable>();
					pp.transform.position = Vector3.Lerp(gameObject.transform.position, pos, Time.deltaTime * 5f);
					if (!resource.drillingExo.storageContainer.container.HasRoomFor(pp)) {
						if (Player.main.GetVehicle() == resource.drillingExo) {
							ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
						}
					}
					else {
						string arg = Language.main.Get(pp.GetTechName());
						ErrorMessage.AddMessage(Language.main.GetFormat<string>("VehicleAddedToStorage", arg));
						uGUI_IconNotifier.main.Play(pp.GetTechType(), uGUI_IconNotifier.AnimationType.From, null);
						pp = pp.Initialize();
						InventoryItem item = new InventoryItem(pp);
						resource.drillingExo.storageContainer.container.UnsafeAdd(item);
						pp.PlayPickupSound();
					}
				}
			}
			GetComponent<ResourceTracker>().Unregister();
			UnityEngine.Object.Destroy(gameObject, 0.5F);
		}
		
		public void OnGrab() {
			GetComponent<ResourceTracker>().Unregister();
		}
	
		public void OnShoot() {
			
		}
	
		public void OnRelease() {
			if (resource.health[0] > 50) {
				ResourceTracker rt = GetComponent<ResourceTracker>();
				rt.Register();
				rt.StartUpdatePosition();
			}
		}
	
		public void OnImpact() {
			
		}
	
		public bool GetAllowedToGrab() {
			return true;
		}
	
		public bool GetAllowedToShoot() {
			return false;
		}
		
	}
}

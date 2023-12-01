using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{
	internal class SeamothTetherController : MonoBehaviour {
		
		private static readonly float POWER_COST = 0.5F; //per second
		internal static readonly float RANGE = 25F;
		
		private SeaMoth vehicle;
		
		private int moduleSlot;
		
		internal GameObject tetherRoot;
		private FMOD_CustomLoopingEmitter grabSound;
		
		private List<Tether> tethers = new List<Tether>();
		
		private bool towing;
		
		void FixedUpdate() {
			if (!vehicle) {
				vehicle = GetComponent<SeaMoth>();
			}
			
			if (!tetherRoot) {
				tetherRoot = ObjectUtil.getChildObject(gameObject, "TetherRoot");
				if (!tetherRoot) {
					tetherRoot = new GameObject("TetherRoot");
					tetherRoot.transform.SetParent(transform);
					tetherRoot.transform.localRotation = Quaternion.identity;
					tetherRoot.transform.localPosition = Vector3.zero;
					
					PropulsionCannon template = ObjectUtil.lookupPrefab(TechType.PropulsionCannon).GetComponent<PropulsionCannon>();
					grabSound = tetherRoot.EnsureComponent<FMOD_CustomLoopingEmitter>();
					grabSound.copyObject(template.grabbingSound);
					tetherRoot.name = "TetherRoot";
				}
			}
			
			while (tethers.Count < 5) {
				tethers.Add(new Tether());
			}

			if (vehicle && moduleSlot >= 0 && vehicle.IsToggled(moduleSlot)) {
				//SNUtil.writeToChat("Module is toggled");
				float energy;
				float capacity;
				vehicle.GetEnergyValues(out energy, out capacity);
				float dT = Time.deltaTime;
				float amt = POWER_COST*dT;
				if (energy <= amt) {
					vehicle.ToggleSlot(moduleSlot, false);
					return;
				}
				vehicle.ConsumeEnergy(amt);
				HashSet<Rigidbody> grabbed = new HashSet<Rigidbody>();
				foreach (Tether t in tethers) {
					Rigidbody rb = t.getTarget();
					if (rb) {
						t.pull(this, dT);
						grabbed.Add(rb);
					}
					else {
						rb = tryFindGrabbableTarget();
						if (rb && !grabbed.Contains(rb)) {
							t.grab(rb);
							grabbed.Add(rb);
							//SNUtil.writeToChat(tethers.IndexOf(t)+" @ "+DayNightCycle.main.timePassedAsFloat.ToString("0.00000")+": Grabbed "+rb+" from list "+grabbed.toDebugString());
						}
					}
				}
				towing = grabbed.Count > 0;
			}
			else {
				towing = false;
				//SNUtil.writeToChat("Module is NOT toggled");
				foreach (Tether t in tethers)
					t.drop();
			}
			if (grabSound) {
				if (towing)
					grabSound.Play();
				else
					grabSound.Stop();
			}
		}
		
		public bool isTowing() {
			return towing;
		}
		
		private Rigidbody tryFindGrabbableTarget() {
			List<Rigidbody> available = new List<Rigidbody>();
			WorldUtil.getGameObjectsNear(transform.position, RANGE, go => {
				Rigidbody rb = go.GetComponent<Rigidbody>();
				if (rb && !rb.GetComponent<Vehicle>()) {
					Drillable d = go.GetComponent<Drillable>();
					if (d) {
						available.Add(rb);
						return;
					}/*
					EcoTarget e = go.GetComponent<EcoTarget>();
					if (e && e.GetTargetType() == EcoTargetType.Shark) {
						available.Add(rb);
						return;
					}*/
					Creature c = go.GetComponent<Creature>();
					if (c is GasoPod || c is Shocker || c is LavaLizard || c is Stalker || c is BoneShark || c is SpineEel || c is SandShark || c is Warper) {
						available.Add(rb);
						return;
					}
				}
			});
			return available.Count == 0 ? null : available.GetRandom();
		}
		
		public void recalculateModule() {
			if (!vehicle) {
				Invoke("recalculateModule", 0.5F);
				return;
			}
			foreach (int idx in vehicle.slotIndexes.Values) {
				InventoryItem ii = vehicle.GetSlotItem(idx);
				if (ii != null && ii.item && ii.item.GetTechType() == C2CItems.tetherModule.TechType) {
					moduleSlot = idx;
					return;
				}
			}
			moduleSlot = -1;
		}
		
	}
	
	class Tether {
		
		private Rigidbody target;
		
		private readonly List<GameObject> grabSphereFX = new List<GameObject>();
		private readonly float massScale;
		
		private VFXElectricLine effect;
		
		public Tether() {
			PropulsionCannon template = ObjectUtil.lookupPrefab(TechType.PropulsionCannon).GetComponent<PropulsionCannon>();
			for (int i = 0; i < 4; i++) {
				GameObject go = UnityEngine.Object.Instantiate(template.grabbedEffect);
				go.SetActive(false);
				grabSphereFX.Add(go);
				go.transform.localRotation = UnityEngine.Random.rotationUniform;
			}
			massScale = template.massScalingFactor;
		}
		
		internal Rigidbody getTarget() {
			return target;
		}
		
		internal void grab(Rigidbody rb) {
			target = rb;
			foreach (GameObject go in grabSphereFX)
				go.SetActive(true);
			effect.gameObject.SetActive(true);
			rb.isKinematic = false;
		}
		
		internal void drop() {
			foreach (GameObject go in grabSphereFX) {
				if (go)
					go.SetActive(false);
			}
			if (effect)
				effect.gameObject.SetActive(false);
			target = null;
		}
		
		internal void pull(SeamothTetherController mgr, float dT) {			
			if (!effect) {
				GameObject go = ObjectUtil.lookupPrefab("d11dfcc3-bce7-4870-a112-65a5dab5141b");
				go = go.GetComponent<Gravsphere>().vfxPrefab;
				go = UnityEngine.Object.Instantiate(go);
				effect = go.GetComponent<VFXElectricLine>();
				effect.transform.parent = mgr.tetherRoot.transform;
			}
			
			if (!target)
				return;
			
			bool animal = (bool)target.GetComponent<Creature>();
		
			Vector3 tgtFX = mgr.transform.position-mgr.transform.forward*1-mgr.transform.up*0.25F;
			float f = animal ? 2 : 1;
			Vector3 tgt = mgr.transform.position-mgr.transform.forward*5*f-mgr.transform.up*2*f;
			effect.origin = tgtFX;
			effect.target = target.transform.position;
			effect.originVector = -mgr.transform.forward;
			
			Vector3 distance = tgt - target.transform.position;
			float magnitude = distance.magnitude;
			if (magnitude > SeamothTetherController.RANGE) {
				drop();
				return;
			}
			float d = Mathf.Clamp(magnitude, 1f, 4f);
			Vector3 vector = target.velocity + Vector3.Normalize(distance) * (animal ? 300 : 800) * d * dT / (1f + target.mass * massScale);
			Vector3 amount = vector * (10f + Mathf.Pow(Mathf.Clamp01(1f - magnitude), 1.75f) * 40f) * dT;
			vector = UWE.Utils.SlerpVector(vector, Vector3.zero, amount);
			target.velocity = vector;
			
			foreach (GameObject go in grabSphereFX) {
				go.transform.SetParent(mgr.tetherRoot.transform);
				go.transform.localScale = Vector3.one*(animal ? 5 : 3);
				go.transform.position = target.transform.position;
			}
		}
		
		private class GrabbedTarget {
			
			private Rigidbody body;
			private Vector3 relativeCenter;
			
		}
		
	}
}

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
		private static readonly float RANGE = 20F;
		
		private SeaMoth vehicle;
		
		private int moduleSlot;
		
		private GameObject grabberRoot;
		private PropulsionCannon[] grabbers = null;
		
		void FixedUpdate() {
			if (!vehicle) {
				vehicle = GetComponent<SeaMoth>();
			}
			
			if (!grabberRoot) {
				grabberRoot = ObjectUtil.getChildObject(gameObject, "GrabberRoot");
				if (!grabberRoot) {
					grabberRoot = new GameObject("GrabberRoot");
					grabberRoot.transform.SetParent(transform);
					grabberRoot.transform.localRotation = Quaternion.identity;
					grabberRoot.transform.localPosition = Vector3.forward*-2;
				}
				grabbers = grabberRoot.GetComponents<PropulsionCannon>();
				if (grabbers.Length < 6) {
					for (int i = grabbers.Length; i < 6; i++) {
						grabberRoot.AddComponent<PropulsionCannon>();
					}
					grabbers = grabberRoot.GetComponents<PropulsionCannon>();
				}
				PropulsionCannon template = ObjectUtil.lookupPrefab(TechType.PropulsionCannon).GetComponent<PropulsionCannon>();
				foreach (PropulsionCannon prop in grabbers) {
					prop.copyObject(UnityEngine.Object.Instantiate(template));
					prop.attractionForce = 400;
					prop.pickupDistance = RANGE;
					prop.maxAABBVolume = 999999;
					prop.maxMass = 999999;
					prop.energyInterface = vehicle.energyInterface;
					prop.muzzle = grabberRoot.transform;
				}
				grabberRoot.name = "GrabberRoot";
			}
			
			if (grabbers == null)
				return;

			if (vehicle && moduleSlot >= 0 && vehicle.IsToggled(moduleSlot)) {
				//SNUtil.writeToChat("Module is toggled");
				float energy;
				float capacity;
				vehicle.GetEnergyValues(out energy, out capacity);
				float amt = POWER_COST*Time.deltaTime;
				if (energy <= amt) {
					vehicle.ToggleSlot(moduleSlot, false);
					return;
				}
				vehicle.ConsumeEnergy(amt);
				HashSet<GameObject> grabbed = new HashSet<GameObject>();
				foreach (PropulsionCannon prop in grabbers) {
					GameObject go = prop.grabbedObject;
					if (go) {
						prop.targetPosition = transform.position+transform.forward*3;
						if (go == gameObject || grabbed.Contains(go)) {
							prop.ReleaseGrabbedObject();
							continue;
						}
						grabbed.Add(go);
					}
					else {
						go = tryFindGrabbableTarget();
						if (go && !grabbed.Contains(go))
							prop.GrabObject(go);
					}
				}
			}
			else {
				//SNUtil.writeToChat("Module is NOT toggled");
				foreach (PropulsionCannon prop in grabbers)
					prop.ReleaseGrabbedObject();
			}
		}
		
		private GameObject tryFindGrabbableTarget() {
			List<GameObject> available = new List<GameObject>();
			WorldUtil.getGameObjectsNear(transform.position, RANGE, go => {
				Rigidbody rb = go.GetComponent<Rigidbody>();
				if (rb) {
					Drillable d = go.GetComponent<Drillable>();
					if (d) {
						available.Add(go);
						return;
					}
					EcoTarget c = go.GetComponent<EcoTarget>();
					if (c && c.GetTargetType() == EcoTargetType.Shark) {
						available.Add(go);
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
}

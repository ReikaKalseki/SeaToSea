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
		internal class UnpickablePlant : MonoBehaviour {
        	
			void Start() {
				foreach (Pickupable p in gameObject.GetComponentsInChildren<Pickupable>()) {
					UnityEngine.Object.Destroy(p);
				}
				foreach (PickPrefab p in gameObject.GetComponentsInChildren<PickPrefab>()) {
					UnityEngine.Object.Destroy(p);
				}
				foreach (LiveMixin p in gameObject.GetComponentsInChildren<LiveMixin>()) {
					UnityEngine.Object.Destroy(p);
				}
			}
			
			void Update() {
				
			}
			
		}
}

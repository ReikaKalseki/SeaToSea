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
	
	public class MercuryLootSpawner : Spawnable {
	        
		internal MercuryLootSpawner() : base("MercuryLootSpawner", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = new GameObject();
			go.EnsureComponent<MercuryLootSpawnerTag>();
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			return go;
	    }
		
		class MercuryLootSpawnerTag : MonoBehaviour {
			
			private int spawned;
			
			void Update() {
				Vector3 vec = UnityEngine.Random.rotationUniform.eulerAngles.normalized;
				Ray ray = new Ray(transform.position, vec);
				if (UWE.Utils.RaycastIntoSharedBuffer(ray, 32, Voxeland.GetTerrainLayerMask()) > 0) {
					RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
					if (hit.transform != null) {
						foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(hit.point, transform.localScale.x)) {
							if (pi.ClassId == VanillaResources.MERCURY.prefab)
								return;
						}
						GameObject go = ObjectUtil.createWorldObject(VanillaResources.MERCURY.prefab);
						go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
						go.transform.position = hit.point;
						spawned++;
					}
				}
				if (spawned >= transform.localScale.y)
					UnityEngine.Object.DestroyImmediate(gameObject);
			}
			
		}
			
	}
}

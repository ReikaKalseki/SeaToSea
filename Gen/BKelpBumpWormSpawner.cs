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
	
	public class BKelpBumpWormSpawner : WorldGenerator {
	        
		static BKelpBumpWormSpawner() {
			
	    }
	        
	    public BKelpBumpWormSpawner(Vector3 pos) : base(pos) {
			
	    }
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
			
	    public override void generate(List<GameObject> li) {
			float r = 9;
			foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(position, r)) {
				if (pi.ClassId == "26ce64dd-e703-470d-a0e4-acd43841bdd8" || pi.ClassId == "53e89f85-44a6-4ccf-9790-efae4b5fcae9" || pi.ClassId == "2dd42944-a73f-4443-ba90-bf45956e72f0" || VanillaFlora.DEEP_MUSHROOM.includes(pi.ClassId)) {
					UnityEngine.Object.Destroy(pi.gameObject);
				}
			}
			for (int i = 0; i < 40; i++) {
				Vector3 pos = MathUtil.getRandomVectorAround(position, r);
				if (pos.y < position.y) {
					i--;
					continue;
				}
				Vector3 vec = position-pos;
				Ray ray = new Ray(pos, vec);
				if (UWE.Utils.RaycastIntoSharedBuffer(ray, vec.magnitude, Voxeland.GetTerrainLayerMask()) > 0) {
					RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
					if (hit.transform != null) {
						GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.bkelpBumpWorm.ClassID);
						go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
						go.transform.position = hit.point;
						go.transform.RotateAroundLocal(go.transform.up, UnityEngine.Random.Range(0F, 360F));
						li.Add(go);
					}
				}
			}
			for (int i = 0; i < 1; i++) {
				GameObject grub = ObjectUtil.createWorldObject(C2CItems.broodmother.ClassID);
				grub.transform.rotation = Quaternion.identity;
				grub.transform.position = MathUtil.getRandomVectorAround(position+Vector3.up*6, 3);
				li.Add(grub);
			}
	    }
			
	}
}

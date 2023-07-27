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
	
	public class SanctuaryGrassSpawner : Spawnable {
	        
		internal SanctuaryGrassSpawner() : base("SanctuaryGrassSpawner", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = new GameObject();
			go.EnsureComponent<CrashZoneSanctuaryGrassSpawnerTag>();
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			return go;
	    }
		
		class CrashZoneSanctuaryGrassSpawnerTag : MonoBehaviour {
			
			private float age;
			
			void Update() {
				if (Vector3.Distance(Player.main.transform.position, transform.position) < 100)
					age += Time.deltaTime;
				if (age < 2)
					return;
				RaycastHit? at = WorldUtil.getTerrainVectorAt(transform.position, 60);
				if (!at.HasValue) {
					age = 0;
					return;
				}
				List<RaycastHit> li = WorldUtil.getTerrainMountedPositionsAround(at.Value.point, 30F, 200);
				foreach (RaycastHit hit in li) {
					if (Vector3.Angle(hit.normal, Vector3.up) >= 30)
						continue;
					GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.crashSanctuaryGrass.ClassID);
					go.transform.position = hit.point;
					go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
					go.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0F, 360F), 0), Space.Self);	
					go.transform.position = go.transform.position+go.transform.up*-0.25F;
				}
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			
		}
			
	}
}

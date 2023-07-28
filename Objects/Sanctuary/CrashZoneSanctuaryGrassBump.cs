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
	
	public class CrashZoneSanctuaryGrassBump : Spawnable {
	        
	    internal CrashZoneSanctuaryGrassBump() : base("CrashZoneSanctuaryGrassBump", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			string pfb = VanillaFlora.MUSHROOM_BUMP.getRandomPrefab(false);
			GameObject go = ObjectUtil.createWorldObject(pfb);
			ObjectUtil.removeComponent<LiveMixin>(go);
			ObjectUtil.removeComponent<Collider>(go);
			ObjectUtil.removeComponent<PlantBehaviour>(go);
			ObjectUtil.removeComponent<FMOD_StudioEventEmitter>(go);
			ObjectUtil.removeComponent<CoralBlendWhite>(go);
			ObjectUtil.removeComponent<Light>(go);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/SanctuaryGrassBump");
				r.material.DisableKeyword("MARMO_EMISSION");
				r.receiveShadows = false;
				r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				if (pfb == "5086a02a-ea6d-41ba-90c3-ea74d97cf6b5") {
					r.transform.localRotation = Quaternion.Euler(-90, 0, 0);
					r.transform.localScale = new Vector3(1, 0.25F, 1);
				}
				else if (pfb != "f3de21af-550b-4901-a6e8-e45e31c1509d") {
					r.transform.localScale = new Vector3(1, 0.5F, 1);
				}
			}
			return go;
	    }
			
	}
}

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
	
	public class CrashZoneSanctuaryCoralSheet : Spawnable {
	        
	    internal CrashZoneSanctuaryCoralSheet() : base("CrashZoneSanctuaryCoralSheet", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject("");
			ObjectUtil.removeComponent<LiveMixin>(go);
			ObjectUtil.removeComponent<Collider>(go);
			ObjectUtil.removeComponent<PlantBehaviour>(go);
			ObjectUtil.removeComponent<FMOD_StudioEventEmitter>(go);
			ObjectUtil.removeComponent<CoralBlendWhite>(go);
			ObjectUtil.removeComponent<Light>(go);
			Renderer r = go.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/SanctuaryCoral");
			r.material.DisableKeyword("MARMO_EMISSION");
			r.receiveShadows = false;
			r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			go.transform.localScale = new Vector3(10, 1, 10);
			return go;
	    }
			
	}
}

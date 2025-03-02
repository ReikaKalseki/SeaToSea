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
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea {
    
	internal class LavaCastleVentCrystalPlacer : Spawnable {
    	
		internal LavaCastleVentCrystalPlacer() : base("LavaCastleVentCrystalPlacer", "", "") {
    		
		}
    	
		public override GameObject GetGameObject() {
			GameObject go = new GameObject("LavaCastleVentCrystalPlacer");
			go.EnsureComponent<LavaCastleVentCrystalConverter>();
			return go;
		}
    	
	}
	
	internal class LavaCastleVentCrystalConverter : MonoBehaviour {
		
		void Update() {
			if ((transform.position - Player.main.transform.position).sqrMagnitude <= 40000) {
				float ch = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 0.15F : 0.25F;
				if (UnityEngine.Random.Range(0F, 1F) < ch) {
					GameObject azur = ObjectUtil.createWorldObject(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).ClassID);
					azur.transform.rotation = transform.rotation;
					azur.transform.position = transform.position;
					azur.SetActive(true);
				}
				UnityEngine.Object.Destroy(gameObject);
			}
		}
			
	}
}

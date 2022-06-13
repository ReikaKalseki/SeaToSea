using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class AlkaliPlant : BasicCustomPlant {
		
		public AlkaliPlant() : base(SeaToSeaMod.itemLocale.getEntry("ALKALI_PLANT"), VanillaFlora.REDWORT) {
			glowIntensity = 2;
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<AlkaliPlantTag>();
			go.transform.localScale = Vector3.one*2;
		}
		
	}
	
	class AlkaliPlantTag : MonoBehaviour {
		
		void Start() {
    		if (gameObject.transform.position.y > -10)
    			UnityEngine.Object.Destroy(gameObject);
    		else
    			gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(2, 2.5F);
		}
		
		void Update() {
			
		}
		
	}
}

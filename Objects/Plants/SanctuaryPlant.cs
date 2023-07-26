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
	
	public class SanctuaryPlant : BasicCustomPlant {
		
		public SanctuaryPlant() : base(SeaToSeaMod.itemLocale.getEntry("SANCTUARY_PLANT"), new FloraPrefabFetch("99bbd145-d50e-4afb-bff0-27b33243642b"), "ce20c267-b52b-4866-8134-f3f78072af3e", "Core") {
			glowIntensity = 1F;
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(2, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<SanctuaryPlantTag>();
			RenderUtil.setEmissivity(go.GetComponentInChildren<Renderer>(), 2);
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}
		
		public override Plantable.PlantSize getSize() {
			return Plantable.PlantSize.Large;
		}
		
	}
	
	class SanctuaryPlantTag : MonoBehaviour {
		
		private bool isGrown;
		
		private Renderer mainRender;
		private Light light;
		
		void Start() {
			isGrown = gameObject.GetComponent<GrownPlant>() != null;
    		//if (gameObject.transform.position.y > -10)
    		//	UnityEngine.Object.Destroy(gameObject);
    		if (isGrown) {
    			gameObject.SetActive(true);
    			gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.8F, 1.2F);
    		}
    		else {
    			
    		}gameObject.transform.localScale = Vector3.one*3;
		}
		
		void Update() {
			if (!light)
				light = GetComponentInChildren<Light>();
			if (mainRender == null)
				mainRender = GetComponentInChildren<Renderer>();
			light.transform.localPosition = Vector3.up*0.91F;
			light.intensity = 1.6F;
			light.range = 24;
			light.color = new Color(26/255F, 231/255F, 220/255F, 1);
		}
		
	}
}

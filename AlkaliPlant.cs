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
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(2, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<AlkaliPlantTag>();
			go.transform.localScale = Vector3.one*2;
			Plantable p = go.EnsureComponent<Plantable>();
			p.aboveWater = false;
			p.underwater = true;
			p.isSeedling = true;
			p.plantTechType = TechType;
			p.size = Plantable.PlantSize.Large;
			p.pickupable = go.GetComponentInChildren<Pickupable>();
			p.model = go.transform.Find("coral_reef_plant_middle_05").gameObject;
			CapsuleCollider cu = go.GetComponentInChildren<CapsuleCollider>();
			if (cu != null) {
				CapsuleCollider cc = p.model.AddComponent<CapsuleCollider>();
				cc.radius = cu.radius*0.8F;
				cc.center = cu.center;
				cc.direction = cu.direction;
				cc.height = cu.height;
				cc.material = cu.material;
				cc.name = cu.name;
			}
			p.modelEulerAngles = new Vector3(270, UnityEngine.Random.Range(0, 360F), 0);
			p.modelScale = Vector3.one*0.8F;
			p.modelIndoorScale = Vector3.one*0.5F;
			r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
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

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
	
	public class HealingFlower : BasicCustomPlant {
		
		public HealingFlower() : base(SeaToSeaMod.itemLocale.getEntry("HEALING_FLOWER"), new FloraPrefabFetch(VanillaFlora.VOXEL), "6f932b93-65e8-4c89-a63b-d105203ab84c", "Leaves") {
			glowIntensity = 1.5F;
			finalCutBonus = 1;
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(1, 1);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<HealingFlowerTag>();
			//SphereCollider sc = go.EnsureComponent<SphereCollider>();
			//sc.radius = 0.25F;
			//sc.isTrigger = true;
			go.GetComponentInChildren<Collider>().gameObject.EnsureComponent<HealingFlowerColliderTag>();
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}
		
		public override Plantable.PlantSize getSize() {
			return Plantable.PlantSize.Small;
		}
		/*
		public override float getGrowthTime() {
			return 6000; //5x
		}*/
		
	}
	
	class HealingFlowerTag : MonoBehaviour {
		
		private bool isGrown;
		
		void Start() {
			isGrown = gameObject.GetComponent<GrownPlant>() != null;
    		//if (gameObject.transform.position.y > -10)
    		//	UnityEngine.Object.Destroy(gameObject);
    		if (isGrown) {
    			gameObject.SetActive(true);
    			//gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.8F, 1.2F);
    		}
    		else {
    			gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(1.33F, 1.67F);
    		}
		}
		
		void Update() {
			
		}
		
	}
	
	class HealingFlowerColliderTag : MonoBehaviour {
		
		private bool isGrown;
		private LiveMixin live;
		
		void Start() {
			isGrown = gameObject.FindAncestor<GrownPlant>() != null;
    		live = gameObject.FindAncestor<LiveMixin>();
		}
		
	    void OnTriggerStay(Collider other) {
			if (!other.isTrigger && other.gameObject.FindAncestor<Player>()) {
				float dt = Time.deltaTime;
				if (other.gameObject.FindAncestor<LiveMixin>().AddHealth((isGrown ? 0.33F : 0.75F)*dt) > 0.0001F) {
					if (isGrown && live != null)
						live.TakeDamage(0.67F*dt, transform.position);
				}
			}
	    }
		
	}
}

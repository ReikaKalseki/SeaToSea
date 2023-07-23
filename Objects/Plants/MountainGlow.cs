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
	
	public class MountainGlow : BasicCustomPlant, MultiTexturePrefab<FloraPrefabFetch> {
		
		public MountainGlow() : base(SeaToSeaMod.itemLocale.getEntry("MOUNTAIN_GLOW"), new FloraPrefabFetch("1d5877a7-bc56-46c8-a27c-f9d0ab99cc80"), "ba866b79-1db1-4689-a697-b7d2bc65959d", "Pods") {
			glowIntensity = 3F;
			collectionMethod = HarvestType.None;
		}

		protected override bool isExploitable() {
			return true;
		}
		
		protected override bool generateSeed() {
			return true;
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(2, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<MountainGlowTag>();
			SphereCollider c = go.EnsureComponent<SphereCollider>();
			c.isTrigger = true;
			c.radius = 4F;
			c.center = Vector3.zero;
			go.EnsureComponent<FruitPlant>();
			Light l = ObjectUtil.addLight(go);
			l.color = new Color(1, 0.1F, 0.2F);
			l.intensity = 1.6F;
			l.range = 4;
			l.transform.localPosition = new Vector3(0, 1, 0);
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}
		
		public override Plantable.PlantSize getSize() {
			return Plantable.PlantSize.Large;
		}
		
		public Dictionary<int, string> getTextureLayers(Renderer r) {
			return new Dictionary<int, string>{{0, ""}, {1, ""}, {2, ""}, {3, ""}};
		}
		
	}
	
	class MountainGlowTag : MonoBehaviour {
		
		private static float lastDamageTime; //static so global, so does not stack lag OR damage
		
		private bool isGrown;
		
		private FruitPlant fruiter;
		private GameObject fruitHolder;
		private PickPrefab[] seeds;
		private Renderer[] renders;
		private Light light;
		private SphereCollider aoe;
		
		void Start() {
			isGrown = gameObject.GetComponent<GrownPlant>() != null;
    		//if (gameObject.transform.position.y > -10)
    		//	UnityEngine.Object.Destroy(gameObject);
    		if (isGrown) {
    			gameObject.SetActive(true);
    			gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.8F, 1.2F);
    			foreach (PickPrefab pp in seeds) {
    				pp.SetPickedUp();
    			}
    		}
    		else {
    			
    		}
		}
		
		void Update() {
			if (!aoe)
				aoe = GetComponent<SphereCollider>();
			if (!fruiter)
				fruiter = GetComponent<FruitPlant>();
			if (!light)
				light = GetComponentInChildren<Light>();
			if (renders == null)
				renders = GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renders)
				r.transform.localPosition = Vector3.down*0.5F;
			if (!fruitHolder) {
				GameObject go = ObjectUtil.lookupPrefab("a17ef178-6952-4a91-8f66-44e1d8ca0575");
				fruitHolder = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(go, "fruit_LODs"));
				fruitHolder.transform.SetParent(transform);
				fruitHolder.transform.localPosition = new Vector3(-0.08F, 6.38F, 0.06F);
				fruitHolder.transform.localScale = Vector3.one*0.3F;
				fruitHolder.transform.localRotation = Quaternion.Euler(0, 0, 180);
				ObjectUtil.removeComponent<ChildObjectIdentifier>(fruitHolder);
				ObjectUtil.removeComponent<TechTag>(fruitHolder);
				seeds = fruitHolder.GetComponentsInChildren<PickPrefab>();
				foreach (PickPrefab pp in seeds) {
					pp.pickTech = C2CItems.mountainGlow.seed.TechType;
			    	pp.pickedEvent.AddHandler(pp.gameObject, new UWE.Event<PickPrefab>.HandleFunction(p => {
						fruiter.inactiveFruits.Add(pp);
			    	}));
					if (isGrown)
						pp.SetPickedUp();
					Renderer r = pp.GetComponentInChildren<Renderer>();
					RenderUtil.setEmissivity(r, 1.5F);
	    			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Plants/MountainGlowSeed");
				}
				fruiter.fruits = seeds;
				fruiter.fruitSpawnEnabled = true;
				fruiter.fruitSpawnInterval = 300;
			}
			light.intensity = Mathf.Lerp(1.4F, 2.2F, 1F-DayNightCycle.main.GetLightScalar())*(1F-(fruiter.inactiveFruits.Count/(float)seeds.Length));
			aoe.isTrigger = true;
			aoe.radius = 4F;
			aoe.center = Vector3.zero;
		}
		
	    void OnTriggerStay(Collider other) {
			if (DayNightCycle.main.timePassedAsFloat-lastDamageTime >= 0.05F && !other.isTrigger && other.gameObject.FindAncestor<Player>()) {
				other.gameObject.FindAncestor<LiveMixin>().TakeDamage(Time.deltaTime*1.5F, transform.position, DamageType.Heat);
				lastDamageTime = DayNightCycle.main.timePassedAsFloat;
			}
	    }
		
	}
}

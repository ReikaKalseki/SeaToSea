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
		
		public AlkaliPlant() : base(SeaToSeaMod.itemLocale.getEntry("ALKALI_PLANT"), VanillaFlora.REDWORT, "daff0e31-dd08-4219-8793-39547fdb745e", "Samples") {
			glowIntensity = 2;
			finalCutBonus = 0;
			//seed.sprite = TextureManager.getSprite("Textures/Items/"+ObjectUtil.formatFileName(this));
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(2, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<AlkaliPlantTag>();
			go.transform.localScale = Vector3.one*2;
			/*
			GameObject seedRef = ObjectUtil.lookupPrefab("daff0e31-dd08-4219-8793-39547fdb745e").GetComponent<Plantable>().model;
			p.pickupable = go.GetComponentInChildren<Pickupable>();
			p.model = UnityEngine.Object.Instantiate(seedRef);
			GrowingPlant grow = p.model.EnsureComponent<GrowingPlant>();
			grow.seed = p;
			RenderUtil.setModel(p.model, "coral_reef_plant_middle_05", ObjectUtil.getChildObject(go, "coral_reef_plant_middle_05"));
			*//*
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
			p.modelEulerAngles = new Vector3(270*0, UnityEngine.Random.Range(0, 360F), 0);*/
			go.EnsureComponent<LiveMixin>().data.maxHealth /= 2;
			foreach (Renderer rr in r) {
				foreach (Material m in rr.materials) {
					m.SetColor("_GlowColor", new Color(1, 1, 1, 1));
					m.SetVector("_Scale", new Vector4(0.35F, 0.2F, 0.1F, 0.0F));
					m.SetVector("_Frequency", new Vector4(1.2F, 0.5F, 1.5F, 0.5F));
					m.SetVector("_Speed", new Vector4(0.2F, 0.5F, 1.5F, 0.5F));
					m.SetVector("_ObjectUp", new Vector4(1F, 1F, 1F, 1F));
					m.SetFloat("_WaveUpMin", 0F);
				}
			}
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}
		
	}
	
	class AlkaliPlantTag : MonoBehaviour {
		
		private Renderer renderer;
		
		private bool isGrown;
		private float rootScale;
		
		private float timeVisible = 0;
		private float currentScale = 1;
		private bool currentlyHiding = false;
		
		void Start() {
			isGrown = gameObject.GetComponent<GrownPlant>() != null;
			currentScale = 1;
			rootScale = UnityEngine.Random.Range(2, 2.5F);
    		if (gameObject.transform.position.y > -10)
    			UnityEngine.Object.Destroy(gameObject);
    		else if (isGrown) {
    			gameObject.SetActive(true);
    			gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.8F, 1.2F);
    		}
    		else {
    			gameObject.transform.localScale = Vector3.one*rootScale;
    		}
		}
		
		void Update() {
			if (!renderer)
				renderer = GetComponentInChildren<Renderer>();
			Player ep = Player.main;
			if (ep && !isGrown) {
				float dT = Time.deltaTime;
				float dd = Vector3.Distance(ep.transform.position, transform.position);
				if (dd <= 15F && canSeePlayer(ep))
					timeVisible += dT;
				else
					timeVisible = 0;
				currentlyHiding = timeVisible >= 0.67F;
				if (currentlyHiding) {
					float sp = 1F*dT;
					if (dd <= 8)
						sp *= 1.5F;
					currentScale = Mathf.Max(0.03F, currentScale-sp);
				}
				else {
					currentScale = Mathf.Min(1, currentScale+0.15F*dT);
				}
				if (float.IsInfinity(currentScale) || float.IsNaN(currentScale)) //how this happens is beyond me
					currentScale = 1;
				currentScale = Mathf.Clamp(currentScale, 0.03F, 1);
				float f = rootScale*currentScale;
				float glow = SeaToSeaMod.alkali.glowIntensity*currentScale;
				if (glow <= 0.035)
					glow = 0;
				RenderUtil.setEmissivity(renderer, glow, "GlowStrength");
				transform.localScale = new Vector3(0.33F+f*0.67F, f, 0.33F+f*0.67F);//Vector3.one*f;//new Vector3(0.75F+f*0.25F, f, 0.75F+f*0.25F);
				GetComponent<LiveMixin>().data.knifeable = isHarvestable();
			}
		}
		
		private bool canSeePlayer(Player ep) {
			if (ep.IsInBase())
				return false;
			Vehicle v = ep.GetVehicle();
			if (v)
				return Vector3.Distance(v.transform.position, transform.position) <= 4 || (v.useRigidbody && v.GetComponent<Rigidbody>().velocity.magnitude > 0.1);
			Vector3 pos1 = ep.transform.position;
			Vector3 pos2 = transform.position+transform.up.normalized*0.5F;
			if (WorldUtil.lineOfSight(ep.gameObject, gameObject, pos1, pos2))
				return true;
			pos2 = transform.position+transform.up.normalized*1.5F;
			if (WorldUtil.lineOfSight(ep.gameObject, gameObject, pos1, pos2))
				return true;
			return false;
		}
		
		public bool isHarvestable() {
			return currentScale >= 0.75F;
		}
		
	}
}

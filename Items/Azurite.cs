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
	
	public class Azurite : BasicCustomOre {
		
		internal static readonly float BASE_LIGHT_RANGE = 2.5F;
		
		public Azurite(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<AzuriteTag>();
			
			Light l = ObjectUtil.addLight(go);
			l.type = LightType.Point;
			l.color = new Color(0F, 0.65F, 1F);
			l.intensity = 0.9F;
			l.range = BASE_LIGHT_RANGE;
		}
		
	}
	
	class AzuriteTag : MonoBehaviour {
		
		private float lastTime;
		
		private Renderer render;
		private Light light;
		
		void Start() {
    		render = gameObject.GetComponentInChildren<Renderer>();
    		light = gameObject.GetComponentInChildren<Light>();
		}
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = Time.deltaTime;
			double phase = gameObject.GetHashCode();
			double sp = 1+0.4*Math.Cos((0.02*gameObject.transform.position.magnitude)%(600*Math.PI)); //was 0.75 and 0.25
			double tick = (sp*time+phase)%(200*Math.PI);
			float lt = (float)Math.Sin(tick)+0.4F*(float)Math.Sin(tick*4.63-289.2);
			float f = CustomMaterials.getMaterial(CustomMaterials.Materials.VENT_CRYSTAL).glow-1.5F+2F*lt;
			RenderUtil.setEmissivity(render, f, "GlowStrength");
			light.range = Azurite.BASE_LIGHT_RANGE+0.5F*f;
			if (dT > 0 && Player.main != null && !Player.main.IsInsideWalkable() && Player.main.IsSwimming()) {
	   			InventoryItem suit = Inventory.main.equipment.GetItemInSlot("Body");
	   			if (suit == null || (suit.item.GetTechType() != C2CItems.sealSuit.TechType && suit.item.GetTechType() != TechType.ReinforcedDiveSuit)) {
					GameObject ep = Player.main.gameObject;
					float distsq = (ep.transform.position-gameObject.transform.position).sqrMagnitude;
					if (distsq < 144) {
						float amt = 2.5F*dT*Mathf.Min(1, 1-distsq/144F);
						//SNUtil.writeToChat(distsq+" & "+dT+" > "+amt);
						//SNUtil.log(distsq+" & "+dT+" > "+amt);
						ep.GetComponentInParent<LiveMixin>().TakeDamage(amt, ep.transform.position, DamageType.Electrical, gameObject);
					}
	   			}
			}
			lastTime = time;
		}
		
	}
}

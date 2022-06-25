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
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<AzuriteTag>();
			
			Light l = ObjectUtil.addLight(go);
			l.type = LightType.Point;
			l.color = new UnityEngine.Color(0F, 0.65F, 1F);
			l.intensity = 0.9F;
			l.range = BASE_LIGHT_RANGE;
		}
		
	}
	
	class AzuriteTag : MonoBehaviour {
		
		private float lastTime;
		
		void Start() {
    		
		}
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = time-lastTime;
			Renderer r = gameObject.GetComponentInChildren<Renderer>();
			double phase = gameObject.GetHashCode();
			double sp = 1+0.4*Math.Cos((0.02*gameObject.transform.position.magnitude)%(600*Math.PI)); //was 0.75 and 0.25
			double tick = (sp*time+phase)%(200*Math.PI);
			float lt = (float)Math.Sin(tick)+0.4F*(float)Math.Sin(tick*4.63-289.2);
			float f = CustomMaterials.getMaterial(CustomMaterials.Materials.VENT_CRYSTAL).glow-1.5F+2F*lt;
			RenderUtil.setEmissivity(r, f, "GlowStrength");
			gameObject.GetComponentInChildren<Light>().range = Azurite.BASE_LIGHT_RANGE+0.5F*f;
			if (dT > 0 && Player.main != null) {
				GameObject ep = Player.main.gameObject;
				double distsq = (ep.transform.position-gameObject.transform.position).sqrMagnitude;
				if (distsq < 64) {
					if (Inventory.main.equipment.GetCount(SeaToSeaMod.sealSuit.TechType) == 0) {
						ep.GetComponentInParent<LiveMixin>().TakeDamage(2.5F*dT*(float)Math.Min(1, 1-distsq/64), ep.transform.position, DamageType.Electrical, ep);
					}
				}
			}
			lastTime = time;
		}
		
	}
}

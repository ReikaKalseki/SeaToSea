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
		
		internal static readonly Vector3 mountainBaseAzurite = new Vector3(939.533630371094F, -347.903259277344F, 1443.25720214844F);
		
		public Azurite(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<AzuriteTag>();
			go.EnsureComponent<AzuriteOreSparker>();
			
			Light l = ObjectUtil.addLight(go);
			l.type = LightType.Point;
			l.color = new Color(0F, 0.65F, 1F);
			l.intensity = 0.9F;
			l.range = BASE_LIGHT_RANGE;
		}
		
	}
	
	internal class AzuriteOreSparker : AzuriteSparker {
		AzuriteOreSparker() : base(3.2F, 1.8F) {
			
		}
	}
		
	internal abstract class AzuriteSparker : MonoBehaviour {
		
		private GameObject sparker;
		
		private ParticleSystem[] particles;
		
		private Rigidbody body;
		
		private readonly float size;
		private readonly float activityLevel;
		private readonly Vector3 particleOrigin;
		
		internal AzuriteSparker(float s, float a, Vector3? orgn = null) {
			size = s;
			activityLevel = a;
			particleOrigin = orgn != null && orgn.HasValue ? orgn.Value : Vector3.zero;
		}
		
		void Update() {
			if (!sparker) {
				sparker = ObjectUtil.createWorldObject("ff8e782e-e6f3-40a6-9837-d5b6dcce92bc");
				sparker.transform.localScale = new Vector3(0.4F, 0.4F, 0.4F);
				sparker.transform.parent = transform;
				//sparker.transform.eulerAngles = new Vector3(325, 180, 0);
				ObjectUtil.removeComponent<DamagePlayerInRadius>(sparker);
				ObjectUtil.removeComponent<PlayerDistanceTracker>(sparker);
				ObjectUtil.removeChildObject(sparker, "ElecLight");
			}
			if (particles == null) {
				particles = sparker.GetComponentsInChildren<ParticleSystem>();
			}
			if (!body)
				body = GetComponentInChildren<Rigidbody>();
			if (disableSparking()) {
				sparker.SetActive(false);
			}
			else if (UnityEngine.Random.Range(0, 20) == 0 && Time.deltaTime > 0.01F) {
				if (!sparker.activeSelf) {
					if (UnityEngine.Random.Range(0, 1F) < 0.5F*activityLevel) {
						sparker.SetActive(true);
						sparker.transform.localPosition = particleOrigin;
						foreach (ParticleSystem p in particles) {
							ParticleSystem.MainModule pm = p.main;
							pm.startSize = size*0.2F;
						}
					}
				}
				else if (UnityEngine.Random.Range(0, 1F) > 0.25F*activityLevel) {
					sparker.SetActive(false);
				}
			}
		}
			
		public bool disableSparking() {
			if (gameObject.FindAncestor<AqueousEngineering.ItemDisplayLogic>())
				return false;
			return !body.isKinematic || Vector3.Distance(C2CHooks.mountainBaseGeoCenter, transform.position) <= 40 || gameObject.FindAncestor<Player>();
		}
		
	}
	
	class AzuriteTag : MonoBehaviour {
		
		static readonly float DAMAGE_RANGE = 12;
		static readonly float DAMAGE_RANGE_MOUNTAIN = 6;
		
		private float lastTime;
		
		private Renderer render;
		private Light light;		
		private Rigidbody body;
		
		void Start() {
    		render = gameObject.GetComponentInChildren<Renderer>();
    		light = gameObject.GetComponentInChildren<Light>();
			body = GetComponentInChildren<Rigidbody>();
		}
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = Time.deltaTime;
			double phase = gameObject.GetInstanceID();
			double sp = 1+0.4*Math.Cos((0.02*(body.isKinematic ? gameObject.transform.position.magnitude : 0))%(600*Math.PI)); //was 0.75 and 0.25
			double tick = (sp*time+phase)%(200*Math.PI);
			float lt = (float)Math.Sin(tick)+0.4F*(float)Math.Sin(tick*4.63-289.2);
			float f = CustomMaterials.getMaterial(CustomMaterials.Materials.VENT_CRYSTAL).glow-1.5F+2F*lt;
			RenderUtil.setEmissivity(render, f);
			light.range = Azurite.BASE_LIGHT_RANGE+0.5F*f;
			bool isMountainBase = (transform.position-Azurite.mountainBaseAzurite).sqrMagnitude <= 0.0625F;
			if (isMountainBase)
				body.isKinematic = true;
			if (dT > 0 && body.isKinematic && Player.main != null && !Player.main.IsInsideWalkable() && Player.main.IsSwimming()) {
	   			InventoryItem suit = Inventory.main.equipment.GetItemInSlot("Body");
	   			if (suit == null || (suit.item.GetTechType() != C2CItems.sealSuit.TechType && suit.item.GetTechType() != TechType.ReinforcedDiveSuit)) {
					GameObject ep = Player.main.gameObject;
					float distsq = (ep.transform.position-gameObject.transform.position).sqrMagnitude;
					float r = isMountainBase ? DAMAGE_RANGE_MOUNTAIN : DAMAGE_RANGE;
					r *= r;
					if (distsq < r) {
						float amt = 2.5F*dT*Mathf.Min(1, 1-distsq/r);
						if (isMountainBase)
							amt *= 0.5F;
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

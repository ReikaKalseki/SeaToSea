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

namespace ReikaKalseki.SeaToSea {
	
	public class AcidSpit : Spawnable {
	        
	    internal AcidSpit() : base("acidspit", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			/*
			GameObject tiger = ObjectUtil.lookupPrefab(VanillaFlora.TIGER.getPrefabID());
			RangedAttackLastTarget att = tiger.GetComponent<RangedAttackLastTarget>();
			GameObject shot = att.attackTypes[0].ammoPrefab;
			GameObject world = UnityEngine.Object.Instantiate(shot);
			*/
			GameObject world = new GameObject("Acid Spit(Clone)");
			world.SetActive(false);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			SphereCollider sc = world.EnsureComponent<SphereCollider>();
			sc.radius = 0.167F;
			sc.center = Vector3.zero;
			sc.isTrigger = false;
			Rigidbody rb = world.EnsureComponent<Rigidbody>();
			rb.mass = 1;
			rb.drag = 0;
			rb.useGravity = false;
			//WorldForces wf = world.EnsureComponent<WorldForces>();
			AcidSpitTag kc = world.EnsureComponent<AcidSpitTag>();
			world.EnsureComponent<ImmuneToPropulsioncannon>();
			return world;
	    }
			
	}
	
	public class AcidSpitTag : MonoBehaviour {
		
		public static float fxSize = 0.6F;
		public static float fxLife = 0.1F;
		public static float fxSpread = 0.25F;
		public static float fxRate = 0.1F;
		
		private Rigidbody body;
		
		public Vector3 spawnPosition;
		
		public float damageScalar = 1;
		
		private bool impacted = false;
		
		private float age;
		
		void Start() {			
			body = GetComponent<Rigidbody>();
		}

		void OnCollisionEnter(Collision c) {
			hit(c.gameObject);
		}
		
		private void hit(GameObject go) {
			if (impacted)
				return;
			//SNUtil.writeToChat("Hit "+go.GetFullHierarchyPath());
			LiveMixin lv = UWE.Utils.GetComponentInHierarchy<LiveMixin>(go);
			bool flag = go.layer == Voxeland.GetTerrainLayerMask() || go.layer == 30; //terrain is 30
			if (lv && (ObjectUtil.isPlayer(lv) || lv.GetComponent<Vehicle>())) { //25 damage, 15 instant (zeroed by sealsuit or reinf suit), plus 10 DoT (becomes 2 with sealsuit and 5 with reinf)
				if (!ObjectUtil.isPlayer(lv))
					damageScalar *= 2;
				lv.TakeDamage(15*damageScalar, transform.position, DamageType.Acid, gameObject);
				DamageOverTime dot = lv.gameObject.AddComponent<DamageOverTime>();
				dot.doer = gameObject;
				dot.totalDamage = 10*damageScalar;
				dot.duration = 2F;
				dot.damageType = DamageType.Acid;
				dot.ActivateInterval(0.25F);
				flag = true;
			}
			if (flag) {
				impacted = true;
				body.velocity = Vector3.zero;
				destroy();
			}
		}
		
		void Update() {
			age += Time.deltaTime;
			
			GameObject particle = UnityEngine.Object.Instantiate(ObjectUtil.lookupPrefab("bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782").GetComponent<GasPod>().gasEffectPrefab);
			particle.transform.position = transform.position;
			ObjectUtil.removeComponent<UWE.TriggerStayTracker>(particle);
			ObjectUtil.removeComponent<FMOD_StudioEventEmitter>(particle);
			ObjectUtil.removeComponent<FMOD_CustomEmitter>(particle);
			ObjectUtil.removeChildObject(particle, "xflash");
			float f = impacted ? 2.5F : 1;
			foreach (ParticleSystem pp in particle.GetComponentsInChildren<ParticleSystem>()) {
				ParticleSystem.MainModule main = pp.main;
				main.startSize = fxSize*f;
				main.startLifetime = fxLife*f*f;
				ParticleSystem.ShapeModule sh = pp.shape;
				sh.radius *= fxSpread*f;
				ParticleSystem.VelocityOverLifetimeModule vel = pp.velocityOverLifetime;
				vel.speedModifier = 0;
				main.startSpeed = 0;
				ParticleSystem.EmissionModule emit = pp.emission;
				emit.rateOverTime = fxRate;
			}
			particle.GetComponent<VFXDestroyAfterSeconds>().lifeTime *= fxLife;
			particle.GetComponent<VFXUnparentAfterSeconds>().timer *= fxLife;
			particle.GetComponent<ParticleSystem>().Play(true);
			
			if ((transform.position-Player.main.transform.position).sqrMagnitude < 0.25)
				hit(Player.main.gameObject);
			
			if (age >= 8 || transform.position.y < -800 || (spawnPosition.sqrMagnitude > 1 && (transform.position - spawnPosition).sqrMagnitude > 10000) || body.velocity.sqrMagnitude < 0.25)
				destroy();
		}
		
		void destroy() {
			UnityEngine.Object.Destroy(gameObject, 1.5F);
		}
		
	}
}

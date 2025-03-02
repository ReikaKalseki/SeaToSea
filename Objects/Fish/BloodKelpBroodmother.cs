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
	
	public class BloodKelpBroodmother : RetexturedFish {
		
		private readonly XMLLocale.LocaleEntry locale;
	        
		internal BloodKelpBroodmother(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.CAVECRAWLER.prefab) {
			locale = e;
			
			scanTime = 20;
			
			eggSpawnRate = 0F;
		}
			
		public override void prepareGameObject(GameObject world, Renderer[] r0) {
			BloodKelpBroodmotherTag kc = world.EnsureComponent<BloodKelpBroodmotherTag>();
			
			world.GetComponent<LiveMixin>().data.maxHealth *= 20;
			
			world.GetComponent<MeleeAttack>().biteDamage = 35F;
			
			world.GetComponent<AggressiveWhenSeeTarget>().ignoreSameKind = true;
			
			CaveCrawler cc = world.GetComponent<CaveCrawler>();
			cc.jumpMaxHeight *= 0.06F;
			
			world.GetComponent<WorldForces>().underwaterGravity *= 3;
			
			foreach (Renderer r in world.GetComponentsInChildren<Renderer>()) {
				RenderUtil.setEmissivity(r.materials[0], 2);
				r.materials[0].SetFloat("_SpecInt", 0.5F);
				r.materials[0].SetFloat("_Fresnel", 0.7F);
				r.materials[0].SetFloat("_Shininess", 4F);
				RenderUtil.setEmissivity(r.materials[1], 0); //eye gloss
				r.materials[1].SetFloat("_SpecInt", 0.85F);
			}
			
			StayAtLeashPosition leash = world.GetComponent<StayAtLeashPosition>();
			
			ObjectUtil.removeComponent<CrawlerJumpRandom>(world);
		}
		
		public override BehaviourType getBehavior() {
			return BehaviourType.Crab;
		}
			
	}
	
	class BloodKelpBroodmotherTag : MonoBehaviour {
		
		private Renderer[] renders;
		private CaveCrawler creature;
		
		private CrawlerAttackLastTarget attack;
		private StayAtLeashPosition leash;
		private Locomotion walk;
		private SwimBehaviour behavior;
		private LastTarget target;
		
		private float lastTickTime = -1;
		private float nextSpitTime = -1;
		
		void Start() {
			attack = GetComponent<CrawlerAttackLastTarget>();
			attack.jumpToTarget = false;
			attack.timeNextJump = 999999999;
			leash = GetComponent<StayAtLeashPosition>();
			leash.leashDistance = 8;
			walk = GetComponent<Locomotion>();
			behavior = GetComponent<SwimBehaviour>();
			target = GetComponent<LastTarget>();
		}
		
		void Update() {
			if (renders == null)
				renders = GetComponentsInChildren<Renderer>();
			if (creature == null)
				creature = GetComponentInChildren<CaveCrawler>();
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastTickTime >= 0.5F) {
				lastTickTime = time;
				transform.localScale = new Vector3(9, 6, 9);
				creature.timeLastJump = time - 0.1F;
				creature.leashPosition = C2CProgression.instance.bkelpNestBumps.getClosest(transform.position);
			}
			if ((creature.leashPosition-transform.position).sqrMagnitude < 2500)
				target.SetTarget(Player.main.gameObject);
			if (!target.target || (creature.leashPosition-transform.position).sqrMagnitude > 100)
				behavior.SwimTo(creature.leashPosition, 5);
			if (attack && target.target && time >= nextSpitTime) {
				shoot(target.target);
			}
		}
		
		public void shoot(GameObject target) {
			GameObject shot = ObjectUtil.createWorldObject(SeaToSeaMod.acidSpit.ClassID);
			shot.transform.position = transform.position + transform.forward * 2.4F + transform.up*1.25F;
			shot.GetComponent<AcidSpitTag>().spawnPosition = shot.transform.position;
			ObjectUtil.ignoreCollisions(shot, gameObject);
			Vector3 diff = target.transform.position - Vector3.up * 0.5F - shot.transform.position;
			shot.GetComponent<Rigidbody>().velocity = 18 * diff.normalized;
			nextSpitTime = DayNightCycle.main.timePassedAsFloat + UnityEngine.Random.Range(0.5F, 2F);
		}
		
	}
}

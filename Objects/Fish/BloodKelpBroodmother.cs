using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

			world.removeComponent<CrawlerJumpRandom>();

			GameObject soundRef = ObjectUtil.lookupPrefab(VanillaCreatures.BLOODCRAWLER.prefab);
			List<FMODAsset> sounds = soundRef.GetComponentsInChildren<FMOD_CustomEmitter>().Where(fc => !(fc is FMOD_CustomLoopingEmitter)).Select(fc => fc.asset).ToList();
			List<FMODAsset> soundLoops = soundRef.GetComponentsInChildren<FMOD_CustomLoopingEmitter>().Select(fc => fc.asset).ToList();
			foreach (FMOD_CustomEmitter snd in world.GetComponentsInChildren<FMOD_CustomEmitter>()) {
				List<FMODAsset> li = snd is FMOD_CustomLoopingEmitter ? soundLoops : sounds;
				if (li.Count > 0) {
					snd.asset = soundLoops[0];
					snd.gameObject.EnsureComponent<SoundPitchScale>().pitch = 0.33F;
					soundLoops.RemoveAt(0);
				}
			}
		}

		public override BehaviourType getBehavior() {
			return BehaviourType.Crab;
		}

	}

	class SoundPitchScale : MonoBehaviour {

		public float pitch = 1;

		private FMOD_CustomEmitter sound;

		void Start() {
			sound = this.GetComponent<FMOD_CustomEmitter>();
		}

		void Update() {
			if (sound) {
				FMOD.Studio.EventInstance evt = sound.GetEventInstance();
				if (evt.isValid() && evt.hasHandle()) {
					evt.setPitch(pitch);
				}
			}
		}

	}

	class BloodKelpBroodmotherTag : MonoBehaviour {

		private static readonly SoundManager.SoundData spitSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "broodmotherspit", "Sounds/broodmotherspit.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 64);}, SoundSystem.masterBus);
		private static readonly SoundManager.SoundData idleSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "broodmotheridle", "Sounds/broodmotheridle.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 64);}, SoundSystem.masterBus);

		private Renderer[] renders;
		private CaveCrawler creature;

		private CrawlerAttackLastTarget attack;
		private StayAtLeashPosition leash;
		private Locomotion walk;
		private SwimBehaviour behavior;
		private LastTarget target;

		private float lastTickTime = -1;
		private float nextSpitTime = -1;

		private float nextSoundTime = -1;

		void Start() {
			attack = this.GetComponent<CrawlerAttackLastTarget>();
			attack.jumpToTarget = false;
			attack.timeNextJump = 999999999;
			leash = this.GetComponent<StayAtLeashPosition>();
			leash.leashDistance = 8;
			walk = this.GetComponent<Locomotion>();
			behavior = this.GetComponent<SwimBehaviour>();
			target = this.GetComponent<LastTarget>();
		}

		void Update() {
			if (renders == null)
				renders = this.GetComponentsInChildren<Renderer>();
			if (creature == null)
				creature = this.GetComponentInChildren<CaveCrawler>();
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastTickTime >= 0.5F) {
				lastTickTime = time;
				transform.localScale = new Vector3(9, 6, 9);
				creature.timeLastJump = time - 0.1F;
				creature.leashPosition = C2CProgression.instance.bkelpNestBumps.getClosest(transform.position);
			}
			if ((creature.leashPosition - transform.position).sqrMagnitude < 2500)
				target.SetTarget(Player.main.gameObject);
			if (!target.target || (creature.leashPosition - transform.position).sqrMagnitude > 100)
				behavior.SwimTo(creature.leashPosition, 5);
			if (attack && target.target && time >= nextSpitTime) {
				this.shoot(target.target);
			}
			if (time >= nextSoundTime) {
				SoundManager.playSoundAt(idleSound, transform.position, false, 40, 1);
				nextSoundTime = time + UnityEngine.Random.Range(3F, 10F);
			}
		}

		public void shoot(GameObject target) {
			GameObject shot = ObjectUtil.createWorldObject(SeaToSeaMod.acidSpit.ClassID);
			shot.transform.position = transform.position + (transform.forward * 2.4F) + (transform.up * 1.25F);
			shot.GetComponent<AcidSpitTag>().spawnPosition = shot.transform.position;
			shot.ignoreCollisions(gameObject);
			Vector3 diff = target.transform.position - (Vector3.up * 0.5F) - shot.transform.position;
			shot.GetComponent<Rigidbody>().velocity = 18 * diff.normalized;
			nextSpitTime = DayNightCycle.main.timePassedAsFloat + UnityEngine.Random.Range(0.5F, 2F);
			SoundManager.playSoundAt(spitSound, transform.position, false, 40, 1);
		}

	}
}

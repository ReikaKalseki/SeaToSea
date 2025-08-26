using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using DeExtinctionMod.Prefabs.Creatures;

using ECCLibrary;

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

	public class VoidThalassacean : CreatureAsset {

		private static readonly ThalassaceanPrefab template = (ThalassaceanPrefab)SNUtil.getModPrefabByTechType(DEIntegrationSystem.instance.getThalassacean());

		private readonly XMLLocale.LocaleEntry locale;

		internal VoidThalassacean(XMLLocale.LocaleEntry e)
			: base(e.key, e.name, e.desc, (GameObject)getECCField(template, "model"), null) {
			locale = e;
			typeof(CreatureAsset).GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, (Atlas.Sprite)getECCField(template, "sprite"));
		}

		private static object getECCField(CreatureAsset c, string name) {
			return typeof(CreatureAsset).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(c);
		}

		public override GameObject GetGameObject() {
			GameObject go = base.GetGameObject();
			Renderer r = go.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Creature/VoidThalassacean");
			go.EnsureComponent<VoidThalassaceanTag>();
			return go;
		}

		public override string GetEncyDesc {
			get {
				return locale.pda;
			}
		}

		public override string GetEncyTitle {
			get {
				return locale.name;
			}
		}

		public override ScannableItemData ScannableSettings {
			get {
				ScannableItemData dat = template.ScannableSettings;
				dat.scanTime *= 1.5F;
				return dat;
			}
		}

		public override List<LootDistributionData.BiomeData> BiomesToSpawnIn {
			get {
				return new List<LootDistributionData.BiomeData>();
			}
		}

		//===========copy pasted from ThalassaceanPrefab, since QMM is stupid and shits itself if you reference another mod's type directly

		public override LargeWorldEntity.CellLevel CellLevel {
			get {
				return LargeWorldEntity.CellLevel.Global;
			}
		}

		public override CreatureAsset.SwimRandomData SwimRandomSettings {
			get {
				return new CreatureAsset.SwimRandomData(true, new Vector3(30f, 0f, 30f), 1.2f, 5f, 0.5f);
			}
		}

		public override EcoTargetType EcoTargetType {
			get {
				return EcoTargetType.Whale;
			}
		}

		public override TechType CreatureTraitsReference {
			get {
				return TechType.SeaTreader;
			}
		}

		public override float Mass {
			get {
				return 250f;
			}
		}

		public override float TurnSpeed {
			get {
				return 0.15f;
			}
		}

		public override void SetLiveMixinData(ref LiveMixinData liveMixinData) {
			liveMixinData.maxHealth = 1800f;
			liveMixinData.knifeable = true;
		}

		public override CreatureAsset.RoarAbilityData RoarAbilitySettings {
			get {
				return new CreatureAsset.RoarAbilityData(true, 4f, 30f, "ThalassaceanRoar", string.Empty, 0.51f, 20f, 30f);
			}
		}

		public override AnimationCurve SizeDistribution {
			get {
				return new AnimationCurve(new Keyframe[] {
					new Keyframe(0f, 0.8f),
					new Keyframe(1f, 1f)
				});
			}
		}

		public override BehaviourType BehaviourType {
			get {
				return BehaviourType.Whale;
			}
		}

		public override float MaxVelocityForSpeedParameter {
			get {
				return 6f;
			}
		}

		public override void AddCustomBehaviour(CreatureAsset.CreatureComponents components) {
			base.CreateTrail(GameObjectExtensions.SearchChild(prefab, "root", 0), new Transform[] {
				GameObjectExtensions.SearchChild(prefab, "spine1", 0).transform,
				GameObjectExtensions.SearchChild(prefab, "spine2", 0).transform,
				GameObjectExtensions.SearchChild(prefab, "spine3", 0).transform,
				GameObjectExtensions.SearchChild(prefab, "spine4", 0).transform
			}, components, 0.2f, -1f);/*
			FleeOnDamage fleeOnDamage = this.prefab.AddComponent<FleeOnDamage>();
			fleeOnDamage.breakLeash = true;
			fleeOnDamage.swimVelocity = 6f;
			fleeOnDamage.damageThreshold = 40f;
			fleeOnDamage.evaluatePriority = 0.9f;*/
		}
	}

	class VoidThalassaceanTag : MonoBehaviour, IOnTakeDamage {

		private static readonly float AGGRESSION_TIME = 2.5F;

		private static readonly Color calmColor = new Color(0.2F, 0.5F, 1F);
		private static readonly Color warnColor = new Color(1F, 0.75F, 0.15F);
		private static readonly Color attackingColor = new Color(1F, 0.1F, 0.05F);

		private static readonly SoundManager.SoundData aggroStartSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidthalaroar2", "Sounds/voidthalaroar2.ogg", SoundManager.soundMode3D, s => {
			SoundManager.setup3D(s, 128);
		}, SoundSystem.masterBus);
		private static readonly SoundManager.SoundData attackStartSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidthalachirp", "Sounds/voidthalachirp.ogg", SoundManager.soundMode3D, s => {
			SoundManager.setup3D(s, 128);
		}, SoundSystem.masterBus);
		private static readonly SoundManager.SoundData attackHitSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidthalahit", "Sounds/voidthalahit.ogg", SoundManager.soundMode3D, s => {
			SoundManager.setup3D(s, 128);
		}, SoundSystem.masterBus);

		private Renderer renderer;
		//private SwimRandom swimTarget;
		//private AggressiveWhenSeeTarget aggression;
		//private AggressiveToPilotingVehicle aggression2;
		private AttackLastTarget attack;
		internal Rigidbody body;
		private SwimBehaviour behavior;

		private readonly List<VoidThalaHitDetection> triggers = new List<VoidThalaHitDetection>();
		private static readonly List<int> aggroTokens = new List<int>(){ 1, 2, 3 };
		//prevent more than three tag teaming

		private float aggressionLevel;
		private float aggressionColorFade;

		private float timeAggressive;
		private float timeFleeing;

		private float flashCycleVar;

		private Vector3 runAwayTarget;

		private int currentAggroToken;

		private float returnAttackLifetime;

		void Start() {
			float r = 100;
			renderer = this.GetComponentInChildren<Renderer>();
			renderer.materials[0].SetFloat("_Shininess", 2.5F);
			renderer.materials[0].SetFloat("_SpecInt", 5.0F);
			renderer.materials[0].SetFloat("_Fresnel", 0.75F);
			body = this.GetComponent<Rigidbody>();
			behavior = this.GetComponent<SwimBehaviour>();
			behavior.turnSpeed *= 1.5F;
			//swimTarget = GetComponent<SwimRandom>();
			/*
	    		aggression = gameObject.EnsureComponent<AggressiveWhenSeeTarget>();
	    		aggression.aggressionPerSecond = 1;
	    		aggression.creature = GetComponent<Creature>();
	    		aggression.ignoreSameKind = true;
	    		aggression.myTechType = instance.voidThelassacean.TechType;
	    		aggression.targetType = EcoTargetType.Shark;
	    		aggression.maxRangeScalar = r;
	    		aggression.isTargetValidFilter = (eco => eco.GetGameObject(.isPlayer()));
	    		aggression.lastTarget = gameObject.EnsureComponent<LastTarget>();
	    		aggression2 = gameObject.EnsureComponent<AggressiveToPilotingVehicle>();
	    		aggression2.aggressionPerSecond = aggression.aggressionPerSecond;
	    		aggression2.creature = aggression.creature;
	    		aggression2.lastTarget = aggression.lastTarget;
	    		aggression2.range = r;
	    		aggression2.updateAggressionInterval = 0.5F;
	    		*/
			attack = gameObject.EnsureComponent<AttackLastTarget>();
			attack.aggressionThreshold = 0.5F;
			attack.creature = this.GetComponent<Creature>();//aggression.creature;
			attack.lastTarget = gameObject.EnsureComponent<LastTarget>();//aggression.lastTarget;
			attack.maxAttackDuration = 60;
			attack.minAttackDuration = 30;
			attack.swimVelocity = 45;
			attack.creature.ScanCreatureActions();

			this.GetComponent<LiveMixin>().damageReceivers = this.GetComponents<IOnTakeDamage>();

			this.Invoke("delayedStart", 0.5F);
		}

		void delayedStart() {
			foreach (Collider c in this.GetComponentsInChildren<Collider>(true)) {
				if (!c.isTrigger) {
					c.gameObject.EnsureComponent<VoidThalaHitDetection>().owner = this;
				}
			}
		}

		private GameObject getTarget(out bool vehicle) {
			if (GameModeUtils.IsInvisible()) {
				vehicle = false;
				return null;
			}
			Vehicle v = Player.main.GetVehicle();
			vehicle = (bool)v;
			return v ? v.gameObject : Player.main.gameObject;
		}

		void Update() {
			float far = Player.main ? (Player.main.transform.position - transform.position).sqrMagnitude : 999999;
			//SNUtil.writeToChat("D="+((int)(Mathf.Sqrt(distSq)))/10*10);
			if (far > 90000) { //more than 300m
				gameObject.destroy(false);
				return;
			}
			transform.localScale = Vector3.one * 1.5F;
			GameObject go = this.getTarget(out bool vehicle);
			bool flag = false;
			Color c = calmColor;

			float dT = Time.deltaTime;

			if (go) {
				float distSq = (go.transform.position - transform.position).sqrMagnitude;
				//SNUtil.writeToChat("D="+((int)(Mathf.Sqrt(distSq)))/10*10);
				if (distSq < (vehicle ? 2500 : 400) || aggressionLevel > 0.9F || (returnAttackLifetime > 0 && distSq < 25600)) { //within 50m in vehicle or 20 on foot, or a queued attack
					flag = true;
				}
				else
				if (aggressionLevel < 0 && (distSq > 2500 || (transform.position - runAwayTarget).sqrMagnitude < 900)) { //more than 50m away while running, or at position
					aggressionLevel = 0;
					//SNUtil.writeToChat("Zeroing flee");
				}
			}
			else {
				aggressionLevel = 0;
			}

			if (Math.Abs(aggressionLevel) < 0.01F && body.velocity.magnitude >= 5) {
				body.velocity *= 0.995F;
				//SNUtil.writeToChat("Braking");
			}

			if (timeFleeing > 15)
				aggressionLevel = 0;

			if (returnAttackLifetime > 0)
				returnAttackLifetime -= dT;

			if (flag) {
				if (aggressionLevel >= 0) {
					bool wasAny = aggressionLevel > 0;
					bool was = aggressionLevel >= 1;
					aggressionLevel = Mathf.Clamp01(aggressionLevel + (Time.deltaTime / AGGRESSION_TIME));
					if (aggressionLevel >= 1 && !was) {
						SoundManager.playSoundAt(attackStartSound, transform.position, false, 128, 2);
					}
					else
					if (aggressionLevel > 0 && !wasAny) {
						SoundManager.playSoundAt(aggroStartSound, transform.position, false, 128, 2);
					}
				}
			}
			else {
				aggressionLevel = 0;
				//SNUtil.writeToChat("No target, calming");
			}

			bool flag2 = false;
			if (aggressionLevel < 0) {
				flag2 = true;
				behavior.SwimTo(runAwayTarget, (runAwayTarget - transform.position).normalized, attack.swimVelocity * 0.67F);
				timeFleeing += dT;
			}
			else
			if (aggressionLevel >= 1 && this.tryAllocateAggroToken()) {
				attack.lastTarget.target = go;
				attack.currentTarget = go;
				behavior.Attack(go.transform.position, (go.transform.position - transform.position).normalized, attack.swimVelocity);
				aggressionColorFade = Mathf.Clamp01(aggressionColorFade + (dT * 2));
				timeAggressive += dT;
				//SNUtil.writeToChat("Attacking!");
				timeFleeing = 0;
			}
			else {
				flag2 = true;
				timeFleeing = 0;
			}

			if (flag2) {
				aggressionColorFade = Mathf.Clamp01(aggressionColorFade - (dT * 0.5F));
				this.clearAggro();
			}

			if (timeAggressive > 30)
				this.resetAggro(true);

			c = aggressionColorFade > 0
				? Color.Lerp(warnColor, attackingColor, aggressionColorFade)
				: Color.Lerp(calmColor, warnColor, aggressionLevel);

			float f = body.velocity.magnitude / attack.swimVelocity;
			if (!flag2) //fast while fading from yellow to red
				f = 1.2F;
			flashCycleVar += dT * Mathf.Deg2Rad * 6000 * f; //faster flashing the faster it goes
			float glow = 3.5F + (2.5F * Mathf.Sin(flashCycleVar));
			if (aggressionLevel < 0)
				glow *= 1 + aggressionLevel;

			renderer.materials[0].SetColor("_GlowColor", c);
			RenderUtil.setEmissivity(renderer, glow);
		}

		private bool tryAllocateAggroToken() {
			if (currentAggroToken > 0)
				return true;
			if (aggroTokens.Count == 0)
				return false;
			currentAggroToken = aggroTokens[0];
			aggroTokens.RemoveAt(0);
			return true;
		}

		private void clearAggro() {
			timeAggressive = 0;
			attack.lastTarget.target = null;
			attack.currentTarget = null;
			if (currentAggroToken != 0) {
				if (aggroTokens.Contains(currentAggroToken))
					SNUtil.writeToChat("Two voidthala with same aggro token: " + currentAggroToken);
				aggroTokens.Add(currentAggroToken);
				currentAggroToken = 0;
			}
			//SNUtil.writeToChat("Clearing aggression values");
		}

		public void resetAggro(bool deflect) {
			aggressionLevel = -1;
			Vector3 offset = body.velocity.setLength(120);
			offset = MathUtil.getRandomVectorAround(offset, deflect ? 90 : 50).setLength(120);
			runAwayTarget = Player.main.transform.position + offset;//MathUtil.getRandomPointAtSetDistance(transform.position, 100);
			if (runAwayTarget.y > -20)
				runAwayTarget = runAwayTarget.setY(-20);
			//SNUtil.writeToChat("Resetting aggro");
			this.Invoke("playFleeSound", 0.8F);
		}

		private void playFleeSound() {
			this.CancelInvoke("playFleeSound");
			SoundManager.playSoundAt(attackHitSound, transform.position, false, 128, 1);
		}

		public void OnTakeDamage(DamageInfo info) {
			if (info.type == DamageType.Electrical || info.type == DamageType.Normal)
				this.resetAggro(true);
		}

		public void returnAttack() {
			returnAttackLifetime = 30;
		}

	}

	class VoidThalaHitDetection : MonoBehaviour {

		internal VoidThalassaceanTag owner;

		void Start() {
			if (!owner)
				owner = gameObject.FindAncestor<VoidThalassaceanTag>();
		}

		void OnCollisionEnter(Collision c) {
			if (!owner)
				return;
			if (c.gameObject.FindAncestor<VoidThalaImpactImmunity>())
				return;
			if (c.collider.isPlayer() || c.collider.gameObject.FindAncestor<Vehicle>()) {
				owner.resetAggro(false);
				if (UnityEngine.Random.Range(0F, 1F) < 0.67F) {
					owner.Invoke("returnAttack", UnityEngine.Random.Range(10F, 20F));
				}
				Vehicle v = c.collider.gameObject.FindAncestor<Vehicle>();
				if (v && v.liveMixin) {
					v.liveMixin.TakeDamage(2, c.contacts[0].point, DamageType.Normal, owner.gameObject);
				}
				c.rigidbody.AddForce(owner.body.velocity.setLength(15), ForceMode.VelocityChange);
				c.gameObject.EnsureComponent<VoidThalaImpactImmunity>().elapseWhen = DayNightCycle.main.timePassedAsFloat + 0.5F;
			}
		}

	}

	class VoidThalaImpactImmunity : SelfRemovingComponent {

	}
}

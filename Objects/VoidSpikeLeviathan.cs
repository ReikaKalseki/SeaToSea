using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ECCLibrary;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Exscansion;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class VoidSpikeLeviathan : CreatureAsset {

		private readonly XMLLocale.LocaleEntry locale;

		internal VoidSpikeLeviathan(XMLLocale.LocaleEntry e) : base("voidspikelevi", e.name, e.desc, loadAsset(), null) {
			locale = e;
		}

		public override EcoTargetType EcoTargetType {
			get {
				return EcoTargetType.Leviathan;
			}
		}

		public override BehaviourType BehaviourType {
			get {
				return BehaviourType.Leviathan;
			}
		}

		public override LargeWorldEntity.CellLevel CellLevel {
			get {
				return LargeWorldEntity.CellLevel.VeryFar;
			}
		}

		public override SwimRandomData SwimRandomSettings {
			get {
				return new SwimRandomData(true, Vector3.one * 150, 10F, 5F, 0.1F);
			}
		}

		public override CreatureAsset.CreatureTraitsData TraitsSettings {
			get {
				return new CreatureAsset.CreatureTraitsData(0.1F, 0.02F, 0.25F);
			}
		}

		public override ItemSoundsType ItemSounds {
			get {
				return ItemSoundsType.Fish;
			}
		}

		public override UBERMaterialProperties MaterialSettings {
			get {
				return new UBERMaterialProperties(1F, 3F, 2F);
			}
		}

		public override CreatureAsset.StayAtLeashData StayAtLeashSettings {
			get {
				return new CreatureAsset.StayAtLeashData(0.5F, 150);
			}
		}

		public override CreatureAsset.RespawnData RespawnSettings {
			get {
				return new CreatureAsset.RespawnData(false);
			}
		}

		public override float TurnSpeedHorizontal {
			get {
				return 0.5F;
			}
		}

		public override float TurnSpeedVertical {
			get {
				return 0.8F;
			}
		}

		public override bool ScannerRoomScannable {
			get {
				return true;
			}
		}

		public override bool CanBeInfected {
			get {
				return false;
			}
		}
		/*
                public override CreatureAsset.RoarAbilityData RoarAbilitySettings {
                    get {
                        return new RoarAbilitySettings();
                    }
                }
        */
		public override CreatureAsset.SmallVehicleAggressivenessSettings AggressivenessToSmallVehicles {
			get {
				return new CreatureAsset.SmallVehicleAggressivenessSettings(1F, 440F);
			}
		}

		public override float Mass {
			get {
				return 10000F; //4x reaper
			}
		}

		public override CreatureAsset.BehaviourLODLevelsStruct BehaviourLODSettings {
			get {
				return new CreatureAsset.BehaviourLODLevelsStruct(600, 999, 999);
			}
		}

		public override float EyeFov {
			get {
				return -1;
			}
		}

		public override CreatureAsset.AvoidObstaclesData AvoidObstaclesSettings {
			get {
				return new CreatureAsset.AvoidObstaclesData(0.2F, false, 25);
			}
		}

		public override string GetEncyDesc {
			get {
				return locale.pda;
			}
		}

		public override ScannableItemData ScannableSettings {
			get {
				const string path = "Lifeforms/Fauna/Leviathans";
				return new ScannableItemData(true, 20, path, path.Split('/'), null, TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/PDA/" + locale.getString("header")));
			}
		}

		public override void AddCustomBehaviour(CreatureComponents cc) {
			//if you have special components you want to add here (like your own custom CreatureActions) then add them here
			if (cc.infectedMixin)
				cc.infectedMixin.RemoveInfection();
			if (cc.locomotion)
				cc.locomotion.maxVelocity = 30F;
			cc.pickupable = null;
			if (cc.renderer)
				cc.renderer.materials[0].SetFloat("_Fresnel", 0.8F);
			prefab.EnsureComponent<VoidSpikeLeviathanAI>();
			this.CreateTrail(prefab, cc, 1.5F, 15F, 1.5F);
			this.MakeAggressiveTo(512, 8, EcoTargetType.Leviathan, 0, 1F);
			this.MakeAggressiveTo(512, 8, EcoTargetType.Whale, 0.0F, 0.5F);
			this.MakeAggressiveTo(400, 6, EcoTargetType.Shark, 0.2F, 0.25F);
		}

		public override void SetLiveMixinData(ref LiveMixinData data) {
			data.maxHealth = 75000F; //15x reaper
		}

		private static GameObject loadAsset() {
			AssetBundle ab = ReikaKalseki.DIAlterra.AssetBundleManager.getBundle(SeaToSeaMod.modDLL, "voidlevi");
			return ab.LoadAsset<GameObject>("VoidSpikeLevi_FixedRig");
		}

		public void register() {
			this.Patch();
			//SNUtil.addPDAEntry(this, 20, "Lifeforms/Fauna/Leviathans", locale.pda, locale.getString("header"));
		}

		public static void makeReefbackTest() {
			GameObject go = ObjectUtil.createWorldObject(VanillaCreatures.REEFBACK.prefab);
			VoidSpikeLeviathanAI ai = go.EnsureComponent<VoidSpikeLeviathanAI>();
			ai.spawn();
			GameObject inner = go.getChildObject("Pivot/Reefback/Reefback");
			ai.creatureRenderer = inner.GetComponent<Renderer>();
			go.transform.position = Player.main.transform.position + (Camera.main.transform.forward.normalized * 80);
			ai.isDebug = true;
		}

		internal class VoidSpikeLeviathanAI : MonoBehaviour {

			private static readonly float EMP_CHARGE_TIME = 4F;
			private static readonly float FLASH_CHARGE_TIME = 1.0F;

			private static readonly float MAX_EMISSIVITY_DELTA = 100F;

			internal bool isDebug = false;

			private static readonly SoundManager.SoundData empChargeSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidlevi-emp-charge", "Sounds/voidlevi/emp-charge-2.ogg", SoundManager.soundMode3D);
			private static readonly SoundManager.SoundData flashChargeSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidlevi-flash-charge", "Sounds/voidlevi/flash-charge-2.ogg", SoundManager.soundMode3D);

			private Creature creatureClass;

			internal Renderer creatureRenderer;

			private float empRamp = -1;
			private float flashRamp = -1;

			private float nextPossibleBurstTime = 10;

			private float targetEmissivity;
			private float currentEmissivity = 1;

			void Start() {

			}

			internal void spawn() {
				float time = DayNightCycle.main.timePassedAsFloat;
				nextPossibleBurstTime = time + 10;
			}

			void Update() {
				if (!creatureClass)
					creatureClass = this.GetComponent<Creature>();
				if (!creatureRenderer)
					creatureRenderer = this.GetComponentInChildren<Renderer>();

				if (creatureClass) {
					creatureClass.leashPosition = Player.main.transform.position;
					if (isDebug)
						creatureClass.Aggression.Add(1);

					if (Story.StoryGoalManager.main.completedGoals.Contains(VoidSpikeLeviathanSystem.PASSIVATION_GOAL)) { //sea emperor befriend
						creatureClass.Aggression.Add(-0.1F);
					}
				}

				float dT = Time.deltaTime;
				float time = DayNightCycle.main.timePassedAsFloat;
				if (this.isEMPInProgress()) {
					empRamp += dT / EMP_CHARGE_TIME;
					if (empRamp >= 1) {
						this.doEMP();
						empRamp = -1;
					}
				}
				else if (this.isFlashInProgress()) {
					flashRamp += dT / FLASH_CHARGE_TIME;
					if (flashRamp >= 1) {
						this.doFlash();
						flashRamp = -1;
					}
				}
				else if (time >= nextPossibleBurstTime && this.canStartAPulse() && UnityEngine.Random.Range(0F, 1F) <= 0.01F * creatureClass.Aggression.Value) {
					float f = 0.15F+(0.35F*creatureClass.Aggression.Value);
					if (UnityEngine.Random.Range(0F, 1F) < f)
						this.startFlash();
					else
						this.startEMP();
				}

				targetEmissivity = 1F + (0.5F * Mathf.Sin(transform.position.magnitude * 0.075F));
				if (this.isEMPInProgress()) {
					targetEmissivity = Mathf.Max(0, (empRamp * 4F) + UnityEngine.Random.Range(0F, 2.5F) + (4F * Mathf.Sin(empRamp * 8)));
				}
				else if (this.isFlashInProgress()) {
					targetEmissivity = Mathf.Max(0, flashRamp * flashRamp * 50 * UnityEngine.Random.Range(0, 1F)/*+0.5F+15F*Mathf.Sin(flashRamp*50)*/);
				}

				if (currentEmissivity < targetEmissivity) {
					currentEmissivity = Mathf.Min(targetEmissivity, currentEmissivity + (dT * MAX_EMISSIVITY_DELTA));
				}
				else if (currentEmissivity > targetEmissivity) {
					currentEmissivity = Mathf.Max(targetEmissivity, currentEmissivity - (dT * MAX_EMISSIVITY_DELTA));
				}

				if (creatureRenderer)
					RenderUtil.setEmissivity(creatureRenderer.materials[0], Mathf.Max(0, currentEmissivity));
			}

			internal void startEMP() {
				if (!this.canStartAPulse())
					return;
				empRamp = 0;
				SoundManager.playSoundAt(empChargeSound, transform.position, false, -1, 1);
				nextPossibleBurstTime = DayNightCycle.main.timePassedAsFloat + UnityEngine.Random.Range(30, isDebug ? 31 : 90);
			}

			internal void startFlash() {
				if (!this.canStartAPulse())
					return;
				flashRamp = 0;
				SoundManager.playSoundAt(flashChargeSound, transform.position, false, -1, 1);
				nextPossibleBurstTime = DayNightCycle.main.timePassedAsFloat + UnityEngine.Random.Range(60, isDebug ? 90 : 120);
			}

			internal bool isEMPInProgress() {
				return empRamp >= 0;
			}

			internal bool isFlashInProgress() {
				return flashRamp >= 0;
			}

			internal bool canStartAPulse() {
				return !this.isFlashInProgress() && !this.isEMPInProgress() && !VoidSpikeLeviathanSystem.instance.isVoidFlashActive(true);
			}

			private void doEMP() {
				VoidSpikeLeviathanSystem.instance.spawnEMPBlast(transform.position);
			}

			private void doFlash() {
				VoidSpikeLeviathanSystem.instance.doFlash(transform.position);
			}

			void OnDisable() {
				gameObject.destroy(false);
			}

			void OnDestroy() {
				VoidSpikeLeviathanSystem.instance.deleteVoidLeviathan();
			}

			public void OnMeleeAttack(GameObject target) {
				//SNUtil.writeToChat(this+" attacked "+target);
				Vehicle v = target.GetComponent<Vehicle>();
				if (v) {
					VoidSpikeLeviathanSystem.instance.shutdownSeamoth(v, true, 6); //shut down for up to 30s, drain up to 25% power
				}
			}

		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	internal class C2CMoth : MonoBehaviour {

		private static readonly SoundManager.SoundData startPurgingSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "startheatsink", "Sounds/startheatsink2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 40);}, SoundSystem.masterBus);
		private static readonly SoundManager.SoundData meltingSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "seamothmelt", "Sounds/seamothmelt2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 120);}, SoundSystem.masterBus);
		private static readonly SoundManager.SoundData boostSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "seamothboost", "Sounds/seamothboost.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 120);}, SoundSystem.masterBus);
		private static readonly SoundManager.SoundData purgeEnergySound = SoundManager.registerSound(SeaToSeaMod.modDLL, "seamothsounddump", "Sounds/stealthsounddump2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 120);}, SoundSystem.masterBus);
		//private static readonly SoundManager.SoundData ejectionPrepareSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "heatsinkEjectPrepare", "Sounds/heatsinkejectprepare.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 120);}, SoundSystem.masterBus);

		private static readonly Vector3 sweepArchCave = new Vector3(1570, -338, 1075);

		internal static bool useSeamothVehicleTemperature = true;

		public static bool temperatureDebugActive = false;

		private static readonly float TICK_RATE = 0.1F;
		private static readonly float HOLD_LOW_TIME = 30.8F;

		public static readonly float MAX_VOIDSTEALTH_ENERGY = 2400;

		public static float getOverrideTemperature(float temp) {
			if (!useSeamothVehicleTemperature)
				return temp;
			Player ep = Player.main;
			if (!ep)
				return temp;
			Vehicle v = ep.GetVehicle();
			return !v ? temp : getOverrideTemperature(v, temp);
		}

		public static float getOverrideTemperature(Vehicle v, float temp) {
			if (!useSeamothVehicleTemperature)
				return temp;
			if (v is SeaMoth) {
				C2CMoth cm = v.GetComponent<C2CMoth>();
				if (cm)
					return cm.vehicleTemperature;
			}
			return temp;
		}

		public SeaMoth seamoth { get; private set; }
		public TemperatureDamage temperatureDamage { get; private set; }
		private VFXVehicleDamages damageFX;
		private FMOD_CustomLoopingEmitter engineSounds;
		private VehicleAccelerationModifier speedModifier;
		private SeamothTetherController tethers;
		public Rigidbody body { get; private set; }

		private float baseDamageAmount;

		public float vehicleTemperature { get; private set; }

		private float holdTempLowTime = 0;

		private float temperatureAtPurge = -1;

		public bool isPurgingHeat { get { return temperatureAtPurge >= 0; } }

		private float lastMeltSound = -1;
		//private float lastPreEjectSound = -1;

		private Channel? heatsinkSoundEvent;
		private Channel? boostSoundEvent;

		private float lastTickTime = -1;

		public float speedBonus { get; private set; }

		private Vector3 jitterTorque;
		private Vector3 jitterTorqueTarget;

		public int stuckCells { get; private set; }
		public bool touchingKelp { get; private set; }

		public float voidStealthStoredEnergy { get; private set; }

		public bool hasVoidStealth = false;

		//private Renderer deepStalkerStorageDamage;

		private PredatoryBloodvine holdingBloodKelp;

		private static uGUI_SeamothHUD seamothHUD;
		private SeamothWithStealthHUD stealthEnabledSeamothHUDElement;

		public float soundStorageScalar { get { return Mathf.Clamp01(voidStealthStoredEnergy/MAX_VOIDSTEALTH_ENERGY); } } //0-1

		public C2CMoth() {
			vehicleTemperature = 25;
		}

		void Start() {
			useSeamothVehicleTemperature = false;
			vehicleTemperature = WaterTemperatureSimulation.main.GetTemperature(transform.position);
			useSeamothVehicleTemperature = true;

			this.Invoke("validateDepthModules", 0.5F);
		}

		void validateDepthModules() {
			if (!seamoth || seamoth.modules == null) {
				this.Invoke("validateDepthModules", 0.5F);
				return;
			}
			if (!C2CProgression.isSeamothDepth1UnlockedLegitimately()) {
				foreach (int idx in seamoth.slotIndexes.Values) {
					InventoryItem ii = seamoth.GetSlotItem(idx);
					if (ii != null && ii.item) {
						TechType tt = ii.item.GetTechType();
						if (tt == TechType.VehicleHullModule1 || tt == TechType.VehicleHullModule2 || tt == TechType.VehicleHullModule3 || tt == C2CItems.depth1300.TechType) {
							ItemUnlockLegitimacySystem.instance.destroyModule(seamoth.modules, ii, seamoth.slotIDs[idx]);
							seamoth.liveMixin.TakeDamage(10); //stop cheating
							KnownTech.Remove(TechType.VehicleHullModule1);
							KnownTech.Remove(TechType.VehicleHullModule2);
							KnownTech.Remove(TechType.VehicleHullModule3);
							KnownTech.Remove(C2CItems.depth1300.TechType);
						}
					}
				}

			}
		}

		void Update() {
			if (C2CHooks.skipSeamothTick)
				return;
			if (!stealthEnabledSeamothHUDElement) {
				seamothHUD = UnityEngine.Object.FindObjectOfType<uGUI_SeamothHUD>();
				if (seamothHUD) {
					stealthEnabledSeamothHUDElement = seamothHUD.gameObject.EnsureComponent<SeamothWithStealthHUD>();
					if (!stealthEnabledSeamothHUDElement.root) {
						GameObject hudRoot = seamothHUD.root.transform.parent.gameObject;
						uGUI_ExosuitHUD exo = seamothHUD.GetComponent<uGUI_ExosuitHUD>();
						GameObject go = exo.root.gameObject.clone().setName("SeamothStealthHUD");
						go.SetActive(true);
						go.transform.SetParent(exo.root.transform.parent);
						go.transform.localPosition = exo.root.transform.localPosition;
						go.transform.localRotation = exo.root.transform.localRotation;
						go.transform.localScale = exo.root.transform.localScale;
						stealthEnabledSeamothHUDElement.init(exo);
						stealthEnabledSeamothHUDElement.root = go;
					}

					Image bcg = stealthEnabledSeamothHUDElement.root.getChildObject("Background").GetComponent<Image>();
					Texture2D tex = TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/SeamothStealthHUD");
					bcg.sprite = TextureManager.createSprite(tex);
					Image bar = stealthEnabledSeamothHUDElement.root.getChildObject("ThrustBar").GetComponent<Image>();
					Material mat = bar.material;
					tex = TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/SeamothStealthEnergyBar");
					bar.sprite = TextureManager.createSprite(tex);
					bar.material = mat;
					mat.mainTexture = tex;
					//bcg.GetComponent<RectTransform>().sizeDelta = new Vector2(tex.width / 2F, tex.height / 2F);
					//bcg.transform.localPosition = new Vector3(22, 0, 0);
				}
			}
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = time-lastTickTime;
			if (dT >= TICK_RATE) {
				this.tick(time, Mathf.Min(1, dT));
				lastTickTime = time;
			}
		}

		internal void purgeHeat() {
			temperatureAtPurge = vehicleTemperature;
			SNUtil.log("Starting heat purge (" + temperatureAtPurge + ") @ " + DayNightCycle.main.timePassedAsFloat, SeaToSeaMod.modDLL);
			//Invoke("fireHeatsink", 1.5F);
			heatsinkSoundEvent = SoundManager.playSoundAt(startPurgingSound, transform.position, false, -1, 0.67F);
		}

		internal void fireHeatsink(float time) {
			SNUtil.log("Heat purge complete @ " + time + " (" + holdTempLowTime + "/" + HOLD_LOW_TIME + "), firing heatsink", SeaToSeaMod.modDLL);
			GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.ejectedHeatSink.ClassID);
			go.transform.position = seamoth.transform.position + (seamoth.transform.forward * 4);
			go.GetComponent<Rigidbody>().AddForce(seamoth.transform.forward * 20, ForceMode.VelocityChange);
			body.AddForce(-seamoth.transform.forward * 5, ForceMode.VelocityChange);
			go.GetComponent<HeatSinkTag>().onFired(Mathf.Clamp01((temperatureAtPurge / 250F * 0.25F) + 0.75F));
		}

		internal void dumpSoundEnergy() {
			if (voidStealthStoredEnergy <= 0)
				return;
			Utils.PlayOneShotPS(ObjectUtil.lookupPrefab(VanillaCreatures.CRASHFISH.prefab).GetComponent<Crash>().detonateParticlePrefab, transform.position+transform.forward*2, Quaternion.identity);
			SoundManager.playSoundAt(purgeEnergySound, transform.position, false, -1, 2 * soundStorageScalar);
			//ECHooks.attractToSoundPing(seamoth, false, 1); //range 400 to attract
			/*
			for (int i = 0; i < UWE.Utils.OverlapSphereIntoSharedBuffer(transform.position, 30); i++) {
				Collider collider = UWE.Utils.sharedColliderBuffer[i];
				GameObject go = UWE.Utils.GetEntityRoot(collider.gameObject);
				if (!go)
					go = collider.gameObject;
				Creature c = go.GetComponent<Creature>();
				LiveMixin lv = go.GetComponent<LiveMixin>();
				if (c != null && lv != null) {
					lv.TakeDamage(2, transform.position, DamageType.Explosive, gameObject);
				}
			}
			*/
			float r = 150*Mathf.Clamp(soundStorageScalar, 0.33F, 0.67F); //so minimum 50m <= 33% and max 100m >= 67%
			/*
			foreach (AggressiveToPilotingVehicle a in WorldUtil.getObjectsNearWithComponent<AggressiveToPilotingVehicle>(transform.position, r)) {
				if (a.lastTarget && a.lastTarget.target && a.lastTarget.target == gameObject)
					a.lastTarget.SetTarget(null);
			}*/
			foreach (AttackLastTarget a in WorldUtil.getObjectsNearWithComponent<AttackLastTarget>(transform.position, r)) {
				a.clearAttackTarget();
			}
			voidStealthStoredEnergy = 0;
		}

		internal void applySpeedBoost(float charge) {
			if (speedBonus > 0.5F || !seamoth.HasEnoughEnergy(5))
				return;
			speedBonus = 2F;
			seamoth.ConsumeEnergy(5);
			boostSoundEvent = SoundManager.playSoundAt(boostSound, transform.position, false, -1, 1);
			seamoth.screenEffectModel.SetActive(true);
			ECHooks.attractToSoundPing(seamoth, false, 0.33F);
			if (holdingBloodKelp)
				holdingBloodKelp.release();
			if (seamoth.liveMixin.GetHealthFraction() < 0.67F)
				seamoth.liveMixin.TakeDamage(5);
		}

		public void OnBloodKelpGrab(PredatoryBloodvine c) {
			holdingBloodKelp = c;
		}

		internal void onHitByLavaBomb(LavaBombTag bomb) {
			vehicleTemperature = Mathf.Max(vehicleTemperature, bomb.getTemperature());
		}

		internal void tick(float time, float tickTime) {
			if (!seamoth)
				seamoth = this.GetComponent<SeaMoth>();
			if (!body)
				body = this.GetComponent<Rigidbody>();
			if (!engineSounds) {
				engineSounds = this.GetComponentInChildren<EngineRpmSFXManager>().gameObject.GetComponent<FMOD_CustomLoopingEmitter>();
			}

			if (!speedModifier) {
				speedModifier = seamoth.gameObject.AddComponent<VehicleAccelerationModifier>();
				seamoth.accelerationModifiers = seamoth.GetComponentsInChildren<VehicleAccelerationModifier>();
			}
			if (!temperatureDamage) {
				temperatureDamage = this.GetComponent<TemperatureDamage>();
				baseDamageAmount = temperatureDamage.baseDamagePerSecond;
			}
			if (!tethers)
				tethers = this.GetComponent<SeamothTetherController>();
			if (!damageFX)
				damageFX = gameObject.GetComponent<VFXVehicleDamages>();

			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);

			if (UnityEngine.Random.Range(0F, 1F) < 0.5F) {
				//stuckCells = GetComponentsInChildren<VoidBubbleTag>().Length;
				stuckCells = 0;
				foreach (VoidBubbleTag vb in WorldUtil.getObjectsNearWithComponent<VoidBubbleTag>(transform.position, 24)) {
					if (vb.isStuckTo(body))
						stuckCells++;
				}
			}

			float health = seamoth.liveMixin.GetHealthFraction();

			float minSpeedBonus = seamoth.isVehicleUpgradeSelected(C2CItems.speedModule.TechType) ? 0.25F : 0;
			if (speedBonus > minSpeedBonus)
				speedBonus *= 0.933F;
			else
				speedBonus = Mathf.Min(minSpeedBonus, speedBonus + 0.1F);
			speedModifier.accelerationMultiplier = 1 + speedBonus;
			if (stuckCells > 0)
				speedModifier.accelerationMultiplier *= Mathf.Exp(-stuckCells * 0.2F);
			if (touchingKelp)
				speedModifier.accelerationMultiplier *= 0.3F;
			if (health < 0.9F)
				speedModifier.accelerationMultiplier *= Mathf.Max(0.1F, health / 0.9F);
			if (tethers.isTowing())
				speedModifier.accelerationMultiplier *= 0.6F;
			//SNUtil.writeToChat(speedBonus.ToString("0.000"));

			if (heatsinkSoundEvent != null && heatsinkSoundEvent.Value.hasHandle()) {
				ATTRIBUTES_3D attr = transform.position.To3DAttributes();
				heatsinkSoundEvent.Value.set3DAttributes(ref attr.position, ref attr.velocity, ref attr.forward);
			}
			if (boostSoundEvent != null && boostSoundEvent.Value.hasHandle()) {
				ATTRIBUTES_3D attr = transform.position.To3DAttributes();
				boostSoundEvent.Value.set3DAttributes(ref attr.position, ref attr.velocity, ref attr.forward);
			}

			if (engineSounds && engineSounds.evt.isValid()) {
				if (hasVoidStealth) {
					engineSounds.evt.setVolume(0.25F);
				}
				else {
					engineSounds.evt.setVolume(1 + speedBonus);
				}
			}

			if (hasVoidStealth && body) {
				voidStealthStoredEnergy += tickTime * 0.2F * body.velocity.magnitude;
				if (voidStealthStoredEnergy >= MAX_VOIDSTEALTH_ENERGY)
					seamoth.liveMixin.Kill(DamageType.Explosive);
			}

			if (speedBonus > 0.5F) { //during boost only
				float jitter = ((speedBonus+1)*(speedBonus+1))-1; //0.25 -> 0.56, 3 -> 8
				Vector3 add = jitterTorque*tickTime*jitter*25000;
				body.AddTorque(add, ForceMode.Force);
				//SNUtil.writeToChat("Adding jitter: "+add);
			}
			if ((jitterTorque - jitterTorqueTarget).sqrMagnitude < 0.01) {
				jitterTorqueTarget = UnityEngine.Random.onUnitSphere;//MathUtil.getRandomVectorAround(Vector3.zero, 3).setLength(1);
			}
			else {
				jitterTorque += (jitterTorqueTarget - jitterTorque) * Mathf.Min(1, tickTime * 9);
			}

			bool kooshCave = false;

			if (health < 0.5F) {
				float force = 1+(Mathf.Pow((0.5F-health)*2, 1.5F)*9);
				body.AddForce(Vector3.down * tickTime * 50, ForceMode.Acceleration);
			}

			if (VanillaBiomes.KOOSH.isInBiome(transform.position)) {
				string biome = WaterBiomeManager.main.GetBiome(transform.position, false);
				if (biome != null && biome.ToLowerInvariant().Contains("cave") && Vector3.Distance(transform.position, sweepArchCave) >= 40) {
					kooshCave = true;
					Vector3 vel = body.velocity;
					Vector3 vec = Vector3.zero;
					vec = vel.magnitude < 0.2 ? UnityEngine.Random.onUnitSphere * 0.6F : MathUtil.rotateVectorAroundAxis(Vector3.Cross(vel, Vector3.up), seamoth.transform.forward, UnityEngine.Random.Range(0F, 360F)).setLength(0.8F);
					body.AddForce(vec, ForceMode.VelocityChange);
				}
			}

			if (seamoth.GetPilotingMode()) {
				VoidSpikesBiome.instance.tickTeleportCheck(seamoth);
			}

			if (this.isPurgingHeat) {
				vehicleTemperature -= tickTime * 150;
				if (vehicleTemperature <= 5) {
					vehicleTemperature = 5;
					holdTempLowTime += tickTime;
					if (holdTempLowTime >= HOLD_LOW_TIME) {
						this.fireHeatsink(time);
						temperatureAtPurge = -1;
					}
				}
				else {
					holdTempLowTime = 0;
				}
				if (temperatureDebugActive)
					SNUtil.writeToChat("Purging: " + vehicleTemperature.ToString("0000.00") + " > " + holdTempLowTime.ToString("00.00"));
			}
			else {
				holdTempLowTime = 0;
				useSeamothVehicleTemperature = false;
				float Tamb = temperatureDamage.GetTemperature();// this will call WaterTempSim, after the lava checks in DI
				if (seamoth.docked || seamoth.IsInsideAquarium() || EnvironmentalDamageSystem.instance.isInPrecursor(gameObject))
					Tamb = 25;
				else if (kooshCave)
					Tamb = 95;
				useSeamothVehicleTemperature = true;
				float dT = Tamb-vehicleTemperature;
				float excess = Mathf.Clamp01((vehicleTemperature-400)/400F);
				float f0 = dT > 0 ? 4F : 25F-(15*excess);
				float f1 = dT > 0 ? 5F : 1F+(1.5F*excess);
				float speed = seamoth.useRigidbody.velocity.magnitude;
				if (speed >= 2) {
					f0 /= 1 + ((speed - 2) / 8F);
				}
				float qDot = tickTime*Math.Sign(dT)*Mathf.Min(Math.Abs(dT), Mathf.Max(f1, Math.Abs(dT)/f0));
				if (qDot > 0) {
					if (Tamb < 300)
						qDot *= hard ? 0.33F : 0.25F;
					else
						qDot *= hard ? 0.8F : 0.67F;
				}
				vehicleTemperature += qDot;
				if (temperatureDebugActive)
					SNUtil.writeToChat(Tamb + " > " + dT + " > " + speed.ToString("00.0") + " > " + f0.ToString("00.0000") + " > " + qDot.ToString("00.0000") + " > " + vehicleTemperature.ToString("0000.00"));
			}
			float factor = 1+(Mathf.Max(0, vehicleTemperature-250)/25F);
			float f2 = Mathf.Min(hard ? 36 : 32, Mathf.Pow(factor, 2.5F));
			temperatureDamage.baseDamagePerSecond = baseDamageAmount * f2;
			//SNUtil.writeToChat(vehicleTemperature+" > "+factor.ToString("00.0000")+" > "+f2.ToString("00.0000")+" > "+temperatureDamage.baseDamagePerSecond.ToString("0000.00"));
			if (vehicleTemperature >= 90 && seamoth.GetPilotingMode()) {
				damageFX.OnTakeDamage(new DamageInfo { damage = 1, type = DamageType.Heat });
				if (time - lastMeltSound >= 0.5F && !seamoth.docked && UnityEngine.Random.Range(0F, 1F) <= 0.25F) {
					SoundManager.playSoundAt(meltingSound, Player.main.transform.position, false, -1, 0.125F + (Mathf.Clamp01((vehicleTemperature - 90) / 100F) * 0.125F));
					lastMeltSound = time;
				}
			}
		}

		public void recalculateModules() {
			if (!seamoth) {
				this.Invoke("recalculateModules", 0.5F);
				return;
			}
			hasVoidStealth = seamoth.vehicleHasUpgrade(C2CItems.voidStealth.TechType);
			if (!hasVoidStealth)
				voidStealthStoredEnergy = 0;
			this.validateDepthModules();
		}

		class SeamothWithStealthHUD : uGUI_ExosuitHUD {

			internal void init(uGUI_ExosuitHUD from) {
				this.copyObject(from);
			}

			void Start() {
				textHealth = root.getChildObject("Health").GetComponent<Text>();
				textPower = root.getChildObject("Power").GetComponent<Text>();
				textTemperature = root.getChildObject("Temperature").GetComponent<Text>();
				imageThrust = root.getChildObject("ThrustBar").GetComponent<Image>();
				imageThrust.material = new Material(imageThrust.material);
			}

			private new void Update() {
				bool flag1 = false;
				bool flag2 = false;
				SeaMoth sm = null;
				if (Player.main) {
					sm = Player.main.GetVehicle() as SeaMoth;
					flag1 = (bool)sm && !(Player.main.GetPDA() && Player.main.GetPDA().isInUse);
					flag2 = flag1 && sm.vehicleHasUpgrade(C2CItems.voidStealth.TechType);
				}
				root.SetActive(flag2);
				if (seamothHUD)
					seamothHUD.root.SetActive(flag1 && !flag2);
				if (!flag2)
					return;
				sm.GetHUDValues(out float health, out float power);
				C2CMoth cm = sm.GetComponent<C2CMoth>();
				float thrust = cm.soundStorageScalar;
				float temperature = cm.vehicleTemperature;
				int num4 = Mathf.CeilToInt(health * 100f);
				if (this.lastHealth != num4) {
					this.lastHealth = num4;
					this.textHealth.text = IntStringCache.GetStringForInt(this.lastHealth);
				}
				int num5 = Mathf.CeilToInt(power * 100f);
				if (this.lastPower != num5) {
					this.lastPower = num5;
					this.textPower.text = IntStringCache.GetStringForInt(this.lastPower);
				}
				if (this.lastThrust != thrust) {
					this.lastThrust = thrust;
					this.imageThrust.material.SetFloat(ShaderPropertyID._Amount, this.lastThrust);
				}
				this.temperatureSmoothValue = ((this.temperatureSmoothValue < -10000f) ? temperature : Mathf.SmoothDamp(this.temperatureSmoothValue, temperature, ref this.temperatureVelocity, 1f));
				int num6 = Mathf.CeilToInt(this.temperatureSmoothValue);
				if (this.lastTemperature != num6) {
					this.lastTemperature = num6;
					this.textTemperature.text = IntStringCache.GetStringForInt(this.lastTemperature);
					this.textTemperatureSuffix.text = Language.main.GetFormat("ThermometerFormat");
				}
			}

		}

	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.AqueousEngineering;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	public class MoraleSystem {

		public static readonly MoraleSystem instance = new MoraleSystem();

		private static readonly float CYCLOPS_MIN_DECO = 4;
		private static readonly float CYCLOPS_MAX_DECO = 9;
		private static readonly float BASE_MIN_DECO = 7.5F;
		private static readonly float BASE_MAX_DECO = 15;

		//minutes from 100% morale to 0 or 0 to 100%, at max strength
		private static readonly float CYCLOPS_MORALE_TIME_TO_DECAY = 45;
		private static readonly float CYCLOPS_MORALE_TIME_TO_FILL = 20;
		private static readonly float BASE_MORALE_TIME_TO_DECAY = 15;
		private static readonly float BASE_MORALE_TIME_TO_FILL = 10;

		private static readonly float MINUTE_DURATION_TO_PER_SECOND_CONVERSION = 0.6F; //x60 for time in seconds then /100 to per percent

		//percent per second
		private static readonly float CYCLOPS_MORALE_LOSS_SPEED = 1F/(MINUTE_DURATION_TO_PER_SECOND_CONVERSION*CYCLOPS_MORALE_TIME_TO_DECAY);
		private static readonly float CYCLOPS_MORALE_GAIN_SPEED = 1F/(MINUTE_DURATION_TO_PER_SECOND_CONVERSION*CYCLOPS_MORALE_TIME_TO_FILL);
		private static readonly float BASE_MORALE_LOSS_SPEED = 1F/(MINUTE_DURATION_TO_PER_SECOND_CONVERSION*BASE_MORALE_TIME_TO_DECAY);
		private static readonly float BASE_MORALE_GAIN_SPEED = 1F/(MINUTE_DURATION_TO_PER_SECOND_CONVERSION*BASE_MORALE_TIME_TO_FILL);

		private static readonly float PRAWN_MORALE_COOLDOWN = 600; //10 min

		private static readonly float INITIAL_MORALE = 50;
		private static readonly float INITIAL_REAL_MORALE = 10;

		private static readonly float GENERIC_DEAD_LIFEPOD_MORALE_IMPACT = -10;
		private static readonly float GENERIC_DEAD_LIFEPOD_MORALE_DURATION = 60*10; //10 min

		private static readonly float LEISURE_ROOM_CONSTANT_BONUS = 5; //+5%/s

		public static uint printMoraleForDebug = 0;

		private readonly Dictionary<BiomeBase, AmbientMoraleInfluence> biomeEffect = new Dictionary<BiomeBase, AmbientMoraleInfluence>();
		private readonly Dictionary<string, float> goalMorale = new Dictionary<string, float>();
		private readonly List<Func<Player, float>> baselineEffects = new List<Func<Player, float>>();

		private GameObject barsRoot;
		private MoraleBar bar;

		private float lastDecoCheckTime;
		private float currentDecoLevel;
		private BaseRoomSpecializationSystem.RoomTypes currentRoom;

		private float lastBaselineCheckTime;

		private float timeInPrawn;
		private float prawnMoraleCooldown;

		private float timeUntilMoraleAdoptsRealValue;

		private float lastCoffeeTime;

		public float moralePercentage { get; private set; }
		public float maxMorale { get; private set; }
		public float currentMoraleBaseline { get; private set; }

		//private MoraleVisual moraleVisual;

		private BleederAttachTarget bleederTarget;

		public enum MoraleDebugFlags {
			STACKTRACE = 1,
			CORE = 2,
			SHIFT = 4,
			SET = 8,
			BIOME = 16,
			DECO = 32,
		}

		public static void setMoraleDebugFlags(string names) {
			if (names.ToLowerInvariant() == "all") {
				printMoraleForDebug = 0xffffffff;
			}
			else {
				printMoraleForDebug = 0;
				foreach (string s in names.Split('/')) {
					if (Enum.TryParse(s.ToUpperInvariant(), out MoraleDebugFlags flag)) {
						printMoraleForDebug |= (uint)flag;
					}
				}
			}
		}

		private static bool checkMoraleDebugFlag(MoraleDebugFlags flag) {
			return (printMoraleForDebug & (uint)flag) != 0;
		}

		private MoraleSystem() {
			biomeEffect[VanillaBiomes.ALZ] = new AmbientMoraleInfluence(-20, -2, -2);
			biomeEffect[VanillaBiomes.ILZ] = new AmbientMoraleInfluence(-10, 0, -1);
			biomeEffect[VanillaBiomes.BLOODKELP] = new AmbientMoraleInfluence(-1, 0, -0.5F);
			biomeEffect[VanillaBiomes.BLOODKELPNORTH] = new AmbientMoraleInfluence(-1, 0, -0.75F);
			biomeEffect[VanillaBiomes.COVE] = new AmbientMoraleInfluence(2, 2, 2);
			biomeEffect[VanillaBiomes.CRASH] = new AmbientMoraleInfluence(-5, 0, -2);
			biomeEffect[VanillaBiomes.DUNES] = new AmbientMoraleInfluence(-0.5F, 0, -1);
			biomeEffect[VanillaBiomes.GRANDREEF] = new AmbientMoraleInfluence(0.5F, 1, 1);
			biomeEffect[VanillaBiomes.JELLYSHROOM] = new AmbientMoraleInfluence(0, -40, 1);
			biomeEffect[VanillaBiomes.SHALLOWS] = new AmbientMoraleInfluence(0.05F, -20, 0);

			goalMorale["AuroraExplode"] = -20;

			goalMorale["Goal_BiomeDunes"] = -20; //are you sure what you are doing is worth it

			//"OMG RESCUE!"
			this.registerBaselineAdjustment(ep => SNUtil.isSunbeamExpected() ? 200 : 0);
			//hope falls, discontent rises
			float sunbeamCrisisDuration = 3600; //1h
			this.registerBaselineAdjustment("PDASunbeamDestroyEventInRange", -200, sunbeamCrisisDuration);
			this.registerBaselineAdjustment("PDASunbeamDestroyEventOutOfRange", -200, sunbeamCrisisDuration);
			goalMorale["PDASunbeamDestroyEventInRange"] = -200;
			goalMorale["PDASunbeamDestroyEventOutOfRange"] = -200;

			goalMorale["Precursor_Gun_DisableDenied"] = -100; //No rescue until cure

			//T4 infection gives one-time -50 (via the cinematic) and a continuous -10
			goalMorale["Precursor_LostRiverBase_DataDownload4"] = -50;
			this.registerBaselineAdjustment(ep => ep.GetInfectionAmount() > 0.9F ? -10 : 0);

			this.registerBaselineAdjustment("Lifepod2", GENERIC_DEAD_LIFEPOD_MORALE_IMPACT, GENERIC_DEAD_LIFEPOD_MORALE_DURATION); //pod12
			this.registerBaselineAdjustment("LifepodCrashZone2", GENERIC_DEAD_LIFEPOD_MORALE_IMPACT, GENERIC_DEAD_LIFEPOD_MORALE_DURATION); //pod6 #2
			this.registerBaselineAdjustment("LifepodSeaglide", GENERIC_DEAD_LIFEPOD_MORALE_IMPACT, GENERIC_DEAD_LIFEPOD_MORALE_DURATION); //pod17, ozzy
			this.registerBaselineAdjustment("Lifepod3", GENERIC_DEAD_LIFEPOD_MORALE_IMPACT * 2, GENERIC_DEAD_LIFEPOD_MORALE_DURATION); //; x2 strength since usually first
			this.registerBaselineAdjustment("Lifepod1", GENERIC_DEAD_LIFEPOD_MORALE_IMPACT, GENERIC_DEAD_LIFEPOD_MORALE_DURATION); //pod2
			this.registerBaselineAdjustment("Lifepod4", GENERIC_DEAD_LIFEPOD_MORALE_IMPACT / 2F, GENERIC_DEAD_LIFEPOD_MORALE_DURATION * 0.67F); //pod13, khasar; half because Alterra vs Mongolians

			goalMorale["LifepodDecoy"] = -25;
			this.registerBaselineAdjustment("LifepodDecoy", GENERIC_DEAD_LIFEPOD_MORALE_IMPACT, GENERIC_DEAD_LIFEPOD_MORALE_DURATION); //pod4; also add strong negative impulse because of realization of danger

			this.registerBaselineAdjustment("LifepodKeenLog", 100, "RendezvousFloatingIsland", -50, sunbeamCrisisDuration / 2F); //survivors on an island...except not

			this.registerBaselineAdjustment("rescuepdalog", 30, "treepda", -20, 60 * 15); //hint of rescue...never mind

			this.registerBaselineAdjustment(ep => SNUtil.isPlayerCured() ? 10 : 0); //permanent +10 after cure
			this.registerBaselineAdjustment("RocketComplete", 10); //another permanent +10 after rocket built

			//boosts from a few major milestones
			goalMorale["RepairLifepod"] = 25;
			goalMorale["Goal_Seamoth"] = 50;
			goalMorale["AuroraRadiationFixed"] = 40;
			goalMorale["Goal_Exo"] = 50;
			goalMorale["Goal_Cyclops"] = 75;
			goalMorale["Goal_Disable_Gun"] = 100;
			goalMorale["RocketComplete"] = 100;
			goalMorale["SeaEmperorBabiesHatched"] = 50;
			goalMorale["Infection_Progress5"] = 200;

			this.reset();

			//moraleVisual = new MoraleVisual();
			//ScreenFXManager.instance.addOverride(moraleVisual);
		}

		public void register() {
			StoryHandler.instance.addListener(this.onStoryGoal);

			SaveSystem.addPlayerSaveCallback(new SaveHook());
		}

		public void registerStoryGoalEffect(string goal, float effect) {
			if (goalMorale.ContainsKey(goal))
				throw new Exception("Goal '" + goal + "' already has a morale effect (" + instance.goalMorale[goal] + ")!");
			goalMorale[goal] = effect;
		}

		public void registerBiomeEffect(BiomeBase bb, AmbientMoraleInfluence amb) {
			if (!biomeEffect.ContainsKey(bb))
				biomeEffect[bb] = amb;
		}

		public void registerBaselineAdjustment(ProgressionTrigger check, float effect) {
			this.registerBaselineAdjustment(ep => check.isReady(ep) ? effect : 0);
		}

		public void registerBaselineAdjustment(string goal1, float effect1, string goal2, float effect2Initial, float fadeTime = -1) {
			this.registerBaselineAdjustment(ep => {
				if (StoryGoalManager.main.IsGoalComplete(goal1)) {
					if (StoryGoalManager.main.IsGoalComplete(goal2)) {
						if (fadeTime < 0)
							return effect2Initial;
						float since = StoryHandler.instance.getTimeSince(goal2);
						return since >= fadeTime ? 0 : (float)MathUtil.linterpolate(since, 0, fadeTime, effect2Initial, 0, true);
					}
					else {
						return effect1;
					}
				}
				else {
					return 0;
				}
			});
		}

		public void registerBaselineAdjustment(string goal, float initial, float fadeTime = -1) {
			this.registerBaselineAdjustment(ep => {
				if (StoryGoalManager.main.IsGoalComplete(goal)) {
					if (fadeTime < 0)
						return initial;
					float since = StoryHandler.instance.getTimeSince(goal);
					return since >= fadeTime ? 0 : (float)MathUtil.linterpolate(since, 0, fadeTime, initial, 0, true);
				}
				else {
					return 0;
				}
			});
		}

		public void registerBaselineAdjustment(Func<Player, float> effect) {
			if (effect == null)
				throw new Exception("Invalid null effect!");
			baselineEffects.Add(effect);
		}

		private void onStoryGoal(string goal) {
			if (goalMorale.ContainsKey(goal))
				this.shiftMorale(goalMorale[goal]);

			if (goal == "Goal_Lifepod2") //exit pod, and this is where morale *actually* begins
				timeUntilMoraleAdoptsRealValue = 7.5F;

			if (goal == "OnPlayRadioBounceBack") {
				Player.main.gameObject.EnsureComponent<MoraleEffect9999>().InvokeRepeating("trigger", 7.25F, 1);
			}
		}

		class MoraleEffect9999 : MonoBehaviour {

			private int times = 0;

			void trigger() {
				times++;
				MoraleSystem.instance.shiftMorale(-6 * times);
				if (times >= 5) {
					CancelInvoke("trigger");
					UnityEngine.Object.Destroy(this, 0.5F);
				}
			}

		}
		/*
		class MoraleVisual : ScreenFXManager.ScreenFXOverride {

			private float currentEffect;

			internal MoraleVisual() : base(200) {

			}

			public override void onTick() {
				float targetEffect = (float)MathUtil.linterpolate(MoraleSystem.instance.moralePercentage, 0, 30, 0.2, 0, true);
				float dE = targetEffect-currentEffect;
				if (targetEffect <= 0) {
					currentEffect -= Time.deltaTime;
				}
				else if (!Mathf.Approximately(dE, 0)) {
					currentEffect += dE * 0.5F * Time.deltaTime;
				}
				if (currentEffect > 0) {
					ScreenFXManager.instance.registerOverrideThisTick(ScreenFXManager.instance.telepathyShader);
					ScreenFXManager.instance.telepathyShader.amount = currentEffect;
				}
			}

		}
*/
		public void reset() {
			moralePercentage = INITIAL_MORALE;
		}

		public void tick(Player ep) {
			if (!ep || !ep.liveMixin || GameModeUtils.currentEffectiveMode == GameModeOption.Creative || GameModeUtils.currentEffectiveMode == GameModeOption.Freedom || !DIHooks.isWorldLoaded() || /*ep.cinematicModeActive*/IntroVignette.isIntroActive || EscapePod.main.IsPlayingIntroCinematic()) {
				setMorale(75);
				if (bar)
					bar.gameObject.SetActive(false);
				return;
			}
			if (!barsRoot) {
				uGUI_OxygenBar o2bar = UnityEngine.Object.FindObjectOfType<uGUI_OxygenBar>();
				if (o2bar) {
					barsRoot = o2bar.transform.parent.gameObject;
					bar = barsRoot.GetComponentInChildren<MoraleBar>();
					if (!bar) {
						GameObject wb = barsRoot.getChildObject("WaterBar");
						GameObject fb = barsRoot.getChildObject("FoodBar");
						GameObject mb = UnityEngine.Object.Instantiate(wb).setName("MoraleBar");
						mb.transform.SetParent(wb.transform.parent);
						Vector3 diff = fb.transform.localPosition-wb.transform.localPosition;
						mb.transform.localPosition = wb.transform.localPosition + new Vector3(-diff.x, diff.y, diff.z);
						mb.transform.localScale = Vector3.one;
						uGUI_WaterBar refBar = mb.GetComponentInChildren<uGUI_WaterBar>();
						bar = mb.gameObject.EnsureComponent<MoraleBar>();
						bar.init(refBar);
						refBar.OnDisable();
						refBar.destroy();
						mb.SetActive(true);
					}

					Image bcg = barsRoot.getChildObject("BackgroundQuad/Quad").GetComponent<Image>();
					Texture2D tex = TextureManager.getTexture(AqueousEngineeringMod.modDLL, "Textures/HUDBarsBCG");
					bcg.sprite = TextureManager.createSprite(tex);
					bcg.GetComponent<RectTransform>().sizeDelta = new Vector2(tex.width / 2F, tex.height / 2F);
					bcg.transform.localPosition = new Vector3(22, 0, 0);
				}
			}

			if (bar)
				bar.gameObject.SetActive(true);

			if (!bleederTarget)
				bleederTarget = ep.GetComponentInChildren<BleederAttachTarget>();

			Vehicle v = ep.GetVehicle();
			BiomeBase bb = BiomeBase.getBiome(ep.transform.position);
			AmbientMoraleInfluence amb = bb != null && biomeEffect.ContainsKey(bb) ? biomeEffect[bb] : null;

			if (checkMoraleDebugFlag(MoraleDebugFlags.CORE))
				SNUtil.writeToChat("Morale UI initialized, applying sim");

			float delta = 0;
			float dT = Time.deltaTime;
			float time = DayNightCycle.main.timePassedAsFloat;

			if (timeUntilMoraleAdoptsRealValue > 0) {
				timeUntilMoraleAdoptsRealValue -= dT;
				if (timeUntilMoraleAdoptsRealValue <= 0)
					this.setMorale(INITIAL_REAL_MORALE);
			}

			if (checkMoraleDebugFlag(MoraleDebugFlags.BIOME))
				SNUtil.writeToChat("Biome " + bb + " morale ambient " + amb);

			if (time - lastBaselineCheckTime > 1F) {
				lastBaselineCheckTime = time;
				currentMoraleBaseline = 0;
				foreach (Func<Player, float> e in baselineEffects) {
					currentMoraleBaseline += e.Invoke(ep);
				}

				if (checkMoraleDebugFlag(MoraleDebugFlags.CORE))
					SNUtil.writeToChat("Computed morale baseline " + currentMoraleBaseline.ToString("0.00"));
			}

			if (ep.currentSub) {
				this.resetPrawnTime();
				if (time - lastDecoCheckTime > 0.5F) {
					lastDecoCheckTime = time;
					currentRoom = BaseRoomSpecializationSystem.instance.getPlayerRoomType(ep, out currentDecoLevel, out float thresh);
				}

				bool cyclops = ep.currentSub.isCyclops;
				float min = cyclops ? CYCLOPS_MIN_DECO : BASE_MIN_DECO;

				if (currentDecoLevel < min) {
					float lossLim = cyclops ? CYCLOPS_MORALE_LOSS_SPEED : BASE_MORALE_LOSS_SPEED;
					delta = -(float)MathUtil.linterpolate(currentDecoLevel, 0, min, lossLim, 0, true);
				}
				else {
					float gainCeil = cyclops ? CYCLOPS_MAX_DECO : BASE_MAX_DECO;
					float gainLim = cyclops ? CYCLOPS_MORALE_GAIN_SPEED : BASE_MORALE_GAIN_SPEED;
					delta = (float)MathUtil.linterpolate(currentDecoLevel, min, gainCeil, 0, gainLim, true);
				}

				if (checkMoraleDebugFlag(MoraleDebugFlags.DECO))
					SNUtil.writeToChat("Base deco morale " + delta.ToString("0.00") + "/s from " + currentDecoLevel);

				delta *= AqueousEngineeringMod.config.getFloat(AEConfig.ConfigEntries.MORALESPEED);

				switch (currentRoom) {
					case BaseRoomSpecializationSystem.RoomTypes.LEISURE:
						delta += LEISURE_ROOM_CONSTANT_BONUS;
						break;
				}

				if (amb != null && cyclops)
					delta += amb.moralePerSecondCyclops;
			}
			else if (v) {
				//if (checkMoraleDebugFlag(MoraleDebugFlags.))
				//	SNUtil.writeToChat("Vehicle is "+v+" with vel "+v.useRigidbody.velocity.magnitude.ToString());
				if (v is SeaMoth sm) {
					this.resetPrawnTime();
					delta += (float)MathUtil.linterpolate(sm.useRigidbody.velocity.magnitude, 5, 20, 0, 1.0); //up to 1% per second if moving fast
					if (amb != null)
						delta += amb.moralePerSecondSeamoth;
				}
				else if (v is Exosuit p) {
					float bonus = 0.5F*(1-(timeInPrawn/120F)); //"sense of limitless power" -> 0.5%/s fading over 120 seconds (total morale maximum: +30)
					if (bonus > 0)
						delta += bonus;
					if (amb != null)
						delta += amb.moralePerSecondPrawn;
					prawnMoraleCooldown = PRAWN_MORALE_COOLDOWN;
					timeInPrawn += dT;
				}
				else {
					this.resetPrawnTime();
				}
			}
			else {
				if (amb != null && ep.IsSwimming() && !ep.precursorOutOfWater && ep.transform.position.y < -1)
					delta += amb.moralePerSecondFreeDiving;
				this.resetPrawnTime();
			}
			/*
            float temp = WaterTemperatureSimulation.main.GetTemperature(ep.transform.position);
            if (temp < 0)
                delta -= 2;
            else if (temp > 40)
                delta -= (float)MathUtil.linterpolate(temp, 40, 90, 0, 10, true);
            */
			prawnMoraleCooldown = Mathf.Max(prawnMoraleCooldown - (dT / PRAWN_MORALE_COOLDOWN), 0);

			if (checkMoraleDebugFlag(MoraleDebugFlags.CORE))
				SNUtil.writeToChat("Core sim computed to delta "+delta);

			Survival s = ep.GetComponent<Survival>();
			float f = s ? Mathf.Min(s.water, s.food) : 1;
			maxMorale = Mathf.Min(ep.liveMixin.health * 1.25F, f * 2); //need both food and water >= 50 and health over 80 for 100%

			if (checkMoraleDebugFlag(MoraleDebugFlags.CORE))
				SNUtil.writeToChat("Max morale " + maxMorale.ToString("0.00") + " from " + f + " & " + ep.liveMixin.health);

			if (ep.GetOxygenAvailable() <= 0.001)
				delta = -20;

			if (bleederTarget && bleederTarget.occupied)
				delta = -10;

			delta += currentMoraleBaseline;

			if (checkMoraleDebugFlag(MoraleDebugFlags.CORE))
				SNUtil.writeToChat("Final morale delta " + delta.ToString("0.00") + "/s");

			//SNUtil.writeToChat("Adjusting morale by "+delta.ToString("0.00")+"/s because of "+currentDecoLevel.ToString("0.0"));
			this.shiftMorale(delta * dT);
		}

		private void resetPrawnTime() {
			if (prawnMoraleCooldown <= 0)
				timeInPrawn = 0;
		}

		public void shiftMorale(float delta) {
			if (checkMoraleDebugFlag(MoraleDebugFlags.SHIFT))
				SNUtil.writeToChat("Shifting morale by " + delta.ToString("0.0"));
			this.setMorale(moralePercentage + delta);
		}

		private void setMorale(float f) {
			if (checkMoraleDebugFlag(MoraleDebugFlags.SET)) {
				SNUtil.writeToChat("Setting morale to " + f.ToString("0.0"));
				if (checkMoraleDebugFlag(MoraleDebugFlags.STACKTRACE)) {
					SNUtil.log("Setting morale to " + f.ToString("0.0"));
					SNUtil.log(SNUtil.getStacktrace());
				}
			}
			moralePercentage = Mathf.Clamp(f, 0, Mathf.Min(100, maxMorale));
			if (bar)
				bar.setValue(moralePercentage);
		}

		internal bool areFoodsDifferent(TechType item1, TechType item2) {
			return item1.getFoodCategory() != item2.getFoodCategory();
		}

		internal void onDrinkCoffee() {
			float time = DayNightCycle.main.timePassedAsFloat;
			float diff = time-lastCoffeeTime;
			lastCoffeeTime = time;
			if (diff >= 3600 && DayNightCycle.main.GetLightScalar() > 0.2F) {
				shiftMorale(10);
			}
			else {
				shiftMorale(diff < 300 ? -15 : -5);
			}
		}

		class MoraleBar : uGUI_WaterBar {

			private Image iconImage;

			private float lastSetTime;

			private float targetPercentage;

			void Start() {
				iconImage = this.GetComponentInChildren<Image>();

				bar.color = new Color(0.7F, 0.2F, 1F);
				bar.borderColor = new Color(0.8F, 0.67F, 1F);

				dampSpeed *= 3F;

				Texture2D tex = TextureManager.getTexture(AqueousEngineeringMod.modDLL, "Textures/MoraleIcon");
				iconImage.sprite = TextureManager.createSprite(tex, 100);
				RectTransform rt = iconImage.gameObject.GetComponent<RectTransform>();
				rt.sizeDelta = new Vector2(tex.width / 2F, tex.height / 2F);
				rt.transform.localPosition = new Vector3(21, 0, 0);

				iconImage.transform.localPosition = Vector3.zero;

				this.OnDisable();
			}

			internal void init(uGUI_WaterBar refBar) {
				this.copyObject(refBar);
			}

			private void updateValue() {
				float tgt = targetPercentage/100F;
				curr = Mathf.SmoothDamp(curr, tgt, ref vel, dampSpeed);
				bar.value = curr;
				int num = Mathf.CeilToInt(targetPercentage);
				if (cachedValue != num) {
					cachedValue = num;
					text.text = IntStringCache.GetStringForInt(cachedValue);
				}
			}

			internal void setValue(float pct) {
				float time = DayNightCycle.main.timePassedAsFloat;
				float diff = Mathf.Abs(pct-targetPercentage);
				if (diff <= 0.5F && time - lastSetTime <= 1)
					return;
				targetPercentage = pct;
				lastSetTime = time;
				if (diff > 10) {
					float maxScale = 1f + Mathf.Clamp01((diff-10F) / 50F);
					this.Punch(2.5f, maxScale);
				}
			}

			new void LateUpdate() {
				bool flag = showNumbers;
				showNumbers = false;
				Player main = Player.main;
				if (main != null) {
					float num = Mathf.Clamp01(curr / pulseReferenceCapacity);
					float time = 1f - num;
					pulseDelay = pulseDelayCurve.Evaluate(time);
					if (pulseDelay < 0f) {
						pulseDelay = 0f;
					}
					pulseTime = pulseTimeCurve.Evaluate(time);
					if (pulseTime < 0f) {
						pulseTime = 0f;
					}
					float num2 = pulseDelay + pulseTime;
					if (pulseTween.duration > 0f && num2 <= 0f) {
						pulseAnimationState.normalizedTime = 0f;
					}
					pulseTween.duration = num2;

					PDA pda = main.GetPDA();
					if (pda != null && pda.isInUse) {
						showNumbers = true;
					}

					if (pulseAnimationState != null && pulseAnimationState.enabled) {
						icon.localScale += punchScale;
					}
					else {
						icon.localScale = punchScale;
					}
					if (flag != showNumbers) {
						rotationVelocity += UnityEngine.Random.Range(-rotationRandomVelocity, rotationRandomVelocity);
					}
					float time2 = Time.time;
					float num3 = 0.02f;
					float num4 = time2 - lastFixedUpdateTime;
					int num5 = Mathf.FloorToInt(num4);
					if (num5 > 20) {
						num5 = 1;
						num3 = num4;
					}
					lastFixedUpdateTime += num5 * num3;
					for (int i = 0; i < num5; i++) {
						float num6 = rotationCurrent;
						float num7 = showNumbers ? 180f : 0f;
						MathExtensions.Spring(ref rotationVelocity, ref rotationCurrent, num7, rotationSpringCoef, num3, rotationVelocityDamp, rotationVelocityMax);
						if (Mathf.Abs(num7 - rotationCurrent) < 1f && Mathf.Abs(rotationVelocity) < 1f) {
							rotationVelocity = 0f;
							rotationCurrent = num7;
						}
						if (num6 != rotationCurrent) {
							icon.localRotation = Quaternion.Euler(0f, rotationCurrent, 0f);
						}
					}
				}

				this.updateValue();
			}
		}

		public class AmbientMoraleInfluence {

			public readonly float moralePerSecondFreeDiving;
			public readonly float moralePerSecondCyclops;
			public readonly float moralePerSecondSeamoth;
			public readonly float moralePerSecondPrawn;

			public AmbientMoraleInfluence(float m, float c, float v = 0) : this(m, c, v, v) {

			}

			public AmbientMoraleInfluence(float m, float c, float sm, float p) {
				moralePerSecondFreeDiving = m;
				moralePerSecondCyclops = c;
				moralePerSecondSeamoth = sm;
				moralePerSecondPrawn = p;
			}

			public override string ToString() {
				return moralePerSecondFreeDiving.ToString("0.00") + "/" + moralePerSecondSeamoth.ToString("0.00") + "/" + moralePerSecondPrawn.ToString("0.00") + "/" + moralePerSecondCyclops.ToString("0.00");
			}

		}

		class SaveHook : SaveSystem.PlayerSaveHook {

			public SaveHook() : base("Morale", save, load) {

			}

			private static void save(Player ep, XmlElement e) {
				e.addProperty("morale", instance.moralePercentage);
			}

			private static void load(Player ep, XmlElement e) {
				instance.moralePercentage = (float)e.getFloat("morale", INITIAL_MORALE);
			}
		}
	}
}

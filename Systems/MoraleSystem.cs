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
using Oculus.Newtonsoft.Json;

namespace ReikaKalseki.SeaToSea {
	public class MoraleSystem {

		public static readonly MoraleSystem instance = new MoraleSystem();

		private static readonly float CYCLOPS_MIN_DECO = 4;
		private static readonly float CYCLOPS_MAX_DECO = 9;
		private static readonly float BASE_MIN_DECO = 7.5F;
		private static readonly float BASE_MAX_DECO = 15;

		//minutes from 100% morale to 0 or 0 to 100%, at max strength
		private static readonly float CYCLOPS_MORALE_TIME_TO_DECAY = 45;
		private static readonly float CYCLOPS_MORALE_TIME_TO_FILL = 10;
		private static readonly float BASE_MORALE_TIME_TO_DECAY = 15;
		private static readonly float BASE_MORALE_TIME_TO_FILL = 5;

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
		private static readonly float GENERIC_RADIO_MORALE_BOOST = 10;
		private static readonly float GENERIC_RADIO_MORALE_DURATION = 60*30; //30 min
		private static readonly float GENERIC_DEAD_LIFEPOD_MORALE_DURATION = 60*10; //10 min

		private static readonly float LEISURE_ROOM_CONSTANT_BONUS = 2; //+2%/s
		internal static readonly float OBSERVATORY_CONSTANT_BONUS = 0.5F; //+0.5%/s

		private static readonly float INITIAL_MORALE_BASELINE = 20;
		private static readonly float MORALE_RESTORATION_RATE = 0.05F;

		public static readonly float MORALE_DAMAGE_COEFFICIENT = 1;

		public static uint printMoraleForDebug = 0;

		private static readonly MoraleInfluence NO_MORALE_EFFECT = new MoraleInfluence(new ConstantValue<Player>(0), new ConstantValue<Player>(0));

		private readonly Dictionary<BiomeBase, AmbientMoraleInfluence> biomeEffect = new Dictionary<BiomeBase, AmbientMoraleInfluence>();
		private readonly Dictionary<string, float> goalMorale = new Dictionary<string, float>();
		private readonly List<ConditionalMoraleInfluence> persistentEffects = new List<ConditionalMoraleInfluence>();

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
		public float currentMoraleForce { get; private set; }

		//private MoraleVisual moraleVisual;

		private BleederAttachTarget bleederTarget;

		public enum MoraleDebugFlags {
			STACKTRACE = 1,
			CORE = 2,
			SHIFT = 4,
			SET = 8,
			BIOME = 16,
			DECO = 32,
			BASELINE = 64,
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

		}

		public void register() {
			registerBiomeEffect(VanillaBiomes.ALZ).setEnv(-5, 0, EnvContext.FREEDIVE).setEnv(-1F, 0, EnvContext.SEAMOTH);
			registerBiomeEffect(VanillaBiomes.ILZ).setEnv(-0.4F, 0, EnvContext.FREEDIVE).setEnv(-0.1F, 0, EnvContext.SEAMOTH);
			registerBiomeEffect(VanillaBiomes.BLOODKELP).setEnv(-0.1F, -20F, EnvContext.FREEDIVE).setEnv(0, -10F, EnvContext.SEAMOTH);
			registerBiomeEffect(VanillaBiomes.BLOODKELPNORTH).setEnv(-0.125F, -25F, EnvContext.FREEDIVE).setEnv(0, -12F, EnvContext.SEAMOTH);
			registerBiomeEffect(VanillaBiomes.COVE).setEnv(0.5F, 25F).setEnv(1, 50F, EnvContext.SEABASE);
			registerBiomeEffect(VanillaBiomes.CRASH).setEnv(-1, -50F).setEnv(0, -20F, EnvContext.CYCLOPS);
			registerBiomeEffect(VanillaBiomes.DUNES).setEnv(-0.1F, -10F).setEnv(0, -5F, EnvContext.CYCLOPS);
			registerBiomeEffect(VanillaBiomes.GRANDREEF).setEnv(0.25F, 5F).setEnv(0, 2.5F, EnvContext.FREEDIVE).setEnv(0, 10F, EnvContext.SEABASE);
			registerBiomeEffect(VanillaBiomes.JELLYSHROOM).setEnv(-5, -100, EnvContext.CYCLOPS).setEnv(0, 10F, EnvContext.SEABASE);
			registerBiomeEffect(VanillaBiomes.SHALLOWS).setEnv(0.05F, 0).setEnv(-1, -100, EnvContext.CYCLOPS).setEnv(0.125F, 5F, EnvContext.SEABASE);

			goalMorale["AuroraExplode"] = -20;

			goalMorale["Goal_BiomeDunes"] = -20; //are you sure what you are doing is worth it

			//-------------------
			//this is not a baseline, this is a delta/s!
			//-------------------
			//"OMG RESCUE!"
			this.registerPersistentEffect(ep => SNUtil.isSunbeamExpected(), 0, 200);
			//hope falls, discontent rises
			float sunbeamCrisisDuration = 3600; //1h
			this.registerPersistentEffect("SunbeamDestroyed", true, -200, sunbeamCrisisDuration);

			goalMorale[StoryGoals.INFECTED_REJECTION] = -100; //No rescue until cure

			//T4 infection gives one-time -50 (via the cinematic) and a continuous -5
			goalMorale[StoryGoals.INFECTED_CINEMATIC] = -50;
			this.registerPersistentEffect(ep => ep.GetInfectionAmount() > 0.9F, -0.2F, -5);

			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(StoryGoals.POD12RADIO), StoryGoals.POD12);
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(StoryGoals.POD6RADIO), StoryGoals.POD6B);
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(StoryGoals.POD17RADIO), StoryGoals.POD17); //ozzy
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(StoryGoals.POD3RADIO), StoryGoals.POD3, 2F); //x2 strength since usually first
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(StoryGoals.POD2RADIO), StoryGoals.POD2);
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(StoryGoals.POD13RADIO), StoryGoals.POD13, 0.5F); //khasar; half because Alterra vs Mongolians
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(StoryGoals.POD7RADIO), StoryGoals.POD7);
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(StoryGoals.POD19RADIO), StoryGoals.ISLAND_RENDEZVOUS, 1.5F); //only clear once rendezvous fails
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(StoryGoals.POD4RADIO), StoryGoals.POD4); //also add strong negative impulse because of realization of danger
			goalMorale[StoryGoals.POD4] = -25;

			this.registerPersistentEffect(StoryGoals.POD19RENDEZVOUS, false, 100, sunbeamCrisisDuration * 2, StoryGoals.ISLAND_RENDEZVOUS, -50, sunbeamCrisisDuration / 2F); //survivors on an island...except not

			this.registerLifepodMoraleShifts("rescuepdalog", "treepda", 1.5F); //hint of rescue...never mind
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(SeaToSeaMod.treaderSignal.storyGate), "treaderpod");
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(VoidSpikesBiome.instance.getSignalKey()), "voidpod");
			this.registerLifepodMoraleShifts(StoryGoals.getRadioPlayGoal(SeaToSeaMod.crashMesaRadio.key), "crashmesa");

			this.registerPermanentGoalEffect(StoryGoals.getRadioPlayGoal(StoryGoals.ALTERRA_HQ), 0, 5); //+5 from knowing there is a rocket plan
			this.registerPermanentGoalEffect(StoryGoals.ROCKET_INFO, 0, 5); //permanent +5 to baseline after rocket known

			this.registerPersistentEffect(ep => SNUtil.isPlayerCured(), 5, 20); //permanent +5/+20 after cure
			this.registerPermanentGoalEffect(StoryGoals.ROCKET_COMPLETE, 5, 10); //another permanent +5/+10 after rocket built

			//boosts from a few major milestones
			goalMorale[StoryGoals.REPAIR_LIFEPOD] = 25;
			goalMorale[StoryGoals.MAKE_SEAMOTH] = 50;
			goalMorale[StoryGoals.AURORA_FIX] = 40;
			goalMorale[StoryGoals.getRadioPlayGoal(StoryGoals.ALTERRA_HQ)] = 20;
			goalMorale[StoryGoals.MAKE_PRAWN] = 50;
			goalMorale[StoryGoals.MAKE_CYCLOPS] = 75;
			goalMorale[StoryGoals.DISABLE_GUN] = 100;
			goalMorale[StoryGoals.ROCKET_COMPLETE] = 100;
			goalMorale[StoryGoals.EMPEROR_HATCH] = 50;
			goalMorale[StoryGoals.CURED] = 200;

			this.reset();

			//moraleVisual = new MoraleVisual();
			//ScreenFXManager.instance.addOverride(moraleVisual);

			StoryHandler.instance.addListener(this.onStoryGoal);

			SaveSystem.addPlayerSaveCallback(new SaveHook());

			addItemMorale(CraftingItems.getItem(CraftingItems.Items.CrystalLens), 50);
			addItemMorale(CraftingItems.getItem(CraftingItems.Items.Nanocarbon), 20);
			addItemMorale(CraftingItems.getItem(CraftingItems.Items.DenseAzurite), 10);
			addItemMorale(CraftingItems.getItem(CraftingItems.Items.Motor), 2);
			addItemMorale(CraftingItems.getItem(CraftingItems.Items.BioEnzymes), 5);
			addItemMorale(CraftingItems.getItem(CraftingItems.Items.LathingDrone), 25);
			addItemMorale(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 10);
			addItemMorale(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), 20);
			addItemMorale(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS), 10);
			addItemMorale(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL), 5);
			addItemMorale(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL), 50);
			addItemMorale(C2CItems.bandage, 10);
			addItemMorale(C2CItems.breathingFluid, 20);
			addItemMorale(C2CItems.rebreatherV2, 20);
			addItemMorale(C2CItems.t2Battery, 10);

			addItemMorale(TechType.VehicleHullModule1, 5);
			addItemMorale(TechType.VehicleHullModule2, 10);
			addItemMorale(TechType.VehicleHullModule3, 20);
			addItemMorale(C2CItems.depth1300, 30);

			addItemMorale(TechType.Silver, 10);
			addItemMorale(TechType.Gold, 10);
			addItemMorale(TechType.Nickel, 10);
			addItemMorale(TechType.Kyanite, 25);
			addItemMorale(TechType.Magnetite, 5);

			addItemMorale(TechType.PrecursorIonCrystal, 10);
			addItemMorale(TechType.PrecursorKey_Purple, 10);
			addItemMorale(TechType.PrecursorKey_Orange, 20);
			addItemMorale(TechType.PrecursorKey_Blue, 30);
			addItemMorale(TechType.PrecursorKey_Red, 40);
			addItemMorale(TechType.PrecursorKey_White, 50);

			addItemMorale(TechType.Tank, 10);
			addItemMorale(TechType.DoubleTank, 20);
			addItemMorale(TechType.HighCapacityTank, 30);
			addItemMorale(TechType.LaserCutter, 25);
			addItemMorale(TechType.Rebreather, 10);
			addItemMorale(TechType.Seaglide, 15);
			addItemMorale(TechType.Fins, 5);

			addItemMorale(TechType.BaseRoom, 40);
			addItemMorale(TechType.BaseMoonpool, 25);
			addItemMorale(TechType.BaseFiltrationMachine, 20);
			addItemMorale(TechType.BaseMapRoom, 10);
			addItemMorale(TechType.BaseNuclearReactor, 30);
			addItemMorale(TechType.Workbench, 25);
			addItemMorale(C2CItems.processor, 50);
			addItemMorale(AqueousEngineeringMod.grinderBlock, 25);

			addItemMorale(TechType.HatchingEnzymes, 50);
		}

		public void addItemMorale(ModPrefab pfb, float amt) {
			addItemMorale(pfb.TechType, amt);
		}

		public void addItemMorale(TechType tt, float amt) {
			FirstObtainmentSystem.instance.registerEvent(tt, () => this.shiftMorale(amt));
		}

		public void registerStoryGoalEffect(string goal, float effect) {
			if (goalMorale.ContainsKey(goal))
				throw new Exception("Goal '" + goal + "' already has a morale effect (" + instance.goalMorale[goal] + ")!");
			goalMorale[goal] = effect;
		}

		public AmbientMoraleInfluence registerBiomeEffect(BiomeBase bb) {
			AmbientMoraleInfluence amb = new AmbientMoraleInfluence();
			if (biomeEffect.ContainsKey(bb))
				amb = biomeEffect[bb];
			biomeEffect[bb] = amb;
			return amb;
		}

		public void registerLifepodMoraleShifts(string radioGoal, string pdaGoal, float strength = 1) {
			this.registerPersistentEffect(radioGoal, false, GENERIC_RADIO_MORALE_BOOST*strength, GENERIC_RADIO_MORALE_DURATION, pdaGoal, GENERIC_DEAD_LIFEPOD_MORALE_IMPACT* strength, GENERIC_DEAD_LIFEPOD_MORALE_DURATION);
		}

		public void registerBaselineAdjustment(ProgressionTrigger check, float force, float baseline) {
			this.registerPersistentEffect(ep => check.isReady(ep), force, baseline);
		}

		public void registerBaselineAdjustment(ProgressionTrigger check, float effect, bool isForce) {
			this.registerPersistentEffect(ep => check.isReady(ep) ? effect : 0, isForce);
		}

		public void registerPersistentEffect(string goal1, bool isForce, float effect1Initial, float fadeTime1, string goal2, float effect2Initial, float fadeTime2 = -1) {
			this.registerPersistentEffect(ep => {
				if (StoryGoalManager.main.IsGoalComplete(goal1)) {
					if (StoryGoalManager.main.IsGoalComplete(goal2)) {
						if (fadeTime2 < 0)
							return effect2Initial;
						float since = StoryHandler.instance.getTimeSince(goal2);
						return since >= fadeTime2 ? 0 : (float)MathUtil.linterpolate(since, 0, fadeTime2, effect2Initial, 0, true);
					}
					else {
						float since = StoryHandler.instance.getTimeSince(goal1);
						return since >= fadeTime1 ? 0 : (float)MathUtil.linterpolate(since, 0, fadeTime1, effect1Initial, 0, true);
					}
				}
				else {
					return 0;
				}
			}, isForce);
		}

		public void registerPersistentEffect(string goal, bool isForce, float initial, float fadeTime = -1) {
			this.registerPersistentEffect(ep => {
				if (StoryGoalManager.main.IsGoalComplete(goal)) {
					if (fadeTime < 0)
						return initial;
					float since = StoryHandler.instance.getTimeSince(goal);
					return since >= fadeTime ? 0 : (float)MathUtil.linterpolate(since, 0, fadeTime, initial, 0, true);
				}
				else {
					return 0;
				}
			}, isForce);
		}

		public void registerPermanentGoalEffect(string goal, float force, float baseline) {
			this.registerPersistentEffect(ep => StoryGoalManager.main.IsGoalComplete(goal), force, baseline);
		}

		public void registerPersistentEffect(Func<Player, float> effect, bool isForce) { //legacy call
			Predicate<Player> p = ep => Math.Abs(effect.Invoke(ep)) > 0.001F;
			registerPersistentEffect(new ConditionalMoraleInfluence(new MoraleInfluence(new CallbackValue<Player>(effect), isForce), p));
		}

		public void registerPersistentEffect(Predicate<Player> check, float force, float baseline) {
			registerPersistentEffect(check, new MoraleInfluence(force, baseline));
		}

		public void registerPersistentEffect(Predicate<Player> check, DynamicValue<Player> force, DynamicValue<Player> baseline) {
			registerPersistentEffect(check, new MoraleInfluence(force, baseline));
		}

		public void registerPersistentEffect(Predicate<Player> check, MoraleInfluence i) {
			registerPersistentEffect(new ConditionalMoraleInfluence(i, check));
		}

		public void registerPersistentEffect(ConditionalMoraleInfluence effect) {
			if (effect == null)
				throw new Exception("Invalid null effect!");
			persistentEffects.Add(effect);
		}

		private void onStoryGoal(string goal) {
			if (goalMorale.ContainsKey(goal))
				this.shiftMorale(goalMorale[goal]);

			switch (goal) {
				case StoryGoals.EXIT_POD5: { //exit pod, and this is where morale *actually* begins
					timeUntilMoraleAdoptsRealValue = 7.5F;
					break;
				}
				case "OnPlayRadioBounceBack": {
					MoraleEffect9999 e = Player.main.gameObject.EnsureComponent<MoraleEffect9999>();
					e.InvokeRepeating("trigger", 7.25F, 1);
					e.Invoke("firstFire", 6.5F);
					break;
				}
				case StoryGoals.SUNBEAM_DESTROY_NEAR:
				case StoryGoals.SUNBEAM_DESTROY_FAR: {
					MoraleSystem.instance.shiftMorale(200);
					bool witness = goal == StoryGoals.SUNBEAM_DESTROY_NEAR;
					MoraleDelaySunbeamDestroy e = Player.main.gameObject.EnsureComponent<MoraleDelaySunbeamDestroy>();
					e.onsite = witness;
					e.InvokeRepeating("triggerAnticipate", witness ? 28.25F : 18F, 0.05F);
					e.Invoke("triggerAnticipateRamp", witness ? 31 : 21F);
					e.Invoke("triggerAnticipateRamp2", witness ? 34 : 22.5F);
					e.Invoke("trigger", witness ? 36.75F : 27.5F);
					break;
				}
			}
		}

		class MoraleEffect9999 : MonoBehaviour {

			private int times = 0;

			void firstFire() {
				MoraleSystem.instance.shiftMorale(20);
			}

			void trigger() {
				times++;
				MoraleSystem.instance.shiftMorale(-6 * times);
				if (times >= 5) {
					CancelInvoke("trigger");
					UnityEngine.Object.Destroy(this, 0.5F);
				}
			}

		}

		class MoraleDelaySunbeamDestroy : MonoBehaviour {

			internal bool onsite;
			private bool rampedAnticipate = false;
			private bool rampedAnticipate2 = false;

			void triggerAnticipate() {
				float amt = rampedAnticipate2 ? -0.5F : (rampedAnticipate ? -0.25F : -0.075F); //at 20/s, this is therefore 1.5/s and 5/s
				MoraleSystem.instance.shiftMorale(onsite ? amt : amt*0.67F);
			}

			void triggerAnticipateRamp() {
				rampedAnticipate = true;
			}

			void triggerAnticipateRamp2() {
				rampedAnticipate2 = true;
			}

			void trigger() {
				StoryGoal.Execute("SunbeamDestroyed", Story.GoalType.Story);
				MoraleSystem.instance.shiftMorale(-200);
				CancelInvoke("triggerAnticipate");
				UnityEngine.Object.Destroy(this, 0.5F);
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
			if (!ep || !ep.liveMixin || GameModeUtils.currentEffectiveMode == GameModeOption.Creative || GameModeUtils.currentEffectiveMode == GameModeOption.Freedom || !DIHooks.isWorldLoaded() || /*ep.cinematicModeActive*/IntroVignette.isIntroActive || EscapePod.main.IsPlayingIntroCinematic() || LaunchRocket.launchStarted || StoryGoalManager.main.IsGoalComplete(StoryGoals.LAUNCH_ROCKET) || !MainCameraControl.main.enabled) {
				if (MainCameraControl.main.enabled) //do not re-set morale just because you are in a camera
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
						GameObject mb = wb.clone().setName("MoraleBar");
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

			EnvContext env = EnvContext.UNKNOWN;
			if (ep.currentSub) {
				this.resetPrawnTime();
				if (time - lastDecoCheckTime > 0.5F) {
					lastDecoCheckTime = time;
					currentRoom = BaseRoomSpecializationSystem.instance.getPlayerRoomType(ep, out currentDecoLevel, out float thresh);
				}

				bool cyclops = ep.currentSub.isCyclops;
				env = cyclops ? EnvContext.CYCLOPS : EnvContext.SEABASE;
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

				delta *= SeaToSeaMod.config.getFloat(C2CConfig.ConfigEntries.MORALESPEED);

				switch (currentRoom) {
					case BaseRoomSpecializationSystem.RoomTypes.LEISURE:
						delta += LEISURE_ROOM_CONSTANT_BONUS;
						break;
				}

				if (amb != null && cyclops)
					delta += amb.cyclops.moralePerSecondForce.getValue(ep);
			}
			else if (v) {
				//if (checkMoraleDebugFlag(MoraleDebugFlags.))
				//	SNUtil.writeToChat("Vehicle is "+v+" with vel "+v.useRigidbody.velocity.magnitude.ToString());
				if (v is SeaMoth sm) {
					env = EnvContext.SEAMOTH;
					this.resetPrawnTime();
					delta += (float)MathUtil.linterpolate(sm.useRigidbody.velocity.magnitude, 5, 20, 0, 0.75); //up to 0.75% per second if moving fast
					if (amb != null)
						delta += amb.seamoth.moralePerSecondForce.getValue(ep);
				}
				else if (v is Exosuit p) {
					env = EnvContext.PRAWN;
					float bonus = 0.5F*(1-(timeInPrawn/120F)); //"sense of limitless power" -> 0.5%/s fading over 120 seconds (total morale maximum: +30)
					if (bonus > 0)
						delta += bonus;
					if (amb != null)
						delta += amb.prawn.moralePerSecondForce.getValue(ep);
					prawnMoraleCooldown = PRAWN_MORALE_COOLDOWN;
					timeInPrawn += dT;
				}
				else {
					this.resetPrawnTime();
				}
			}
			else {
				env = EnvContext.FREEDIVE;
				if (amb != null && ep.IsSwimming() && !ep.precursorOutOfWater && ep.transform.position.y < -1)
					delta += amb.freeDiving.moralePerSecondForce.getValue(ep);
				this.resetPrawnTime();
			}


			if (time - lastBaselineCheckTime > 1F) {
				lastBaselineCheckTime = time;
				currentMoraleBaseline = INITIAL_MORALE_BASELINE;
				currentMoraleForce = 0;
				foreach (ConditionalMoraleInfluence amt in persistentEffects) {
					if (!amt.isActive(ep))
						continue;
					if (checkMoraleDebugFlag(MoraleDebugFlags.BASELINE))
						SNUtil.writeToChat("Morale effect "+amt);
					currentMoraleForce += amt.influence.moralePerSecondForce.getValue(ep);
					currentMoraleBaseline += amt.influence.moraleBaselineShift.getValue(ep);
				}
				if (amb != null) {
					switch (env) {
						case EnvContext.FREEDIVE:
						currentMoraleBaseline += amb.freeDiving.moraleBaselineShift.getValue(ep);
							break;
						case EnvContext.CYCLOPS:
						currentMoraleBaseline += amb.cyclops.moraleBaselineShift.getValue(ep);
							break;
						case EnvContext.SEAMOTH:
						currentMoraleBaseline += amb.seamoth.moraleBaselineShift.getValue(ep);
							break;
						case EnvContext.PRAWN:
						currentMoraleBaseline += amb.prawn.moraleBaselineShift.getValue(ep);
							break;
						case EnvContext.SEABASE:
							currentMoraleBaseline += amb.seabase.moraleBaselineShift.getValue(ep);
							break;
					}
				}

				if (checkMoraleDebugFlag(MoraleDebugFlags.CORE))
					SNUtil.writeToChat("Computed morale baseline " + currentMoraleBaseline.ToString("0.00") + " and force " + currentMoraleForce.ToString("0.00"));
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

			delta += currentMoraleForce;
			if (moralePercentage > currentMoraleBaseline) {
				delta -= MORALE_RESTORATION_RATE;
			}
			else if (moralePercentage < currentMoraleBaseline) {
				delta += MORALE_RESTORATION_RATE;
			}

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

		public class ConditionalMoraleInfluence {

			public readonly MoraleInfluence influence;
			public readonly Predicate<Player> isActive;

			public ConditionalMoraleInfluence(MoraleInfluence i, Predicate<Player> p) {
				influence = i;
				isActive = p;
			}

		}

		public class MoraleInfluence {

			public readonly DynamicValue<Player> moralePerSecondForce;
			public readonly DynamicValue<Player> moraleBaselineShift;

			public MoraleInfluence(DynamicValue<Player> f, DynamicValue<Player> s) {
				moralePerSecondForce = f;
				moraleBaselineShift = s;
			}

			public MoraleInfluence(float f, float s) : this(new ConstantValue<Player>(f), new ConstantValue<Player>(s)) {

			}

			public MoraleInfluence(float val, bool isForce) : this(new ConstantValue<Player>(val), isForce) {

			}

			public MoraleInfluence(DynamicValue<Player> val, bool isForce) : this(isForce ? val : new ConstantValue<Player>(0), isForce ? new ConstantValue<Player>(0) : val) {

			}

			public override string ToString() {
				return string.Format("F=({0}) B=({1})", moralePerSecondForce.ToString("0.00"), moraleBaselineShift.ToString("0.00"));
			}

		}

		public class AmbientMoraleInfluence {

			public MoraleInfluence freeDiving = NO_MORALE_EFFECT;
			public MoraleInfluence cyclops = NO_MORALE_EFFECT;
			public MoraleInfluence seamoth = NO_MORALE_EFFECT;
			public MoraleInfluence prawn = NO_MORALE_EFFECT;
			public MoraleInfluence seabase = NO_MORALE_EFFECT;

			public AmbientMoraleInfluence() {

			}

			public AmbientMoraleInfluence(MoraleInfluence m, MoraleInfluence c, MoraleInfluence v = null) : this(m, c, v, v) {

			}

			public AmbientMoraleInfluence(MoraleInfluence m, MoraleInfluence c, MoraleInfluence sm, MoraleInfluence p) {
				freeDiving = m;
				cyclops = c;
				seamoth = sm;
				prawn = p;
			}

			public AmbientMoraleInfluence setEnv(float force, float baseline) {
				foreach (EnvContext e in Enum.GetValues(typeof(EnvContext))) {
					setEnv(force, baseline, e);
				}
				return this;
			}

			public AmbientMoraleInfluence setEnv(float force, float baseline, EnvContext e) {
				MoraleInfluence inf = new MoraleInfluence(force, baseline);
				switch (e) {
					case EnvContext.FREEDIVE:
						freeDiving = inf;
						break;
					case EnvContext.CYCLOPS:
						cyclops = inf;
						break;
					case EnvContext.SEAMOTH:
						seamoth = inf;
						break;
					case EnvContext.PRAWN:
						prawn = inf;
						break;
					case EnvContext.SEABASE:
						seabase = inf;
						break;
				}
				return this;
			}

			public override string ToString() {
				return freeDiving + "/" + seamoth + "/" + prawn + "/" + cyclops+" / "+seabase;
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

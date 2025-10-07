using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;

using Story;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public class C2CProgression : IStoryGoalListener {

		public static readonly C2CProgression instance = new C2CProgression();

		internal readonly Vector3 pod12Location = new Vector3(1117, -268, 568);
		internal readonly Vector3 pod3Location = new Vector3(-33, -23, 409);
		internal readonly Vector3 pod6Location = new Vector3(363, -110, 309);
		internal readonly Vector3 dronePDACaveEntrance = new Vector3(-80, -79, 262);
		internal readonly Vector3 pod2Location = new Vector3(-489, -500, 1328);

		private readonly Dictionary<string, StoryGoal> locationGoals = new Dictionary<string, StoryGoal>() {
			{"OZZY_FORK_DEEP_ROOM", StoryHandler.instance.createLocationGoal(new Vector3(-645.6F, -102.7F, -16.2F), 12, "ozzyforkdeeproom")},
			{"UNDERISLANDS_BLOCKED_ROOM", StoryHandler.instance.createLocationGoal(new Vector3(-124.38F, -200.69F, 855F), 5, "underislandsblockedroom")},
			{"FLOATING_ARCH", StoryHandler.instance.createLocationGoal(new Vector3(-662.55F, 5.50F, -1064.35F), 25, "floatarch", vec => vec.y > 0 && vec.y < 22.5F)},
			{"PLANT_ALCOVE", StoryHandler.instance.createLocationGoal(new Vector3(375, 22, 870), 15, "islandalcove", vec => vec.y > 15 && vec.y < 30F)},
			{"MUSHTREE", StoryHandler.instance.createLocationGoal(new Vector3(-883.7F, -144, 591.4F), 25, "mushtree")},
			{"MUSHTREE_ARCH", StoryHandler.instance.createLocationGoal(new Vector3(-777.5F, -229.8F, 404.8F), 12, "musharch")},
			{"CRAG_ARCH", StoryHandler.instance.createLocationGoal(new Vector3(-90.2F, -287.4F, -1261.5F), 6, "cragarch")},
			{"KOOSH_ARCH", StoryHandler.instance.createLocationGoal(new Vector3(1344.8F, -309.2F, 730.7F), 8, "koosharch")},
			{"LR_ARCH", StoryHandler.instance.createLocationGoal(new Vector3(-914.7F, -621.2F, 1078.4F), 6, "lrarch")},
			{"SOUTH_GRASS_WRECK", StoryHandler.instance.createLocationGoal(new Vector3(-29.19F, -103.46F, -608.40F), 20, "southgrasswreck")},
			{"EAST_GRASS_WRECK", StoryHandler.instance.createLocationGoal(new Vector3(318.79F, -90.34F, 441.63F), 30, "eastgrasswreck")},
			{"LR_LAB", StoryHandler.instance.createLocationGoal(new Vector3(-1119.8F, -683.1F, -688.2F), 8, "lrlab")},
			{"SEE_GUN", StoryHandler.instance.createLocationGoal(new Vector3(402.3F, 19.7F, 1118.9F), 160, "see_gun")},
			{"SEE_ATP", StoryHandler.instance.createLocationGoal(WorldUtil.lavaCastleCenter, 80, "see_atp")},
			{"SPARSE_CACHE", StoryHandler.instance.createLocationGoal(new Vector3(-889.8F, -305.6F, -815.3F), 10, "sparse_cache")},
			{"DUNES_CACHE", StoryHandler.instance.createLocationGoal(new Vector3(-1224.6F, -393.3F, 1078.9F), 18, "dunes_cache")},
			{"NBKELP_CACHE", StoryHandler.instance.createLocationGoal(new Vector3(-624.0F, -558.7F, 1485.9F), 18, "nbkelp_cache")},
			{"FLOATISLAND_DEGASI", StoryHandler.instance.createLocationGoal(WorldUtil.DEGASI_FLOATING_BASE, 50, "floatisland_degasi")},
			{"JELLY_DEGASI", StoryHandler.instance.createLocationGoal(WorldUtil.DEGASI_JELLY_BASE, 50, "jelly_degasi")},
			{"DGR_DEGASI", StoryHandler.instance.createLocationGoal(WorldUtil.DEGASI_DGR_BASE, 75, "dgr_degasi")},
		};

		internal static readonly string METEOR_GOAL = "meteorhit";
		internal static readonly string TUNGSTEN_GOAL = "filtertung";
		private static readonly HashSet<string> mountainPodVisibilityTriggers = new HashSet<string>{
			"mountainpodearly",
			"mountainpodlate",
			"mountaincave",
			"islandpda",
			"islandcave",
		};
		internal static readonly string MOUNTAIN_POD_ENTRY_VISIBILITY_GOAL = "mountainPodEntriesVisible";

		private readonly Vector3[] seacrownCaveEntrances = new Vector3[]{
			new Vector3(279, -140, 288),//new Vector3(300, -120, 288)/**0.67F+pod6Location*0.33F*/,
	    	new Vector3(-621, -130, -190),//new Vector3(-672, -100, -176),
	    	//new Vector3(-502, -80, -102), //empty in vanilla, and right by pod 17
	    };

		internal readonly Vector3[] bkelpNestBumps = new Vector3[]{
			new Vector3(-847.46F, -530.82F, 1273.73F),
			new Vector3(-863.82F, -532.87F, 1302.29F),
			new Vector3(-841.12F, -535.97F, 1304.40F),
		};

		private float lastDunesEntry = -1;

		private readonly HashSet<TechType> gatedTechnologies = new HashSet<TechType>();
		private readonly HashSet<string> requiredProgression = new HashSet<string>();

		private readonly List<TechType> pipeRoomTechs = new List<TechType>();

		private C2CProgression() {
			StoryHandler.instance.addListener(this);

			StoryHandler.instance.registerTrigger(new StoryTrigger(StoryGoals.AURORA_FIX), new DelayedProgressionEffect(VoidSpikesBiome.instance.fireRadio, VoidSpikesBiome.instance.isRadioFired, 0.00003F));
			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.PrecursorKey_Orange), new DelayedStoryEffect(SeaToSeaMod.crashMesaRadio, 0.00004F));
			StoryHandler.instance.registerTrigger(new ProgressionTrigger(ep => ep.GetVehicle() is SeaMoth), new DelayedProgressionEffect(SeaToSeaMod.treaderSignal.fireRadio, SeaToSeaMod.treaderSignal.isRadioFired, 0.000015F));

			StoryGoal pod12Radio = new StoryGoal(StoryGoals.POD12RADIO, Story.GoalType.Radio, 0);
			DelayedStoryEffect ds = new DelayedStoryEffect(pod12Radio, 0.00008F);
			StoryHandler.instance.registerTrigger(new StoryTrigger(StoryGoals.SUNBEAM_DESTROY_START), ds);
			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.BaseNuclearReactor), ds);
			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.HighCapacityTank), ds);
			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.PrecursorKey_Purple), ds);
			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.BaseUpgradeConsole), ds);
			StoryHandler.instance.registerTrigger(new TechTrigger(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType), ds);
			StoryHandler.instance.registerTrigger(new EncylopediaTrigger("SnakeMushroom"), ds);

			this.addPDAPrompt(PDAMessages.Messages.KooshCavePrompt, ep => Vector3.Distance(pod12Location, ep.transform.position) <= 75);
			this.addPDAPrompt(PDAMessages.Messages.RedGrassCavePrompt, this.isNearSeacrownCave);
			this.addPDAPrompt(PDAMessages.Messages.UnderwaterIslandsPrompt, this.isInUnderwaterIslands);
			this.addPDAPrompt(PDAMessages.Messages.KelpCavePrompt, ep => this.isNearKelpCave(ep) && !this.isJustStarting(ep));
			this.addPDAPrompt(PDAMessages.Messages.KelpCavePromptLate, this.hasMissedKelpCavePromptLate);
			this.addPDAPrompt(PDAMessages.Messages.BloodKelpNestPrompt, ep => Vector3.Distance(pod2Location, ep.transform.position) <= 100);
			this.addPDAPrompt(PDAMessages.Messages.TrailerBasePrompt, ep => VanillaBiomes.CRASH.isInBiome(ep.transform.position) && WorldUtil.isInsideAurora2D(ep.transform.position, 200));
			/*
			PDAPrompt kelpLate = addPDAPrompt(PDAMessages.Messages.KelpCavePromptLate, new TechTrigger(TechType.HighCapacityTank), 0.0001F);
			addPDAPrompt(kelpLate, new TechTrigger(TechType.StasisRifle));
			addPDAPrompt(kelpLate, new TechTrigger(TechType.BaseMoonpool));
			*/
			StoryHandler.instance.registerTrigger(new PDAPromptCondition(new ProgressionTrigger(this.doDunesCheck)), new DunesPrompt());
			StoryHandler.instance.registerTrigger(new PDAPromptCondition(new StoryTrigger(METEOR_GOAL)), new MeteorPrompt());

			this.addPDAPrompt(PDAMessages.Messages.FollowRadioPrompt, this.hasMissedRadioSignals);

			StoryHandler.instance.registerTrigger(new ProgressionTrigger(this.canUnlockEnzy42Recipe), new TechUnlockEffect(Bioprocessor.getByOutput(CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42).TechType).outputDelegate.TechType, 1, 6));
			StoryHandler.instance.registerTrigger(new ProgressionTrigger(this.canUnlockEnzy42Recipe), new TechUnlockEffect(CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42).TechType, 1, 6));

			StoryHandler.instance.registerTrigger(new ProgressionTrigger(this.canSunbeamCountdownBegin), new DelayedStoryEffect(SeaToSeaMod.sunbeamCountdownTrigger, 0.001F, 90));

			foreach (StoryGoal g in locationGoals.Values) {
				StoryHandler.instance.registerTickedGoal(g);
			}

			//StoryHandler.instance.registerChainedRedirect("PrecusorPrisonAquariumIncubatorActive", null); //deregister

			pipeRoomTechs.Add(TechType.PrecursorPipeRoomIncomingPipe);
			pipeRoomTechs.Add(TechType.PrecursorPipeRoomOutgoingPipe);
			pipeRoomTechs.Add(SeaToSeaMod.prisonPipeRoomTank);

			gatedTechnologies.Add(TechType.Kyanite);
			gatedTechnologies.Add(TechType.Sulphur);
			gatedTechnologies.Add(TechType.Nickel);
			gatedTechnologies.Add(TechType.MercuryOre);
			gatedTechnologies.Add(TechType.JellyPlant);
			gatedTechnologies.Add(TechType.BloodOil);
			gatedTechnologies.Add(TechType.AramidFibers);
			gatedTechnologies.Add(TechType.WhiteMushroom);
			gatedTechnologies.Add(TechType.SeaCrown);
			gatedTechnologies.Add(TechType.Aerogel);
			gatedTechnologies.Add(TechType.Seamoth);
			gatedTechnologies.Add(TechType.Cyclops);
			gatedTechnologies.Add(TechType.Exosuit);
			gatedTechnologies.Add(TechType.Benzene);
			gatedTechnologies.Add(TechType.HydrochloricAcid);
			gatedTechnologies.Add(TechType.Polyaniline);
			gatedTechnologies.Add(TechType.ExosuitDrillArmModule);
			gatedTechnologies.Add(TechType.ExoHullModule1);
			gatedTechnologies.Add(TechType.ExoHullModule2);
			gatedTechnologies.Add(TechType.VehicleHullModule2);
			gatedTechnologies.Add(TechType.VehicleHullModule3);
			gatedTechnologies.Add(TechType.SeamothElectricalDefense);
			gatedTechnologies.Add(TechType.CyclopsHullModule2);
			gatedTechnologies.Add(TechType.CyclopsHullModule3);
			gatedTechnologies.Add(TechType.CyclopsThermalReactorModule);
			gatedTechnologies.Add(TechType.CyclopsFireSuppressionModule);
			gatedTechnologies.Add(TechType.CyclopsShieldModule);
			gatedTechnologies.Add(TechType.StasisRifle);
			gatedTechnologies.Add(TechType.LaserCutter);
			gatedTechnologies.Add(TechType.ReinforcedDiveSuit);
			gatedTechnologies.Add(TechType.ReinforcedGloves);
			gatedTechnologies.Add(TechType.PrecursorIonCrystal);
			gatedTechnologies.Add(TechType.PrecursorIonBattery);
			gatedTechnologies.Add(TechType.PrecursorIonPowerCell);
			gatedTechnologies.Add(TechType.PrecursorKey_Blue);
			gatedTechnologies.Add(TechType.PrecursorKey_Red);
			gatedTechnologies.Add(TechType.PrecursorKey_White);
			gatedTechnologies.Add(TechType.PrecursorKey_Orange);
			gatedTechnologies.Add(TechType.PrecursorKey_Purple);
			gatedTechnologies.Add(TechType.HeatBlade);
			gatedTechnologies.Add(TechType.ReactorRod);

			//requiredProgression.Add();
		}

		public IEnumerable<TechType> getGatedTechnologies() {
			return new ReadOnlyCollection<TechType>(gatedTechnologies.ToList());
		}

		public StoryGoal getLocationGoal(string key) {
			return !locationGoals.ContainsKey(key) ? throw new Exception("No such location goal '" + key + "'") : locationGoals[key];
		}

		private bool canSunbeamCountdownBegin(Player ep) {
			return StoryGoalManager.main.completedGoals.Contains(StoryGoals.getRadioPlayGoal(StoryGoals.SUNBEAM_FILLER)) && ep.GetVehicle() is SeaMoth;
		}

		private bool canUnlockEnzy42Recipe(Player ep) {
			return PDAEncyclopedia.entries.ContainsKey("HeroPeeper") && KnownTech.knownTech.Contains(C2CItems.processor.TechType);
		}

		private bool hasMissedRadioSignals(Player ep) {
			bool late = KnownTech.knownTech.Contains(TechType.StasisRifle) || KnownTech.knownTech.Contains(TechType.BaseMoonpool) || KnownTech.knownTech.Contains(TechType.HighCapacityTank);
			bool all = PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.RedGrassCavePrompt).key) && PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePrompt).key) && PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KooshCavePrompt).key);
			return late && !all;
		}

		private bool isNearKelpCave(Player ep) {
			return (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePromptLate).key) && Vector3.Distance(ep.transform.position, pod3Location) <= 80) || MathUtil.isPointInCylinder(dronePDACaveEntrance.setY(-40), ep.transform.position, 60, 40) || (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.FollowRadioPrompt).key) && Vector3.Distance(pod3Location, ep.transform.position) <= 60);
		}

		private bool isJustStarting(Player ep) {
			if (Inventory.main.equipment.GetTechTypeInSlot("Head") != TechType.None || KnownTech.knownTech.Contains(TechType.Seamoth) || KnownTech.knownTech.Contains(TechType.BaseMapRoom) || KnownTech.knownTech.Contains(TechType.BaseRoom))
				return false;
			if (StoryGoalManager.main.completedGoals.Contains(StoryGoals.getRadioPlayGoal(StoryGoals.POD3RADIO)))
				return false;
			//if (StoryGoalManager.main.completedGoals.Contains("Goal_Builder") || StoryGoalManager.main.completedGoals.Contains("Goal_Seaglide")) //craft build tool or seaglide
			//	return false;
			return true;
		}

		private bool hasMissedKelpCavePromptLate(Player ep) {
			if (!StoryGoalManager.main.completedGoals.Contains(StoryGoals.getRadioPlayGoal(StoryGoals.POD3RADIO)))
				return false;
			bool late1 = StoryGoalManager.main.completedGoals.Contains("Goal_LocationAuroraDriveEntry") || StoryGoalManager.main.completedGoals.Contains(StoryGoals.SUNBEAM_DESTROY_START);
			bool late2 = KnownTech.knownTech.Contains(TechType.Workbench) || KnownTech.knownTech.Contains(TechType.StasisRifle) || KnownTech.knownTech.Contains(TechType.BaseMoonpool) || KnownTech.knownTech.Contains(TechType.HighCapacityTank);
			return late1 && late2 && ep.GetBiomeString().ToLowerInvariant().Contains("safe") && !PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePrompt).key);
		}

		private bool isInUnderwaterIslands(Player ep) {
			return ep.transform.position.y <= -150 && (ep.transform.position - new Vector3(-112.3F, ep.transform.position.y, 990.3F)).magnitude <= 180 && ep.GetBiomeString().ToLowerInvariant().Contains("underwaterislands");
		}

		private bool isNearSeacrownCave(Player ep) {
			Vector3 pos = ep.transform.position;
			foreach (Vector3 vec in seacrownCaveEntrances) {
				if (pos.y <= vec.y && MathUtil.isPointInCylinder(vec, pos, 30, 10)) {
					return true;
				}
			}
			return false;
		}

		private PDAPrompt addPDAPrompt(PDAMessages.Messages m, Predicate<Player> condition, float ch = 0.01F) {
			return this.addPDAPrompt(m, new ProgressionTrigger(condition), ch);
		}

		private PDAPrompt addPDAPrompt(PDAMessages.Messages m, ProgressionTrigger pt, float ch = 0.01F) {
			PDAPrompt p = new PDAPrompt(m, ch);
			this.addPDAPrompt(p, pt);
			return p;
		}

		private void addPDAPrompt(PDAPrompt m, ProgressionTrigger pt) {
			StoryHandler.instance.registerTrigger(new PDAPromptCondition(pt), m);
		}

		private bool doDunesCheck(Player ep) {
			string biome = ep.GetBiomeString();
			if (biome != null && biome.ToLowerInvariant().Contains("dunes")) {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (lastDunesEntry < 0)
					lastDunesEntry = time;
				//SNUtil.writeToChat(lastDunesEntry+" > "+(time-lastDunesEntry));
				if (time - lastDunesEntry >= 90) { //in dunes for at least 90s
					return true;
				}
			}
			else {
				lastDunesEntry = -1;
			}
			return false;
		}

		public bool isRequiredProgressionComplete() {
			foreach (string s in requiredProgression) {
				if (!StoryGoalManager.main.IsGoalComplete(s))
					return false;
			}
			return true;
		}

		public void onScanComplete(PDAScanner.EntryData data) {
			if (pipeRoomTechs.Contains(data.key)) {
				foreach (TechType tt in pipeRoomTechs) {
					if (!PDAScanner.complete.Contains(tt))
						return;
				}
				SeaToSeaMod.enviroSimulation.unlock();
			}
		}

		public void onWorldLoaded() {
			foreach (string s in mountainPodVisibilityTriggers) {
				if (StoryGoalManager.main.completedGoals.Contains(s)) {
					StoryGoal.Execute(MOUNTAIN_POD_ENTRY_VISIBILITY_GOAL, Story.GoalType.Story);
				}
			}
		}

		public void NotifyGoalComplete(string key) {
			if (key.StartsWith("OnPlay", StringComparison.InvariantCultureIgnoreCase)) {
				if (key.Contains(SeaToSeaMod.treaderSignal.storyGate)) {
					SeaToSeaMod.treaderSignal.activate(20);
				}
				else if (key.Contains(VoidSpikesBiome.instance.getSignalKey())) {
					VoidSpikesBiome.instance.activateSignal();
				}
				else if (key.Contains(SeaToSeaMod.crashMesaRadio.key)) {
					Player.main.gameObject.EnsureComponent<DelayedPromptsCallback>().Invoke("triggerCrashMesa", 25);
				}
			}
			else if (key == PDAManager.getPage("voidpod").id) { //id is pda page story key
				SeaToSeaMod.voidSpikeDirectionHint.activate(4);
			}
			else if (key == SeaToSeaMod.auroraTerminal.key) {
				PDAManager.getPage("auroraringterminalinfo").unlock(false);
			}
			else if (mountainPodVisibilityTriggers.Contains(key)) {
				StoryGoal.Execute(MOUNTAIN_POD_ENTRY_VISIBILITY_GOAL, Story.GoalType.Story);
			}
			else {
				switch (key) {
					case StoryGoals.SUNBEAM_DESTROY_START:
						Player.main.gameObject.EnsureComponent<AvoliteSpawner.TriggerCallback>().Invoke("trigger", 39);
						break;
					case StoryGoals.MAIDA_SEAMOTH_LOG:
						Player.main.gameObject.EnsureComponent<DelayedPromptsCallback>().Invoke("triggerJellySeamothDepth", 5);
						break;
					case "drfwarperheat":
						KnownTech.Add(C2CItems.cyclopsHeat.TechType);
						break;
					case "stepcaveterminal":
						KnownTech.Add(CraftingItems.getItem(CraftingItems.Items.MicroFilter).TechType);
						//SNUtil.triggerTechPopup(CraftingItems.getItem(CraftingItems.Items.MicroFilter).TechType);
						break;
					case "prisonpipeterminal": //removed, scanning the four tanks is now the trigger
											   //KnownTech.Add(CraftingItems.getItem(CraftingItems.Items.FluidPump).TechType);
						break;
					case "prisoneggterminal":
						//KnownTech.Add(C2CItems.incubatorInjector.TechType);
						break;
					case "prisonhighterminal": //removed
											   //KnownTech.Add(TechType.HatchingEnzymes);						
						break;
				}
			}
		}

		internal bool canTriggerPDAPrompt(Player ep) {
			return SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.PROMPTS) && (ep.IsSwimming() || Mathf.Abs(ep.transform.position.y) <= 1 || ep.GetVehicle() != null) && ep.currentSub == null && !ep.currentEscapePod && !ep.precursorOutOfWater && !WorldUtil.isPrecursorBiome(ep.transform.position);
		}

		public bool isTechGated(TechType tt) {
			if (gatedTechnologies.Contains(tt))
				return true;
			Spawnable s = ItemRegistry.instance.getItem(tt);
			return s is DIPrefab && ((DIPrefab)s).getOwnerMod() == SeaToSeaMod.modDLL;
		}

		public static void onSeamothDepthChit() {
			if (StoryGoalManager.main.IsGoalComplete("seamothdepthchit2")) {
				StoryGoal.Execute("seamothdepthchit3", Story.GoalType.Story);
				SNUtil.setBlueprintUnlockProgress(SeaToSeaMod.seamothDepthUnlockTracker, 3);
				KnownTech.Add(TechType.VehicleHullModule1);
				SNUtil.triggerTechPopup(TechType.VehicleHullModule1);
			}
			else if (StoryGoalManager.main.IsGoalComplete("seamothdepthchit1")) {
				StoryGoal.Execute("seamothdepthchit2", Story.GoalType.Story);
				SNUtil.writeToChat("2/3 Data Entries Recovered");
				SNUtil.setBlueprintUnlockProgress(SeaToSeaMod.seamothDepthUnlockTracker, 2);
			}
			else {
				StoryGoal.Execute("seamothdepthchit1", Story.GoalType.Story);
				SNUtil.writeToChat("1/3 Data Entries Recovered");
				SNUtil.setBlueprintUnlockProgress(SeaToSeaMod.seamothDepthUnlockTracker, 1);
			}
		}

		public static bool isSeamothDepth1UnlockedLegitimately() {
			return StoryGoalManager.main.IsGoalComplete("seamothdepthchit2"); //2 for now because 3 did not exist at time of creation
		}

		public static void spawnPOIMarker(string id, Vector3 at) {
			if (StoryGoalManager.main.IsGoalComplete(id))
				return;
			DIMod.areaOfInterestMarker.spawnGenericSignalHolder(at);
			StoryGoal.Execute(id, Story.GoalType.Story);
		}
	}

	internal class PDAPromptCondition : ProgressionTrigger {

		private readonly ProgressionTrigger baseline;

		public PDAPromptCondition(ProgressionTrigger p) : base(ep => C2CProgression.instance.canTriggerPDAPrompt(ep) && p.isReady(ep)) {
			baseline = p;
		}

		public override string ToString() {
			return "Free-swimming " + baseline;
		}

	}

	internal class PDAPrompt : DelayedProgressionEffect {

		private readonly PDAMessages.Messages prompt;

		public PDAPrompt(PDAMessages.Messages m, float f) : base(() => PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(m).key), () => PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(m).key), f) {
			prompt = m;
		}

		public override string ToString() {
			return "PDA Prompt " + prompt;
		}

	}

	internal class DunesPrompt : DelayedProgressionEffect {

		private static readonly PDAManager.PDAPage page = PDAManager.getPage("dunearchhint");

		public DunesPrompt() : base(() => { PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.DuneArchPrompt).key); page.unlock(false); }, () => PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.DuneArchPrompt).key), 0.006F) {

		}

		public override string ToString() {
			return "Dunes Prompt";
		}

	}

	internal class MeteorPrompt : DelayedProgressionEffect {

		public MeteorPrompt() : base(() => { PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.MeteorPrompt).key); }, () => PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.MeteorPrompt).key), 100F, 2) {

		}

		public override string ToString() {
			return "Meteor Prompt";
		}

	}

	class DelayedPromptsCallback : MonoBehaviour {

		void triggerCrashMesa() {
			SoundManager.playSound("event:/tools/scanner/new_encyclopediea"); //triple-click
			SoundManager.playSound("event:/player/story/RadioShallows22NoSignalAlt"); //"signal coordinates corrupted"
			PDAManager.getPage("crashmesahint").unlock(false);
		}

		void triggerSanctuary() {
			if (!PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.SanctuaryPrompt).key)) {
				PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.SanctuaryPrompt).key);
				SeaToSeaMod.sanctuaryDirectionHint.activate(12);
			}
		}

		void triggerJellySeamothDepth() {
			if (Player.main.GetPDA().isOpen) {
				this.Invoke("triggerJellySeamothDepth", 2);
				return;
			}
			if (!PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.JellySeamothDepthPrompt).key)) {
				PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.JellySeamothDepthPrompt).key);
			}
		}

	}

}

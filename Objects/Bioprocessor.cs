using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using FMOD;
using FMOD.Studio;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	public class Bioprocessor : CustomMachine<BioprocessorLogic> {

		internal static readonly Dictionary<TechType, BioRecipe> recipes = new Dictionary<TechType, BioRecipe>();
		private static readonly Dictionary<TechType, DuplicateRecipeDelegate> delegates = new Dictionary<TechType, DuplicateRecipeDelegate>();

		internal static readonly Arrow leftArrow = new Arrow("arrowL", "", "", "");
		internal static readonly Arrow rightArrow = new Arrow("arrowR", "", "", "");
		internal static readonly Arrow returnArrow = new Arrow("arrowRet", "", "", "");
		internal static readonly Arrow spacer = new Arrow("spacer", "", "", "");
		internal static readonly NotFabricable sparklePeeperDisplay = new NotFabricable(SeaToSeaMod.miscLocale.getEntry("EnzymePeeperDisplay"), CraftData.GetClassIdForTechType(TechType.Peeper));

		internal static readonly float POWER_COST_IDLE = 0.5F; //per second; was 1.5 then 2.5
															   //internal static readonly float POWER_COST_ACTIVE = 18.0F; //per second

		private static readonly string MACHINE_GO_NAME = "MachineModel";

		internal static Sound workingSoundReference;
		internal static readonly SoundManager.SoundData workingSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "bioprocwork", "Sounds/bioproc-loop.ogg", SoundManager.soundMode3D, s => {workingSoundReference = s; SoundManager.setup3D(s, 16); SoundManager.setLooping(s);});

		private static TechCategory bioprocCategory = TechCategory.Misc;

		static Bioprocessor() {
			leftArrow.Patch();
			rightArrow.Patch();
			returnArrow.Patch();
			spacer.Patch();
			sparklePeeperDisplay.sprite = SpriteManager.Get(TechType.Peeper);
			sparklePeeperDisplay.Patch();
			bioprocCategory = TechCategoryHandler.Main.AddTechCategory("bioprocessor", "Bioprocessor");
			TechCategoryHandler.Main.TryRegisterTechCategoryToTechGroup(TechGroup.Resources, bioprocCategory);
			/*
			ItemRegistry.instance.addListener(item => {
			    if (item.ClassID == "MiniPoop") {
					BioRecipe rec = getRecipe(TechType.SeaTreaderPoop);
					addRecipe(item.TechType, CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, rec.enzyCount, rec.processTime, rec.totalEnergyCost, rec.inputCount*4, rec.outputCount);                   	
			    }
			});*/

		}

		public static void addRecipes() {
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
			addRecipe(new TypeInput(TechType.CreepvineSeedCluster), TechType.Lubricant, 2, 10, 120, 1, 2);
			addRecipe(new TypeInput(TechType.AcidMushroom), CraftingItems.getItem(CraftingItems.Items.WeakAcid).TechType, 2, 2, 30, 5, hard ? 2 : 3);
			addRecipe(new TypeInput(TechType.WhiteMushroom), TechType.HydrochloricAcid, 6, 20, 400, 9, 2);
			addRecipe(new TypeInput(TechType.BloodOil), TechType.Benzene, 5, 45, 800, 4);
			addRecipe(new TypeInput(C2CItems.alkali), CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 4, 30, 600, 5);
			addRecipe(new TypeInput(TechType.GasPod), CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType, 1, 15, 240, 3, 5);
			addRecipe(new TypeInput(TechType.SnakeMushroomSpore), CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2, 90, hard ? 2500 : 1500, 2);
			addRecipe(new TypeInput(TechType.HatchingEnzymes), CraftingItems.getItem(CraftingItems.Items.SmartPolymer).TechType, 6, 120, hard ? 4500 : 3000, 2);
			addRecipe(new TypeInput(TechType.SeaTreaderPoop), CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, 1, 5, 120, 1, 4, true);
			addRecipe(new TypeInput(C2CItems.kelp), CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType, 2, 15, hard ? 750 : 180, 3, 8);
			addRecipe(new SparklePeeperInput(), CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42).TechType, 1, 45, hard ? 1000 : 200, 2, 1, true);
			//addRecipe(new TypeInput(SeaToSeaMod.geogel), CraftingItems.getItem(CraftingItems.Items.FilteredGeoGel).TechType, 3, 60, 250, 1, 1, true);
		}

		public static void addRecipe(BioInput inp, TechType o, int enzy, float secs, float energy, int inamt = 1, int outamt = 1, bool preventUnlock = false) {
			if (inp == null || inp.getBaseType() == TechType.None)
				throw new Exception("You may not register a recipe using null!");
			if (o == TechType.None)
				throw new Exception("You may not register a recipe making null!");
			BioRecipe r = new BioRecipe(enzy, secs, energy, inp, o);
			recipes[r.inputItem.getBaseType()] = r;
			r.inputCount = inamt;
			r.outputCount = outamt;
			createRecipeDelegate(r, preventUnlock);
		}

		private static TechType createRecipeDelegate(BioRecipe r, bool preventUnlock = false) {
			BasicCraftingItem to = CraftingItems.getItemByTech(r.outputItem);
			string rec = " (x"+r.outputCount+")\n("+Mathf.CeilToInt(r.totalEnergyCost)+" Power)";
			DuplicateRecipeDelegate item = to == null ? new DuplicateRecipeDelegate(r.outputItem, rec) : new DuplicateRecipeDelegate(to, rec);
			item.category = bioprocCategory;
			item.group = TechGroup.Resources;
			item.unlock = preventUnlock ? TechType.Unobtanium : r.inputItem.getBaseType();
			item.ownerMod = SeaToSeaMod.modDLL;
			item.allowUnlockPopups = true;
			if (item.sprite == null && to != null)
				item.sprite = to.getIcon();
			if (item.sprite == null)
				item.sprite = SpriteManager.Get(r.outputItem);
			item.Patch();
			RecipeUtil.addRecipe(item.TechType, TechGroup.Resources, bioprocCategory, null, r.outputCount, CraftTree.Type.None);
			//RecipeUtil.addIngredient(item.TechType, SeaToSeaMod.processor.TechType, 1);
			//RecipeUtil.addIngredient(item.TechType, leftArrow.TechType, 1);
			RecipeUtil.addIngredient(item.TechType, r.inputItem.getIngredientDisplay(), r.inputCount);
			RecipeUtil.addIngredient(item.TechType, CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType, r.enzyCount);
			//RecipeUtil.addIngredient(item.TechType, spacer.TechType, 1);
			//RecipeUtil.addIngredient(item.TechType, spacer.TechType, 1);
			//RecipeUtil.addIngredient(item.TechType, returnArrow.TechType, 1);
			//RecipeUtil.addIngredient(item.TechType, rightArrow.TechType, 1);
			//RecipeUtil.addIngredient(item.TechType, r.outputItem, r.outputCount);
			delegates[r.inputItem.getBaseType()] = item;
			r.outputDelegate = item;
			return item.TechType;
		}

		public static BioRecipe getRecipe(TechType input) {
			return recipes[input];
		}

		public static TechType getRecipeReferenceItem(TechType input) {
			return delegates[input].TechType;
		}

		public static BioRecipe getByOutput(TechType output) {
			foreach (BioRecipe r in recipes.Values) {
				if (r.outputItem == output || (r.outputDelegate != null && r.outputDelegate.TechType == output)) {
					return r;
				}
			}
			return null;
		}

		public Bioprocessor(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "6d71afaa-09b6-44d3-ba2d-66644ffe6a99") {
			this.addIngredient(TechType.TitaniumIngot, 1);
			this.addIngredient(TechType.Magnetite, 4);
			this.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 6);
			this.addIngredient(TechType.CopperWire, 1);
			this.addIngredient(TechType.EnameledGlass, 3);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}

		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			go.removeComponent<Aquarium>();
			go.removeChildObject("Bubbles");

			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			this.initializeStorageContainer(con, 6, 6);
			//con.prefabRoot = go;
			BioprocessorLogic lgc = go.GetComponent<BioprocessorLogic>();
			//lgc.storage = con;

			GameObject mdl = go.setModel("model", ObjectUtil.lookupPrefab("6ca93e93-5209-4c27-ba60-5f68f36a95fb").getChildObject("Starship_control_terminal_01"));
			mdl.transform.localEulerAngles = new Vector3(270, 0, 0);

			GameObject machineMdl = go.getChildObject(MACHINE_GO_NAME);
			go.removeChildObject("Submarine_engine_fragments_02");
			if (machineMdl == null) {
				machineMdl = ObjectUtil.createWorldObject("02dfa77b-5407-4474-90c6-fcb0003ecf2d", true, false).setName(MACHINE_GO_NAME);
				Vector3 vec = new Vector3(0, 1.41F, -0.975F);
				machineMdl.transform.localPosition = vec;
				machineMdl.transform.localScale = new Vector3(1, 1, 0.75F);
				machineMdl.transform.eulerAngles = new Vector3(90, 180, 0);
				machineMdl.transform.parent = go.transform;
			}

			foreach (Collider c in machineMdl.GetComponentsInChildren<Collider>()) {
				c.destroy(false);
			}
			SphereCollider cc = machineMdl.AddComponent<SphereCollider>();
			cc.radius = 0.85F;
			cc.center = new Vector3(0, 0, 0.2F);
			cc = machineMdl.AddComponent<SphereCollider>();
			cc.radius = 0.85F;
			cc.center = new Vector3(0, 0, 1F);

			Renderer r = machineMdl.GetComponentInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			RenderUtil.setEmissivity(r, 2);
			r.materials[0].EnableKeyword("MARMO_EMISSION");
			r.sharedMaterial.EnableKeyword("MARMO_EMISSION");
			r.materials[0].SetFloat("_Shininess", 8);
			r.materials[0].SetFloat("_Fresnel", 0.4F);

			lgc.soundLoop = go.EnsureComponent<FMOD_CustomLoopingEmitter>();
			lgc.soundLoop.asset = workingSound.asset;
			workingSoundReference.getLength(out uint millis, TIMEUNIT.MS);
			lgc.soundLoop.length = millis / 1000F;

			go.GetComponent<Constructable>().model = machineMdl;
			go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.3F, 0.4F, 1.3F);
			go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 0.5F, 0);

			go.removeChildObject("SubDamageSounds");

			//machineMdl.removeComponent<PrefabIdentifier>();
			//ChildObjectIdentifier coi = machineMdl.EnsureComponent<ChildObjectIdentifier>();
			//coi.classId = ClassID+"_mdl";

			setTerminalBox(go);
		}

		internal static void setTerminalBox(GameObject go) {
			BoxCollider box = go.getChildObject("Collider").EnsureComponent<BoxCollider>();
			box.center = new Vector3(0, 0.5F, 0);
			box.size = new Vector3(0.5F, 1.5F, 0.5F);
		}

	}

	public class BioprocessorLogic : DiscreteOperationalMachineLogic {

		private BioRecipe currentOperation;
		private int enzyRequired;
		private float nextEnzyTimeRemaining;

		private float operationCooldown = -1;

		private static readonly Color offlineColor = new Color(0.1F, 0.1F, 0.1F);
		private static readonly Color noRecipeColor = new Color(1, 0, 0);
		private static readonly Color recipeStalledColor = new Color(1, 1, 0);
		private static readonly Color workingColor = new Color(0, 1, 0);
		private static readonly Color completeColor = new Color(0.25F, 0.7F, 1);

		private float lastWorkingSound = -1;

		internal FMOD_CustomLoopingEmitter soundLoop;
		private bool setCollision;

		private float lastPowerCheck;

		private string powerErrorKey;

		private float powerConsumedThisRecipe;
		private float timeThisRecipe;

		void Start() {
			SNUtil.log("Reinitializing bioproc");
			C2CItems.processor.initializeMachine(gameObject);
			this.setEmissiveColor(new Color(0, 0, 1));
		}

		public override bool isWorking() {
			return currentOperation != null;
		}

		public float getRemainingTime() {
			return currentOperation == null ? 0 : ((enzyRequired - 1) * currentOperation.secondsPerEnzyme) + nextEnzyTimeRemaining;
		}

		public override float getProgressScalar() {
			float ret = this.getRemainingTime();
			return ret <= 0 ? 0 : 1 - (ret / currentOperation.processTime);
		}

		protected override void load(System.Xml.XmlElement data) {
			operationCooldown = (float)data.getFloat("cooldown", float.NaN);

			string rec = data.getProperty("recipe");
			TechType inp = string.IsNullOrEmpty(rec) ? TechType.None : SNUtil.getTechType(rec);
			currentOperation = inp == TechType.None ? null : Bioprocessor.recipes[inp];
			nextEnzyTimeRemaining = (float)data.getFloat("countdown", float.NaN);
			enzyRequired = data.getInt("required", 0, false);
		}

		protected override void save(System.Xml.XmlElement data) {
			data.addProperty("cooldown", operationCooldown);

			data.addProperty("recipe", currentOperation != null ? currentOperation.inputItem + "" : null);
			data.addProperty("countdown", nextEnzyTimeRemaining);
			data.addProperty("required", enzyRequired);
		}

		public static string defaultPowerErrorKey = null;

		private void checkPower() {
			powerErrorKey = defaultPowerErrorKey;
			PowerRelay pwr = sub.powerRelay;
			if (!pwr) {
				powerErrorKey = "NoBioprocBasePower";
				return;
			}
			int valid = 0;
			foreach (IPowerInterface src in pwr.inboundPowerSources) {
				if (src is SolarPanel) {

				}
				else if (src.GetMaxPower() >= 400) {
					valid++;
				}
			}
			if (valid < 2 || pwr.GetMaxPower() < (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 2500 : 1200)) {
				powerErrorKey = "WeakBioprocBasePower";
			}
		}

		public override string getErrorHover() {
			return powerErrorKey;
		}

		protected override Renderer[] findRenderers() {
			return new Renderer[] { gameObject.getChildObject("model").GetComponent<Renderer>() };
		}

		protected override void updateEntity(float seconds) {
			if (!storage) {
				this.setEmissiveColor(new Color(1, 0, 1)); //error
				return;
			}
			//SNUtil.writeToChat("I am ticking @ "+transform.position+": "+seconds);
			if (seconds <= 0)
				return;
			storage.container.isAllowedToRemove += (pp, vb) => currentOperation == null || (pp.GetTechType() != CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType && pp.GetTechType() != currentOperation.inputItem.getBaseType());
			storage.container.isAllowedToAdd += (pp, vb) => currentOperation == null || pp.GetTechType() == currentOperation.outputItem || pp.GetTechType() == CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType;
			if (!setCollision) {
				Bioprocessor.setTerminalBox(gameObject);
				setCollision = true;
			}

			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastPowerCheck >= 5) {
				this.checkPower();
				lastPowerCheck = time;
			}

			//soundLoop.attributes.position = transform.position.toFMODVector();

			if (operationCooldown > 0) {
				operationCooldown -= seconds;
				powerConsumedThisRecipe = 0;
				timeThisRecipe = 0;
			}
			else if (powerErrorKey == null && this.consumePower(Bioprocessor.POWER_COST_IDLE * seconds)) {
				powerConsumedThisRecipe += powerConsumedLastAttempt;
				this.setEmissiveColor(noRecipeColor);
				if (currentOperation != null) {
					//SNUtil.writeToChat("ticking recipe: "+currentOperation+", want "+(currentOperation.powerPerSecond-Bioprocessor.POWER_COST_IDLE)*seconds+" pwr");
					float drain = (currentOperation.powerPerSecond-Bioprocessor.POWER_COST_IDLE)*seconds;
					if (this.consumePower(drain)) {
						powerConsumedThisRecipe += drain;
						timeThisRecipe += seconds;
						IList<InventoryItem> kelp = storage.container.GetItems(CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType);
						bool hasKelp = kelp != null && kelp.Count > 0;
						//SNUtil.writeToChat("has kelp: "+(kelp == null ? 0 : kelp.Count));
						this.setEmissiveColor(recipeStalledColor);
						nextEnzyTimeRemaining -= seconds;
						//SNUtil.writeToChat("remaining: "+nextEnzyTimeRemaining);
						if (nextEnzyTimeRemaining <= 0) {
							IList<InventoryItem> enzy = storage.container.GetItems(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType);
							if (enzy != null && enzy.Count >= 1) {
								storage.forceRemoveItem(enzy[0]);
								enzyRequired--;
								SoundManager.playSoundAt(SoundManager.buildSound("event:/loot/pickup_lubricant"), gameObject.transform.position);
								this.setEmissiveColor(workingColor, cooldown: 1 + this.getOperationTime(currentOperation.secondsPerEnzyme));
							}
							else {
								this.setRecipe(null);
							}
							nextEnzyTimeRemaining = this.getOperationTime(currentOperation.secondsPerEnzyme);
							if (enzyRequired <= 0) {
								//SNUtil.writeToChat("try craft");
								IEnumerable<InventoryItem> ing = currentOperation.inputItem.getMatchingItems(storage);
								if (ing != null && ing.Count() >= currentOperation.inputCount) {
									//SNUtil.writeToChat("success");
									for (int i = 0; i < currentOperation.inputCount; i++) {
										InventoryItem ii = ing.ElementAt(0);
										SNUtil.log("Removing " + ii.item + " (" + ii.item.gameObject.GetInstanceID() + ") from bioproc inventory");
										storage.forceRemoveItem(ii); //list is updated in realtime
									}
									int n = currentOperation.outputCount;
									if (hasKelp) {
										n *= 2;
										storage.forceRemoveItem(kelp[0]); //list is updated in realtime
									}
									this.addItemToInventory(currentOperation.outputItem, n);
									string msg = prefab.FriendlyName+" crafted " + currentOperation.outputItem.AsString() + " x" + n+" in "+timeThisRecipe.ToString("0.00")+"s using a total of "+ powerConsumedThisRecipe.ToString("0.0")+" power";
									SNUtil.writeToChat(msg);
									msg += "\nPower cost factor: " + (powerConsumedThisRecipe / currentOperation.totalEnergyCost).ToString("0.00000") + "x";
									msg += "\nTime cost factor: " + (timeThisRecipe / currentOperation.processTime).ToString("0.00000") + "x";
									SNUtil.log(msg);
									this.setRecipe(null);
									this.resetEmissiveCooldown();
									this.setEmissiveColor(completeColor, cooldown: 4);
									SoundManager.playSoundAt(SoundManager.buildSound("event:/tools/knife/heat_hit"), gameObject.transform.position);
									operationCooldown = 2;
								}
								else {
									SNUtil.log("Bioprocessor shutdown due to invalid ingredients");
									this.abort(noRecipeColor);
								}
							}
						}
					}
					else {
						//SNUtil.writeToChat("Insufficient power - only had ");
						SNUtil.log("Bioprocessor shutdown due to insufficient power during operation; wanted " + drain + ", relay has " + sub.powerRelay.GetPower());
						this.abort(offlineColor);
					}
				}
				else {
					//SNUtil.writeToChat("Looking for recipe");
					IList<InventoryItem> enzy = storage.container.GetItems(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType);
					if (enzy != null && enzy.Count > 0) {
						foreach (BioRecipe r in Bioprocessor.recipes.Values) {
							if (this.canRunRecipe(storage, r, enzy.Count)) {
								//SNUtil.writeToChat("Found "+r);
								this.setRecipe(r);
								break;
							}
						}
					}
				}
			}
			else {
				this.setRecipe(null);
				//SNUtil.writeToChat("Insufficient power");
				this.setEmissiveColor(offlineColor);
				operationCooldown = 5;
			}
			if (currentOperation != null && time - lastWorkingSound >= soundLoop.length - 0.1F) {
				lastWorkingSound = time;
				//SNUtil.playSoundAt(SNUtil.getSound("event:/sub_module/workbench/working"), gameObject.transform.position);
				SoundManager.playSoundAt(Bioprocessor.workingSound, gameObject.transform.position);
			}
		}

		private float getOperationTime(float val) {
			if (hasBioprocessorUpgrade())
				val *= 0.67F;
			return val;
		}

		private void abort(Color c) {
			//SNUtil.writeToChat("aborting operation");
			if (currentOperation != null) {
				int n = currentOperation.enzyCount-enzyRequired;
				if (n > 0) {
					SNUtil.log("Refunding " + n + " enzymes");
					this.addItemToInventory(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType, n);
				}
			}
			this.setRecipe(null);
			this.resetEmissiveCooldown();
			operationCooldown = 10;
			this.setEmissiveColor(c, cooldown: 2);
		}

		private bool canRunRecipe(StorageContainer sc, BioRecipe r, int enzy) {
			if (!KnownTech.knownTech.Contains(r.outputItem))
				return false;
			IEnumerable<InventoryItem> ing = r.inputItem.getMatchingItems(sc);
			return ing != null && enzy >= r.enzyCount && ing.Count() >= r.inputCount;
		}

		private void setRecipe(BioRecipe r) {
			//SNUtil.writeToChat(r == null ? "null" : r.inputItem.AsString()+" > "+r.outputItem.AsString());
			bool had = currentOperation != null;
			bool has = r != null;
			currentOperation = r;
			enzyRequired = r != null ? r.enzyCount : -1;
			nextEnzyTimeRemaining = r != null ? this.getOperationTime(r.secondsPerEnzyme) : -1;
			this.setEmissiveColor(r == null ? noRecipeColor : recipeStalledColor);
			powerConsumedThisRecipe = 0;
			timeThisRecipe = 0;
			if (has != had) {
				SoundManager.playSoundAt(SoundManager.buildSound(r == null ? "event:/sub/seamoth/seamoth_light_off" : "event:/sub/seamoth/seamoth_light_on"), gameObject.transform.position);
#pragma warning disable CS0642
				if (has)
					;//soundLoop.Play();
				else
					;//soundLoop.Stop();
			}
		}
#pragma warning restore CS0642
		public static bool hasBioprocessorUpgrade() {
			return Story.StoryGoalManager.main.completedGoals.Contains(SeaToSeaMod.bioProcessorBoost.goal.key);
		}

	}

	public abstract class BioInput {

		public abstract bool isItemValid(Pickupable pp);

		public abstract TechType getBaseType();

		public virtual TechType getIngredientDisplay() {
			return this.getBaseType();
		}

		public IEnumerable<InventoryItem> getMatchingItems(StorageContainer sc) {
			IList<InventoryItem> li = sc.container.GetItems(this.getBaseType());
			return li == null ? null : li.Where(ii => this.isItemValid(ii.item));
		}

	}

	public sealed class TypeInput : BioInput {

		private readonly TechType type;

		internal TypeInput(BasicCustomPlant plant) : this(plant.seed.TechType) {

		}

		internal TypeInput(ModPrefab pfb) : this(pfb.TechType) {

		}

		internal TypeInput(TechType tt) {
			type = tt;
		}

		public override bool isItemValid(Pickupable pp) {
			return pp.GetTechType() == type;
		}

		public override TechType getBaseType() {
			return type;
		}

		public override string ToString() {
			return string.Format("[TypeInput Type={0}]", type);
		}


	}

	sealed class SparklePeeperInput : BioInput {

		public override bool isItemValid(Pickupable pp) {
			//SNUtil.writeToChat("Comparing against "+pp+" ("+(pp.GetComponent<Peeper>() ? pp.GetComponent<Peeper>().isHero.ToString() : "no")+")");
			return pp.GetTechType() == TechType.Peeper && pp.GetComponent<Peeper>().isHero;
		}

		public override TechType getBaseType() {
			return TechType.Peeper;
		}

		public override TechType getIngredientDisplay() {
			return Bioprocessor.sparklePeeperDisplay.TechType;
		}

		public override string ToString() {
			return "Sparkle Peeper";
		}

	}

	public class BioRecipe {

		public readonly BioInput inputItem;
		public readonly TechType outputItem;
		public readonly int enzyCount;
		public readonly float processTime;
		public readonly float totalEnergyCost;

		public readonly float secondsPerEnzyme;
		public readonly float powerPerSecond;

		internal DuplicateRecipeDelegate outputDelegate;

		internal int inputCount = 1;
		internal int outputCount = 1;

		internal BioRecipe(int s, float t, float e, BioInput inp, TechType o) {
			inputItem = inp;
			outputItem = o;
			enzyCount = s;
			processTime = t;
			if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
				e *= 2.5F;
			totalEnergyCost = e;
			secondsPerEnzyme = processTime / enzyCount;
			powerPerSecond = totalEnergyCost / processTime;
			if (powerPerSecond < Bioprocessor.POWER_COST_IDLE)
				throw new Exception("Recipe " + this + " uses too little energy!");
		}

		public int getInputCount() {
			return inputCount;
		}

		public int getOutputCount() {
			return outputCount;
		}

		public override string ToString() {
			return string.Format("[BioRecipe InputItem={0}, OutputItem={1}, enzyCount={2}, ProcessTime={3}, secondsPerEnzyme={4}, InputCount={5}, OutputCount={6}, totalEnergyCost={7}, powerPerSecond={8}]", inputItem, outputItem, enzyCount, processTime, secondsPerEnzyme, inputCount, outputCount, totalEnergyCost, powerPerSecond);
		}


	}
}

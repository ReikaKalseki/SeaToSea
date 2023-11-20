using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using FMOD;
using FMOD.Studio;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class Bioprocessor : CustomMachine<BioprocessorLogic> {
		
		internal static readonly Dictionary<TechType, BioRecipe> recipes = new Dictionary<TechType, BioRecipe>();
		internal static readonly Dictionary<TechType, DuplicateRecipeDelegate> delegates = new Dictionary<TechType, DuplicateRecipeDelegate>();
		
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
			addRecipe(new TypeInput(TechType.CreepvineSeedCluster), TechType.Lubricant, 2, 10, 120, 1, 2);
			addRecipe(new TypeInput(TechType.AcidMushroom), CraftingItems.getItem(CraftingItems.Items.WeakAcid).TechType, 2, 2, 30, 5, 2);
			addRecipe(new TypeInput(TechType.WhiteMushroom), TechType.HydrochloricAcid, 6, 20, 400, 9, 2);
			addRecipe(new TypeInput(TechType.BloodOil), TechType.Benzene, 5, 45, 800, 4);
			addRecipe(new TypeInput(C2CItems.alkali), CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 4, 30, 600, 5);
			addRecipe(new TypeInput(TechType.GasPod), CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType, 1, 15, 240, 3, 5);
			addRecipe(new TypeInput(TechType.SnakeMushroomSpore), CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2, 90, 1500, 2);
			addRecipe(new TypeInput(TechType.HatchingEnzymes), CraftingItems.getItem(CraftingItems.Items.SmartPolymer).TechType, 4, 120, 3000, 6);
			addRecipe(new TypeInput(TechType.SeaTreaderPoop), CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, 1, 5, 120, 1, 4, true);
			addRecipe(new TypeInput(C2CItems.kelp), CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType, 2, 15, 180, 3, 8);
			addRecipe(new SparklePeeperInput(), CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42).TechType, 1, 45, 200, 2, 1, true);
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
			string rec = " (x"+r.outputCount+")";
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
			RecipeUtil.addRecipe(item.TechType, TechGroup.Resources, bioprocCategory, r.outputCount, CraftTree.Type.None);
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
			addIngredient(TechType.TitaniumIngot, 1);
			addIngredient(TechType.Magnetite, 4);
			addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 6);
			addIngredient(TechType.CopperWire, 1);
			addIngredient(TechType.EnameledGlass, 3);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<Aquarium>(go);
			ObjectUtil.removeChildObject(go, "Bubbles");
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			initializeStorageContainer(con, 6, 6);
			//con.prefabRoot = go;
			BioprocessorLogic lgc = go.GetComponent<BioprocessorLogic>();
			//lgc.storage = con;
		 	
			GameObject mdl = RenderUtil.setModel(go, "model", ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("6ca93e93-5209-4c27-ba60-5f68f36a95fb"), "Starship_control_terminal_01"));
			mdl.transform.localEulerAngles = new Vector3(270, 0, 0);
			
		 	GameObject machineMdl = ObjectUtil.getChildObject(go, MACHINE_GO_NAME);
		 	ObjectUtil.removeChildObject(go, "Submarine_engine_fragments_02");
		 	if (machineMdl == null) {
			 	machineMdl = ObjectUtil.createWorldObject("02dfa77b-5407-4474-90c6-fcb0003ecf2d", true, false);
				machineMdl.name = MACHINE_GO_NAME;
			 	Vector3 vec = new Vector3(0, 1.41F, -0.975F);
			 	machineMdl.transform.localPosition = vec;
			 	machineMdl.transform.localScale = new Vector3(1, 1, 0.75F);
			 	machineMdl.transform.eulerAngles = new Vector3(90, 180, 0);
				machineMdl.transform.parent = go.transform;
		 	}
			
		 	foreach (Collider c in machineMdl.GetComponentsInChildren<Collider>()) {
				UnityEngine.Object.Destroy(c);
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
			lgc.mainRenderer = r;
			
			lgc.soundLoop = go.EnsureComponent<FMOD_CustomLoopingEmitter>();
			lgc.soundLoop.asset = workingSound.asset;
			uint millis;
			workingSoundReference.getLength(out millis, TIMEUNIT.MS);
			lgc.soundLoop.length = millis/1000F;
			
			go.GetComponent<Constructable>().model = machineMdl;
			go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.3F, 0.4F, 1.3F);
			go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 0.5F, 0);
			
			ObjectUtil.removeChildObject(go, "SubDamageSounds");
			
			//ObjectUtil.removeComponent<PrefabIdentifier>(machineMdl);
			//ChildObjectIdentifier coi = machineMdl.EnsureComponent<ChildObjectIdentifier>();
			//coi.classId = ClassID+"_mdl";
			
			setTerminalBox(go);
		}
		
		internal static void setTerminalBox(GameObject go) {
			BoxCollider box = ObjectUtil.getChildObject(go, "Collider").EnsureComponent<BoxCollider>();
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
		
		
		private float lastColorChange = -1;
		private float colorCooldown = -1;
		private Color emissiveColor;
		
		private float lastWorkingSound = -1;
		
		internal Renderer mainRenderer;
		internal FMOD_CustomLoopingEmitter soundLoop;
		private bool setCollision;
		
		void Start() {
			SNUtil.log("Reinitializing bioproc");
			SeaToSeaMod.processor.initializeMachine(gameObject);
			setEmissiveColor(new Color(0, 0, 1));
		}
		
		public override bool isWorking() {
			return currentOperation != null;
		}
		
		public float getRemainingTime() {
			if (currentOperation == null)
				return 0;
			return (enzyRequired-1)*currentOperation.secondsPerEnzyme+nextEnzyTimeRemaining;
		}
		
		public override float getProgressScalar() {
			float ret = getRemainingTime();
			if (ret <= 0)
				return 0;
			return 1-ret/currentOperation.processTime;
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
			
			data.addProperty("recipe", currentOperation != null ? currentOperation.inputItem+"" : null);
			data.addProperty("countdown", nextEnzyTimeRemaining);
			data.addProperty("required", enzyRequired);
		}
		
		protected override void updateEntity(float seconds) {
			if (mainRenderer == null)
				mainRenderer = ObjectUtil.getChildObject(gameObject, "model").GetComponent<Renderer>();
			StorageContainer sc = getStorage();
			if (!sc) {
				setEmissiveColor(new Color(1, 0, 1)); //error
				return;
			}
			//SNUtil.writeToChat("I am ticking @ "+transform.position+": "+seconds);
			if (seconds <= 0)
				return;
			if (!setCollision) {
				Bioprocessor.setTerminalBox(gameObject);
				setCollision = true;
			}
			
			//soundLoop.attributes.position = transform.position.toFMODVector();
			
			if (operationCooldown > 0) {
				operationCooldown -= seconds;
			}
			else if (consumePower(Bioprocessor.POWER_COST_IDLE*seconds)) {
				setEmissiveColor(noRecipeColor);
				if (currentOperation != null) {
					//SNUtil.writeToChat("ticking recipe: "+currentOperation+", want "+(currentOperation.powerPerSecond-Bioprocessor.POWER_COST_IDLE)*seconds+" pwr");
					float drain = (currentOperation.powerPerSecond-Bioprocessor.POWER_COST_IDLE)*seconds;
					if (consumePower(drain)) {
						IList<InventoryItem> kelp = sc.container.GetItems(CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType);
						bool hasKelp = kelp != null && kelp.Count > 0;
						//SNUtil.writeToChat("has kelp: "+(kelp == null ? 0 : kelp.Count));
						setEmissiveColor(recipeStalledColor);
						nextEnzyTimeRemaining -= seconds*(hasKelp ? 1.5F : 1);
						//SNUtil.writeToChat("remaining: "+nextEnzyTimeRemaining);
						if (nextEnzyTimeRemaining <= 0) {
							IList<InventoryItem> enzy = sc.container.GetItems(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType);
							if (enzy != null && enzy.Count >= 1) {
								InventoryUtil.removeItem(sc, enzy[0]);
								enzyRequired--;
								SoundManager.playSoundAt(SoundManager.buildSound("event:/loot/pickup_lubricant"), gameObject.transform.position);
								setEmissiveColor(workingColor, 1+getOperationTime(currentOperation.secondsPerEnzyme));
							}
							else {
								setRecipe(null);
							}
							nextEnzyTimeRemaining = getOperationTime(currentOperation.secondsPerEnzyme);
							if (enzyRequired <= 0) {
								//SNUtil.writeToChat("try craft");
								IEnumerable<InventoryItem> ing = currentOperation.inputItem.getMatchingItems(sc);
								if (ing != null && ing.Count() >= currentOperation.inputCount) {
									//SNUtil.writeToChat("success");
									for (int i = 0; i < currentOperation.inputCount; i++) {
										InventoryItem ii = ing.ElementAt(0);
										SNUtil.log("Removing "+ii.item+" ("+ii.item.gameObject.GetInstanceID()+") from bioproc inventory");
										InventoryUtil.removeItem(sc, ii); //list is updated in realtime
									}
									int n = currentOperation.outputCount;
									if (hasKelp) {
										n *= 2;
										InventoryUtil.removeItem(sc, kelp[0]);
									}
									addItemToInventory(currentOperation.outputItem, n);
									SNUtil.log("Bioprocessor crafted "+currentOperation.outputItem.AsString()+" x"+n);
									setRecipe(null);
									colorCooldown = -1;
									setEmissiveColor(completeColor, 4);
									SoundManager.playSoundAt(SoundManager.buildSound("event:/tools/knife/heat_hit"), gameObject.transform.position);
									operationCooldown = 2;
								}
								else {
									SNUtil.log("Bioprocessor shutdown due to invalid ingredients");
									abort(noRecipeColor);
								}
							}
						}
					}
					else {
						//SNUtil.writeToChat("Insufficient power - only had ");
						SNUtil.log("Bioprocessor shutdown due to insufficient power during operation; wanted "+drain+", relay has "+getSub().powerRelay.GetPower());
						abort(offlineColor);
					}
				}
				else {
					//SNUtil.writeToChat("Looking for recipe");
					foreach (BioRecipe r in Bioprocessor.recipes.Values) {
						if (canRunRecipe(sc, r)) {
							//SNUtil.writeToChat("Found "+r);
							setRecipe(r);
							break;
						}
					}
				}
			}
			else {
				setRecipe(null);
				//SNUtil.writeToChat("Insufficient power");
				setEmissiveColor(offlineColor);
				operationCooldown = 5;
			}
			if (currentOperation != null && DayNightCycle.main.timePassedAsFloat-lastWorkingSound >= soundLoop.length-0.1F) {
				lastWorkingSound = DayNightCycle.main.timePassedAsFloat;
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
					SNUtil.log("Refunding "+n+" enzymes");
					addItemToInventory(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType, n);
				}
			}
			setRecipe(null);
			colorCooldown = -1;
			operationCooldown = 10;
			setEmissiveColor(c, 2);
		}
		
		private void setEmissiveColor(Color c, float cooldown = -1) {
			if (mainRenderer == null)
				return;
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastColorChange < colorCooldown && cooldown < colorCooldown)
				return;
			emissiveColor = c;
			colorCooldown = cooldown;
			Material m = mainRenderer.materials[0];
			m.SetColor("_GlowColor", emissiveColor);
			lastColorChange = time;
		}
		
		private bool canRunRecipe(StorageContainer sc, BioRecipe r) {
			if (!KnownTech.knownTech.Contains(r.outputItem))
				return false;
			IEnumerable<InventoryItem> ing = r.inputItem.getMatchingItems(sc);
			IList<InventoryItem> enzy = sc.container.GetItems(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType);
			return ing != null && enzy != null && enzy.Count >= r.enzyCount && ing.Count() >= r.inputCount;
		}
		
		private void setRecipe(BioRecipe r) {
			//SNUtil.writeToChat(r == null ? "null" : r.inputItem.AsString()+" > "+r.outputItem.AsString());
			bool had = currentOperation != null;
			bool has = r != null;
			currentOperation = r;
			enzyRequired = r != null ? r.enzyCount : -1;
			nextEnzyTimeRemaining = r != null ? getOperationTime(r.secondsPerEnzyme) : -1;
			setEmissiveColor(r == null ? noRecipeColor : recipeStalledColor);
			if (has != had) {
				SoundManager.playSoundAt(SoundManager.buildSound(r == null ? "event:/sub/seamoth/seamoth_light_off" : "event:/sub/seamoth/seamoth_light_on"), gameObject.transform.position);
				if (has)
					;//soundLoop.Play();
				else
					;//soundLoop.Stop();
			}
		}
	    
	    public static bool hasBioprocessorUpgrade() {
	    	return Story.StoryGoalManager.main.completedGoals.Contains(SeaToSeaMod.bioProcessorBoost.goal.key);
	    }
		
	}
	
	public abstract class BioInput {
		
		public abstract bool isItemValid(Pickupable pp);
		
		public abstract TechType getBaseType();
		
		public virtual TechType getIngredientDisplay() {
			return getBaseType();
		}
		
		public IEnumerable<InventoryItem> getMatchingItems(StorageContainer sc) {
			IList<InventoryItem> li = sc.container.GetItems(getBaseType());
			return li == null ? null : li.Where(ii => isItemValid(ii.item));
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
		
		public override string ToString()
		{
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
		
		public override string ToString()
		{
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
			secondsPerEnzyme = processTime/(float)enzyCount;
			powerPerSecond = totalEnergyCost/processTime;
			if (powerPerSecond < Bioprocessor.POWER_COST_IDLE)
				throw new Exception("Recipe "+this+" uses too little energy!");
		}
		
		public int getInputCount() {
			return inputCount;
		}
		
		public int getOutputCount() {
			return outputCount;
		}
		
		public override string ToString()
		{
			return string.Format("[BioRecipe InputItem={0}, OutputItem={1}, enzyCount={2}, ProcessTime={3}, secondsPerEnzyme={4}, InputCount={5}, OutputCount={6}, totalEnergyCost={7}, powerPerSecond={8}]", inputItem, outputItem, enzyCount, processTime, secondsPerEnzyme, inputCount, outputCount, totalEnergyCost, powerPerSecond);
		}

		
	}
}

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class Bioprocessor : CustomMachine<BioprocessorLogic> {
		
		internal static readonly Dictionary<TechType, BioRecipe> recipes = new Dictionary<TechType, BioRecipe>();
		internal static readonly Dictionary<TechType, TechType> delegates = new Dictionary<TechType, TechType>();
		
		internal static readonly Arrow leftArrow = new Arrow("arrowL", "", "", "");
		internal static readonly Arrow rightArrow = new Arrow("arrowR", "", "", "");
		internal static readonly Arrow returnArrow = new Arrow("arrowRet", "", "", "");
		internal static readonly Arrow spacer = new Arrow("spacer", "", "", "");
		
		internal static readonly float POWER_COST_IDLE = 0.5F; //per second; was 1.5 then 2.5
		internal static readonly float POWER_COST_ACTIVE = 16.0F; //per second
		
		private static readonly string MACHINE_GO_NAME = "MachineModel";
		
		private static TechCategory bioprocCategory = TechCategory.Misc;
		
		static Bioprocessor() {
			leftArrow.Patch();
			rightArrow.Patch();
			returnArrow.Patch();
			spacer.Patch();
			bioprocCategory = TechCategoryHandler.Main.AddTechCategory("bioprocessor", "Bioprocessor");
			TechCategoryHandler.Main.TryRegisterTechCategoryToTechGroup(TechGroup.Resources, bioprocCategory);
		}
		
		public static void addRecipes() {
			addRecipe(TechType.CreepvineSeedCluster, TechType.Lubricant, 2, 10, 1, 2);
			addRecipe(TechType.AcidMushroom, CraftingItems.getItem(CraftingItems.Items.WeakAcid).TechType, 2, 2, 5, 2);
			addRecipe(TechType.WhiteMushroom, TechType.HydrochloricAcid, 6, 20, 9, 2);
			addRecipe(TechType.BloodOil, TechType.Benzene, 5, 45, 4);
			addRecipe(SeaToSeaMod.alkali.seed.TechType, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 4, 30, 5);
			addRecipe(TechType.GasPod, CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType, 1, 15, 3, 5);
			addRecipe(TechType.SnakeMushroomSpore, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2, 90, 2);
			addRecipe(TechType.HatchingEnzymes, CraftingItems.getItem(CraftingItems.Items.SmartPolymer).TechType, 4, 120, 6);
			addRecipe(TechType.SeaTreaderPoop, CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, 1, 10, 1, 4);
			//addRecipe(SeaToSeaMod.kelp.seed.TechType, CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType, 2, 15, 3, 5);
		}
		
		private static void addRecipe(TechType inp, TechType o, int salt = 5, float secs = 45, int inamt = 1, int outamt = 1) {
			BioRecipe r = new BioRecipe(salt, secs, inp, o);
			recipes[r.inputItem] = r;
			r.inputCount = inamt;
			r.outputCount = outamt;
			createRecipeDelegate(r);
		}
		
		private static TechType createRecipeDelegate(BioRecipe r) {			
			BasicCraftingItem to = CraftingItems.getItemByTech(r.outputItem);
			string rec = " (x"+r.outputCount+")";
			DuplicateRecipeDelegate item = to == null ? new DuplicateRecipeDelegate(r.outputItem, rec) : new DuplicateRecipeDelegate(to, rec);
			item.category = bioprocCategory;
			item.group = TechGroup.Resources;
			item.unlock = r.inputItem;
			if (item.sprite == null && to != null)
				item.sprite = to.getIcon();
			if (item.sprite == null)
				item.sprite = SpriteManager.Get(r.outputItem);
			item.Patch();
			RecipeUtil.addRecipe(item.TechType, TechGroup.Resources, bioprocCategory, r.outputCount, CraftTree.Type.None);
			//RecipeUtil.addIngredient(item.TechType, SeaToSeaMod.processor.TechType, 1);
			//RecipeUtil.addIngredient(item.TechType, leftArrow.TechType, 1);
			RecipeUtil.addIngredient(item.TechType, r.inputItem, r.inputCount);
			RecipeUtil.addIngredient(item.TechType, CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType, r.saltCount);
			//RecipeUtil.addIngredient(item.TechType, spacer.TechType, 1);
			//RecipeUtil.addIngredient(item.TechType, spacer.TechType, 1);
			//RecipeUtil.addIngredient(item.TechType, returnArrow.TechType, 1);
			//RecipeUtil.addIngredient(item.TechType, rightArrow.TechType, 1);
			//RecipeUtil.addIngredient(item.TechType, r.outputItem, r.outputCount);
			delegates[item.TechType] = r.inputItem;
			return item.TechType;
		}
		
		public static BioRecipe getDelegateRecipe(TechType tt) {
			return recipes[delegates[tt]];
		}
		
		public Bioprocessor() : base("bioprocessor", "Bioprocessor", "Decomposes and recombines organic matter into useful raw chemicals.", "6d71afaa-09b6-44d3-ba2d-66644ffe6a99") {
			addIngredient(TechType.TitaniumIngot, 1);
			addIngredient(TechType.Magnetite, 4);
			addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 6);
			addIngredient(TechType.CopperWire, 1);
			addIngredient(CraftingItems.getItem(CraftingItems.Items.BaseGlass).TechType, 3);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		//protected OrientedBounds[] GetBounds { get; }
		
		public override void initializeMachine(GameObject go) { //FIXME sky tint issues
			base.initializeMachine(go);
			ObjectUtil.removeComponent<Aquarium>(go);
			ObjectUtil.removeChildObject(go, "Bubbles");
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			con.storageRoot.ClassId = "bioprocessorcontainer";
			con.hoverText = "Use Bioprocessor";
			con.storageLabel = "BIOPROCESSOR";
			con.enabled = true;
			con.Resize(6, 6);
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
			cc.radius = 1.2F;
			cc.center = new Vector3(0, 0.25F, 0);
			cc = machineMdl.AddComponent<SphereCollider>();
			cc.radius = 1.2F;
			cc.center = new Vector3(0, 1F, 0);
			
			Renderer r = machineMdl.GetComponentInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			RenderUtil.setEmissivity(r, 2, "GlowStrength");
			r.materials[0].EnableKeyword("MARMO_EMISSION");
			r.sharedMaterial.EnableKeyword("MARMO_EMISSION");
			r.materials[0].SetFloat("_Shininess", 8);
			r.materials[0].SetFloat("_Fresnel", 0.4F);
			lgc.mainRenderer = r;
			
			go.GetComponent<Constructable>().model = machineMdl;
			go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.3F, 0.4F, 1.3F);
			go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 0.5F, 0);
			
			ObjectUtil.removeChildObject(go, "SubDamageSounds");
			
			go.EnsureComponent<SkyApplier>();
			SkyApplier[] skies = go.GetComponentsInChildren<SkyApplier>();
			foreach (SkyApplier sky in skies)
				sky.renderers = go.GetComponentsInChildren<Renderer>();
			
			//ObjectUtil.removeComponent<PrefabIdentifier>(machineMdl);
			//ChildObjectIdentifier coi = machineMdl.EnsureComponent<ChildObjectIdentifier>();
			//coi.classId = ClassID+"_mdl";
			
			foreach (SkyApplier sky in skies) {
				sky.renderers = go.GetComponentsInChildren<Renderer>();
				sky.enabled = true;
				sky.RefreshDirtySky();
				sky.ApplySkybox();
			}
			
			setTerminalBox(go);
		}
		
		internal static void setTerminalBox(GameObject go) {
			BoxCollider box = ObjectUtil.getChildObject(go, "Collider").EnsureComponent<BoxCollider>();
			box.center = new Vector3(0, 0.5F, 0);
			box.size = new Vector3(0.5F, 1.5F, 0.5F);
		}
		
	}
		
	public class BioprocessorLogic : CustomMachineLogic {
		
		private BioRecipe currentOperation;
		private int saltRequired;
		private float nextSaltTimeRemaining;
		
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
		private bool setCollision;
		
		void Start() {
			SNUtil.log("Reinitializing bioproc");
			SeaToSeaMod.processor.initializeMachine(gameObject);
			setEmissiveColor(new Color(0, 0, 1));
		}
		
		protected override void updateEntity(float seconds) {
			if (mainRenderer == null)
				mainRenderer = ObjectUtil.getChildObject(gameObject, "model").GetComponent<Renderer>();
			StorageContainer sc = getStorage();
			if (!sc) {
				setEmissiveColor(new Color(1, 0, 1)); //error
				return;
			}
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			if (seconds <= 0)
				return;
			if (!setCollision) {
				Bioprocessor.setTerminalBox(gameObject);
				setCollision = true;
			}
			
			if (consumePower(Bioprocessor.POWER_COST_IDLE, seconds)) {
				setEmissiveColor(noRecipeColor);
				if (currentOperation != null) {
					IList<InventoryItem> kelp = sc.container.GetItems(CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType);
					bool hasKelp = kelp != null && kelp.Count > 0;
					setEmissiveColor(recipeStalledColor);
					nextSaltTimeRemaining -= seconds*(hasKelp ? 1.5F : 1);
					//SNUtil.writeToChat("remaining: "+nextSaltTimeRemaining);
					if (nextSaltTimeRemaining <= 0 && consumePower(Bioprocessor.POWER_COST_IDLE, seconds*((Bioprocessor.POWER_COST_ACTIVE/Bioprocessor.POWER_COST_IDLE)-1))) {
						IList<InventoryItem> salt = sc.container.GetItems(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType);
						if (salt != null && salt.Count >= 1) {
							ObjectUtil.removeItem(sc, salt[0]);
							saltRequired--;
							SNUtil.playSoundAt(SNUtil.getSound("event:/loot/pickup_lubricant"), gameObject.transform.position);
							setEmissiveColor(workingColor, 1+currentOperation.secondsPerSalt);
						}
						else {
							setRecipe(null);
						}
						nextSaltTimeRemaining = currentOperation.secondsPerSalt;
						if (saltRequired <= 0) {
							//SNUtil.writeToChat("try craft");
							IList<InventoryItem> ing = sc.container.GetItems(currentOperation.inputItem);
							if (ing != null && ing.Count >= currentOperation.inputCount) {
								//SNUtil.writeToChat("success");
								for (int i = 0; i < currentOperation.inputCount; i++)
									ObjectUtil.removeItem(sc, ing[0]); //list is updated in realtime
								int n = currentOperation.outputCount;
								if (hasKelp) {
									n *= 2;
									ObjectUtil.removeItem(sc, kelp[0]);
								}
								for (int i = 0; i < n; i++) {
									GameObject item = ObjectUtil.createWorldObject(CraftData.GetClassIdForTechType(currentOperation.outputItem), true, false);
									item.SetActive(false);
									sc.container.AddItem(item.GetComponent<Pickupable>());
									colorCooldown = -1;
									setEmissiveColor(completeColor, 4);
									SNUtil.playSoundAt(SNUtil.getSound("event:/tools/knife/heat_hit"), gameObject.transform.position);
									SNUtil.log("Bioprocessor crafted "+currentOperation.outputItem.AsString());
								}
								setRecipe(null);
							}
							else {
								setRecipe(null);
							}
						}
					}
					else if (DayNightCycle.main.timePassedAsFloat-lastWorkingSound >= 1.0) {
						lastWorkingSound = DayNightCycle.main.timePassedAsFloat;
						//SNUtil.playSoundAt(SNUtil.getSound("event:/sub_module/workbench/working"), gameObject.transform.position);
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
			}
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
			//if (!KnownTech.knownTech.Contains(r.inputItem) || !KnownTech.knownTech.Contains(r.outputItem))
			//	return false;
			IList<InventoryItem> ing = sc.container.GetItems(r.inputItem);
			IList<InventoryItem> salt = sc.container.GetItems(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType);
			return ing != null && salt != null && salt.Count >= r.saltCount && ing.Count >= r.inputCount;
		}
		
		private void setRecipe(BioRecipe r) {
			bool had = currentOperation != null;
			bool has = r != null;
			currentOperation = r;
			saltRequired = r != null ? r.saltCount : -1;
			nextSaltTimeRemaining = r != null ? /*r.secondsPerSalt*/0.05F : -1;
			setEmissiveColor(r == null ? noRecipeColor : recipeStalledColor);
			if (has != had)
				SNUtil.playSoundAt(SNUtil.getSound(r == null ? "event:/sub/seamoth/seamoth_light_off" : "event:/sub/seamoth/seamoth_light_on"), gameObject.transform.position);
		}
		
	}
	
	public class BioRecipe {
			
		public readonly TechType inputItem;
		public readonly TechType outputItem;
		public readonly int saltCount;
		public readonly float processTime;
		
		public readonly float secondsPerSalt;
		
		internal int inputCount = 1;
		internal int outputCount = 1;
		
		internal BioRecipe(int s, float t, TechType inp, TechType o) {
			inputItem = inp;
			outputItem = o;
			saltCount = s;
			processTime = t;
			secondsPerSalt = processTime/(float)saltCount;
		}
		
		public int getInputCount() {
			return inputCount;
		}
		
		public int getOutputCount() {
			return outputCount;
		}
		
		public override string ToString()
		{
			return string.Format("[BioRecipe InputItem={0}, OutputItem={1}, SaltCount={2}, ProcessTime={3}, SecondsPerSalt={4}, InputCount={5}, OutputCount={6}]", inputItem, outputItem, saltCount, processTime, secondsPerSalt, inputCount, outputCount);
		}

		
	}
}

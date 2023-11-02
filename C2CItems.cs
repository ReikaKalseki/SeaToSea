using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Reflection;
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;

using ReikaKalseki.Auroresource;
using ReikaKalseki.Reefbalance;
using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.Exscansion;

namespace ReikaKalseki.SeaToSea
{
  public static class C2CItems {
    
    internal static SeamothVoidStealthModule voidStealth;
    internal static CyclopsHeatModule cyclopsHeat;
    internal static SeamothDepthModule depth1300;
    internal static SeamothPowerSealModule powerSeal;
    internal static SeamothHeatSinkModule heatSinkModule;
    internal static SeamothSpeedModule speedModule;
    internal static VehicleLightModule lightModule;
    internal static SealedSuit sealSuit;
    internal static SealedGloves sealGloves;
    internal static AzuriteBattery t2Battery;
    
    internal static RebreatherV2 rebreatherV2;
    internal static LiquidTank liquidTank;
    
    internal static BreathingFluid breathingFluid;
    internal static SeamothHeatSink heatSink;
    internal static CurativeBandage bandage;
    internal static KharaaTreatment treatment;
    
    internal static AlkaliPlant alkali;
    internal static VentKelp kelp;
    internal static HealingFlower healFlower;
    internal static MountainGlow mountainGlow;
    internal static SanctuaryPlant sanctuaryPlant;
    
    public static BrokenTablet brokenRedTablet;
    public static BrokenTablet brokenWhiteTablet;
    public static BrokenTablet brokenOrangeTablet;
    public static BrokenTablet brokenBlueTablet;
    
    public static TechCategory chemistryCategory;
    public static TechCategory ingotCategory;
    
    private static readonly Dictionary<TechType, IngotDefinition> ingots = new Dictionary<TechType, IngotDefinition>();
    private static readonly Dictionary<TechType, TechType> brokenTablets = new Dictionary<TechType, TechType>();
    
    internal static void registerTabletTechKey(BrokenTablet tb) {
		brokenTablets[tb.TechType] = tb.tablet;
		brokenTablets[tb.tablet] = tb.tablet;
    }
   /*
	public static bool hasNoGasMask() {
   		return Inventory.main.equipment.GetCount(TechType.Rebreather) == 0 && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0;
	}*/
   
   	internal static void preAdd() {
   		XMLLocale.LocaleEntry e = SeaToSeaMod.miscLocale.getEntry("CraftingNodes");
   		chemistryCategory = TechCategoryHandler.Main.AddTechCategory("C2Chemistry", e.getField<string>("chemistry"));
        TechCategoryHandler.Main.TryRegisterTechCategoryToTechGroup(TechGroup.Resources, chemistryCategory);
        CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Fabricator, "C2Chemistry", e.getField<string>("chemistry"), TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/chemistry"), "Resources");
        
        ingotCategory = TechCategoryHandler.Main.AddTechCategory("C2CIngots", e.getField<string>("ingots"));
        TechCategoryHandler.Main.TryRegisterTechCategoryToTechGroup(TechGroup.Resources, ingotCategory);
        CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Fabricator, "C2CIngots", e.getField<string>("ingots"), TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/ingotmaking"), "Resources");
        CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Fabricator, "C2CIngots2", e.getField<string>("ingotUnpack"), TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/ingotbreaking"), "Resources");
        
	    CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Workbench, "C2CMedical", e.getField<string>("medical"), TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/medical"));
	    CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Workbench, "C2CHelmet", e.getField<string>("helmet"), SpriteManager.Get(TechType.Rebreather));
        
	    brokenRedTablet = new BrokenTablet(TechType.PrecursorKey_Red);
	    brokenWhiteTablet = new BrokenTablet(TechType.PrecursorKey_White);
	    brokenOrangeTablet = new BrokenTablet(TechType.PrecursorKey_Orange);
	    brokenBlueTablet = new BrokenTablet(TechType.PrecursorKey_Blue);
	    
        voidStealth = new SeamothVoidStealthModule();        
        depth1300 = new SeamothDepthModule("SMDepth4", "Seamoth Depth Module MK4", "Increases crush depth to 1300m.", 1300);
        powerSeal = new SeamothPowerSealModule();
        heatSinkModule = new SeamothHeatSinkModule();
        speedModule = new SeamothSpeedModule();
        lightModule = new VehicleLightModule();
        cyclopsHeat = new CyclopsHeatModule();        
		sealSuit = new SealedSuit();   
		sealGloves = new SealedGloves();				
		t2Battery = new AzuriteBattery();		
        rebreatherV2 = new RebreatherV2();		
        liquidTank = new LiquidTank();        
		breathingFluid = new BreathingFluid();
		heatSink = new SeamothHeatSink();
		bandage = new CurativeBandage();
		treatment = new KharaaTreatment();
   	}
   
   	internal static void addCraftingItems() {
   		CraftingItems.addAll();
   	}
   
   	internal static void addMainItems() {
		breathingFluid.Patch();
		heatSink.Patch();
		
        depth1300.preventNaturalUnlock();
        depth1300.Patch();
        
        powerSeal.preventNaturalUnlock();
        powerSeal.Patch();
        
        voidStealth.preventNaturalUnlock();
        voidStealth.Patch();
        
        heatSinkModule.preventNaturalUnlock();
        heatSinkModule.Patch();
        
        speedModule.preventNaturalUnlock();
        speedModule.Patch();
        
        lightModule.preventNaturalUnlock();
        lightModule.Patch();
        
        cyclopsHeat.preventNaturalUnlock();
        cyclopsHeat.Patch();
        
        sealGloves.Patch(); //has to be before suit since suit references this in craft
        sealSuit.Patch();
		
		t2Battery.Patch();
		
        rebreatherV2.Patch();
		
        liquidTank.Patch();
        
		bandage.Patch();
		CraftData.useEatSound[bandage.TechType] = CraftData.useEatSound[TechType.FirstAidKit];
		treatment.Patch();
		CraftData.useEatSound[treatment.TechType] = CraftData.useEatSound[TechType.FirstAidKit];
   	}
    
   	internal static void addFlora() {
		alkali = new AlkaliPlant();
		alkali.Patch();	
		XMLLocale.LocaleEntry e = SeaToSeaMod.itemLocale.getEntry(alkali.ClassID);
		alkali.addPDAEntry(e.pda, 3, e.getField<string>("header"));
		SNUtil.log(" > "+alkali);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.Mountains_IslandCaveFloor, 1, 1F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.Mountains_CaveFloor, 1, 0.5F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.Dunes_CaveFloor, 1, 0.5F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.KooshZone_CaveFloor, 1, 2F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.SeaTreaderPath_CaveFloor, 1, 1F);
		//GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.UnderwaterIslands_ValleyFloor, 1, 0.5F);
		
		kelp = new VentKelp();
		kelp.Patch();	
		e = SeaToSeaMod.itemLocale.getEntry(kelp.ClassID);
		kelp.addPDAEntry(e.pda, 3, e.getField<string>("header"));
		SNUtil.log(" > "+kelp);
		
		healFlower = new HealingFlower();
		healFlower.Patch();	
		e = SeaToSeaMod.itemLocale.getEntry(healFlower.ClassID);
		healFlower.addPDAEntry(e.pda, 5, e.getField<string>("header"));
		SNUtil.log(" > "+healFlower);
		GenUtil.registerSlotWorldgen(healFlower.ClassID, healFlower.PrefabFileName, healFlower.TechType, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Near, BiomeType.GrassyPlateaus_CaveFloor, 1, 2.5F);
		
		mountainGlow = new MountainGlow();
		mountainGlow.Patch();	
		e = SeaToSeaMod.itemLocale.getEntry(mountainGlow.ClassID);
		mountainGlow.addPDAEntry(e.pda, 8, e.getField<string>("header"));
		SNUtil.log(" > "+mountainGlow);
		GenUtil.registerSlotWorldgen(mountainGlow.ClassID, mountainGlow.PrefabFileName, mountainGlow.TechType, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Medium, BiomeType.Mountains_Grass, 1, 0.5F);
		GenUtil.registerSlotWorldgen(mountainGlow.ClassID, mountainGlow.PrefabFileName, mountainGlow.TechType, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Medium, BiomeType.Mountains_Rock, 1, 0.1F);
		GenUtil.registerSlotWorldgen(mountainGlow.ClassID, mountainGlow.PrefabFileName, mountainGlow.TechType, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Medium, BiomeType.Mountains_Sand, 1, 0.3F);
		
		sanctuaryPlant = new SanctuaryPlant();
		sanctuaryPlant.Patch();	
		e = SeaToSeaMod.itemLocale.getEntry(sanctuaryPlant.ClassID);
		sanctuaryPlant.addPDAEntry(e.pda, 10, e.getField<string>("header"));
		SNUtil.log(" > "+sanctuaryPlant);
		
		BioReactorHandler.Main.SetBioReactorCharge(alkali.seed.TechType, BaseBioReactor.GetCharge(TechType.RedBushSeed)*1.5F);
		BioReactorHandler.Main.SetBioReactorCharge(kelp.seed.TechType, BaseBioReactor.GetCharge(TechType.BloodOil)*0.8F);
		BioReactorHandler.Main.SetBioReactorCharge(healFlower.seed.TechType, BaseBioReactor.GetCharge(TechType.Peeper));
		BioReactorHandler.Main.SetBioReactorCharge(mountainGlow.seed.TechType, BaseBioReactor.GetCharge(TechType.Oculus)*2F);
		BioReactorHandler.Main.SetBioReactorCharge(sanctuaryPlant.seed.TechType, BaseBioReactor.GetCharge(TechType.RedBasketPlantSeed)*1.5F);
		BioReactorHandler.Main.SetBioReactorCharge(CraftingItems.getItem(CraftingItems.Items.AmoeboidSample).TechType, BaseBioReactor.GetCharge(TechType.CreepvinePiece));
		BioReactorHandler.Main.SetBioReactorCharge(CustomEgg.getEgg(SeaToSeaMod.deepStalker.TechType).TechType, BaseBioReactor.GetCharge(TechType.StalkerEgg)*0.9F);
   	}
   
   	internal static void addTablets() {
        brokenBlueTablet.register();
        brokenRedTablet.register();
        brokenWhiteTablet.register();
        brokenOrangeTablet.register();
   	}
   
   	internal static void postAdd() {		
		registerTabletTechKey(brokenBlueTablet);
		registerTabletTechKey(brokenOrangeTablet);
		registerTabletTechKey(brokenWhiteTablet);
		registerTabletTechKey(brokenRedTablet);
		
		BatteryCharger.compatibleTech.Add(t2Battery.TechType);
        
		//override first aid kit
        UsableItemRegistry.instance.addUsableItem(TechType.FirstAidKit, (s, go) => {
		    if (SeaToSeaMod.playerCanHeal() && Player.main.GetComponent<LiveMixin>().AddHealth(0.1F) > 0.05) {
				HealingOverTime ht = Player.main.gameObject.EnsureComponent<HealingOverTime>();
				ht.setValues(20, 20);
				ht.activate();
				return true;
	    	}
	    	return false;
		});
        UsableItemRegistry.instance.addUsableItem(bandage.TechType, (s, go) => {
	    	if (SeaToSeaMod.playerCanHeal() && Player.main.GetComponent<LiveMixin>().AddHealth(0.1F) > 0.05) {
				HealingOverTime ht = Player.main.gameObject.EnsureComponent<HealingOverTime>();
				ht.setValues(50, 5);
				ht.activate();
				foreach (DamageOverTime dt in Player.main.gameObject.GetComponentsInChildren<DamageOverTime>()) {
					dt.damageRemaining = 0;
					dt.CancelInvoke("DoDamage");
					UnityEngine.Object.DestroyImmediate(dt);
				}
				foreach (PlayerMovementSpeedModifier ds in Player.main.gameObject.GetComponents<PlayerMovementSpeedModifier>()) {
					if (ds.speedModifier < 1)
						UnityEngine.Object.DestroyImmediate(ds);
				}
				Ecocean.FoodEffectSystem.instance.clearNegativeEffects();
				return true;
			}
	    	return false;
		});
        UsableItemRegistry.instance.addUsableItem(treatment.TechType, (s, go) => {
		   	float time = DayNightCycle.main.timePassedAsFloat;
			if (time-LiquidBreathingSystem.instance.lastKharaaTreatmentTime < 30)
				return false;
			LiquidBreathingSystem.instance.lastKharaaTreatmentTime = time;
			return true;
		});
		
		IrreplaceableItemRegistry.instance.registerItem(CraftingItems.getItem(CraftingItems.Items.BrokenT2Battery));
		IrreplaceableItemRegistry.instance.registerItem(CraftingItems.getItem(CraftingItems.Items.DenseAzurite));
		IrreplaceableItemRegistry.instance.registerItem(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL));
		IrreplaceableItemRegistry.instance.registerItem(TechType.PrecursorKey_Blue);
		IrreplaceableItemRegistry.instance.registerItem(TechType.PrecursorKey_Red);
   	}
    
    internal static void onTechUnlocked(TechType tech) {
   		if (DIHooks.getWorldAge() < 0.25F)
   			return;
   		if (brokenTablets.ContainsKey(tech))
   			SNUtil.triggerTechPopup(brokenTablets[tech]);
   		else if (tech == CraftingItems.getItem(CraftingItems.Items.BacterialSample).TechType || tech == CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType)
   			SNUtil.triggerTechPopup(tech);
    }
    
    internal class IngotDefinition {
    	
    	internal readonly TechType material;
    	internal readonly TechType ingot;
    	internal readonly DuplicateRecipeDelegateWithRecipe unpackingRecipe;
    	internal readonly int count;
    	
    	internal IngotDefinition(TechType mat, TechType ing, DuplicateRecipeDelegateWithRecipe unpack, int amt) {
    		material = mat;
    		ingot = ing;
    		count = amt;
    		unpackingRecipe = unpack;
    	}
    	
    }
   
   	internal static void addIngot(TechType item, BasicCraftingItem ing, DuplicateRecipeDelegateWithRecipe unpack, int amt) {
   		addIngot(item, ing.TechType, unpack, amt);
   	}
   
   	internal static void addIngot(TechType item, TechType ing, DuplicateRecipeDelegateWithRecipe unpack, int amt) {
   		ingots[item] = new IngotDefinition(item, ing, unpack, amt);
   	}
    
    internal static IngotDefinition getIngot(TechType item) {
    	return ingots[item];
    }
    
    internal static List<IngotDefinition> getIngots() {
    	return new List<IngotDefinition>(ingots.Values);
    }
    
    internal static void setChemistry(TechType item) {
		RecipeUtil.changeRecipePath(item, "Resources", "C2Chemistry");
		RecipeUtil.setItemCategory(item, TechGroup.Resources, chemistryCategory);
    }
   
   public static bool hasSealedOrReinforcedSuit(out bool isSealed, out bool isReinf) {
	   	InventoryItem suit = Inventory.main.equipment.GetItemInSlot("Body");
	   	InventoryItem glove = Inventory.main.equipment.GetItemInSlot("Gloves");
	   	bool sealSuit = suit != null && suit.item.GetTechType() == C2CItems.sealSuit.TechType;
	   	bool reinfSuit = suit != null && suit.item.GetTechType() == TechType.ReinforcedDiveSuit;
	   	bool sealGlove = glove != null && glove.item.GetTechType() == C2CItems.sealGloves.TechType;
	   	bool reinfGlove = glove != null && glove.item.GetTechType() == TechType.ReinforcedGloves;
	   	isSealed = sealSuit && sealGlove;
	   	isReinf = reinfSuit && reinfGlove;
	   	return (sealSuit || reinfSuit) && (sealGlove || reinfGlove);
   }

  }
}

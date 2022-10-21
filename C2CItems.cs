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
    internal static CustomEquipable sealSuit;
    internal static CustomBattery t2Battery;
    
    internal static RebreatherV2 rebreatherV2;
    internal static LiquidTank liquidTank;
    
    internal static BreathingFluid breathingFluid;
    internal static CurativeBandage bandage;
    
    internal static AlkaliPlant alkali;
    internal static VentKelp kelp;
    internal static HealingFlower healFlower;
    
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
        chemistryCategory = TechCategoryHandler.Main.AddTechCategory("C2Chemistry", "Chemistry");
        TechCategoryHandler.Main.TryRegisterTechCategoryToTechGroup(TechGroup.Resources, chemistryCategory);
        CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Fabricator, "C2Chemistry", "Chemistry", TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/chemistry")/*SpriteManager.Get(SpriteManager.Group.Tab, "fabricator_enzymes")*/, "Resources");
        
        ingotCategory = TechCategoryHandler.Main.AddTechCategory("C2CIngots", "Metal Ingots");
        TechCategoryHandler.Main.TryRegisterTechCategoryToTechGroup(TechGroup.Resources, ingotCategory);
        CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Fabricator, "C2CIngots", "Metal Ingots", TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/ingotmaking"), "Resources");
        CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Fabricator, "C2CIngots2", "Metal Unpacking", TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/ingotbreaking"), "Resources");
        
	    brokenRedTablet = new BrokenTablet(TechType.PrecursorKey_Red);
	    brokenWhiteTablet = new BrokenTablet(TechType.PrecursorKey_White);
	    brokenOrangeTablet = new BrokenTablet(TechType.PrecursorKey_Orange);
	    brokenBlueTablet = new BrokenTablet(TechType.PrecursorKey_Blue);
	    
        voidStealth = new SeamothVoidStealthModule();        
        depth1300 = new SeamothDepthModule("SMDepth4", "Seamoth Depth Module MK4", "Increases crush depth to 1300m.", 1300);        
        powerSeal = new SeamothPowerSealModule();        
        cyclopsHeat = new CyclopsHeatModule();        
		sealSuit = new SealedSuit();		
		t2Battery = new CustomBattery(SeaToSeaMod.itemLocale.getEntry("t2battery"), 750);		
        rebreatherV2 = new RebreatherV2();		
        liquidTank = new LiquidTank();        
		breathingFluid = new BreathingFluid();        
		bandage = new CurativeBandage();
   	}
   
   	internal static void addCraftingItems() {
   		CraftingItems.addAll();
   	}
   
   	internal static void addMainItems() {
        voidStealth.Patch();
        
        depth1300.preventNaturalUnlock();
        depth1300.Patch();
        
        powerSeal.Patch();
        
        cyclopsHeat.Patch();
        
        sealSuit.Patch();
		
		t2Battery.unlockRequirement = TechType.Unobtanium;//CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType;
		t2Battery.Patch();
		
        rebreatherV2.Patch();
		
        liquidTank.Patch();
        
		breathingFluid.Patch();
        
		bandage.Patch();
		CraftData.useEatSound[bandage.TechType] = CraftData.useEatSound[TechType.FirstAidKit];
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
		
		BioReactorHandler.Main.SetBioReactorCharge(alkali.seed.TechType, BaseBioReactor.GetCharge(TechType.RedBushSeed)*1.5F);
		BioReactorHandler.Main.SetBioReactorCharge(kelp.seed.TechType, BaseBioReactor.GetCharge(TechType.BloodOil)*1.5F);
		BioReactorHandler.Main.SetBioReactorCharge(healFlower.seed.TechType, BaseBioReactor.GetCharge(TechType.Peeper));
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
        
        UsableItemRegistry.instance.addUsableItem(TechType.FirstAidKit, (s, go) => {
	    	if (Player.main.GetComponent<LiveMixin>().AddHealth(0.1F) > 0.05) {
				HealingOverTime ht = Player.main.gameObject.EnsureComponent<HealingOverTime>();
				ht.setValues(20, 20);
				ht.activate();
				return true;
	    	}
	    	return false;
		});
        UsableItemRegistry.instance.addUsableItem(bandage.TechType, (s, go) => {
	    	if (Player.main.GetComponent<LiveMixin>().AddHealth(0.1F) > 0.05) {
				HealingOverTime ht = Player.main.gameObject.EnsureComponent<HealingOverTime>();
				ht.setValues(50, 5);
				ht.activate();
				foreach (DamageOverTime dt in Player.main.gameObject.GetComponentsInChildren<DamageOverTime>()) {
					dt.damageRemaining = 0;
					dt.CancelInvoke("DoDamage");
					UnityEngine.Object.DestroyImmediate(dt);
				}
				return true;
			}
	    	return false;
		});
   	}
    
    internal static void onTechUnlocked(TechType tech) {
    	if (brokenTablets.ContainsKey(tech))
    		SNUtil.triggerTechPopup(brokenTablets[tech]);
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

  }
}

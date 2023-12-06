using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using UnityEngine.UI;

using FMOD;
using FMODUnity;

using QModManager.API.ModLoading;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

using FCS_AlterraHub;
using FCS_AlterraHub.Registration;
using FCS_AlterraHomeSolutions;
using FCS_EnergySolutions;
using FCS_EnergySolutions.Configuration;
using FCS_HomeSolutions;
using FCS_HomeSolutions.Configuration;
using FCS_LifeSupportSolutions;
using FCS_LifeSupportSolutions.Configuration;
using FCS_ProductionSolutions;
using FCS_ProductionSolutions.Configuration;
using FCS_StorageSolutions;
using FCS_StorageSolutions.Configuration;
using FCS_HomeSolutions.Mods.TrashRecycler.Mono;
using FCS_HomeSolutions.Mods.TrashRecycler;
using FCS_ProductionSolutions.Mods.DeepDriller;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono;
using FCSCommon;

namespace ReikaKalseki.SeaToSea {
	
	public class FCSIntegrationSystem {
		
		public static readonly FCSIntegrationSystem instance = new FCSIntegrationSystem();
		
		private readonly bool isFCSLoaded;
		
		private Type drillOreManager;
		private TechType fcsBiofuel;
		private DuplicateRecipeDelegateWithRecipe fcsBiofuelAlt;
		private readonly HashSet<TechType> peeperBarFoods = new HashSet<TechType>();
		private readonly HashSet<TechType> registeredEffectFoods = new HashSet<TechType>();
		private readonly HashSet<TechType> notBuyableTechs = new HashSet<TechType>();
		private readonly HashSet<TechType> replacedTechRecipes = new HashSet<TechType>();
    
	    internal BasicCraftingItem fcsDrillFuel;
	    internal BasicCraftingItem luminolDrop;
		
		private float lastTimeUnlock = -1;
		private HashSet<TechType> unlocksRightNow = new HashSet<TechType>();
		
		private FCSIntegrationSystem() {
	    	isFCSLoaded = QModManager.API.QModServices.Main.ModPresent("FCSAlterraHub");
		}
		
		public bool isLoaded() {
			return isFCSLoaded;
		}
		
		public Type getFCSDrillOreManager() {
			return drillOreManager;
		}
		
		public bool isUnlockingTypePurchase(TechType tt) {
			return replacedTechRecipes.Contains(tt) && !notBuyableTechs.Contains(tt);
		}
		
		public TechType getBiofuel() {
			return fcsBiofuel;
		}
		
		public TechType getBiofuelAlt() {
			return fcsBiofuelAlt.TechType;
		}
		
		internal void modifyPeeperFood(Pickupable pp) {
			if (!isFCSLoaded)
				return;
			TechType tt = pp.GetTechType();
			if (peeperBarFoods.Contains(tt)) {
				Eatable ea = pp.GetComponent<Eatable>();
				if (ea) {
					bool alc = ea.waterValue > 70;
					ea.waterValue = Mathf.Min(ea.waterValue*0.2F, 25);
					ea.foodValue = Mathf.Min(ea.foodValue*0.75F, 10);
					//SNUtil.log("New food and water value: "+ea.foodValue+"/"+ea.waterValue);
					
					if (alc && !registeredEffectFoods.Contains(tt)) {
						registeredEffectFoods.Add(tt);
						FoodEffectSystem.instance.addEffect(tt, makeDrunk, "Causes lasting intoxication.");
						//if (ea.waterValue > 15)
						//	FoodEffectSystem.instance.addVomitingEffect(tt, 0, ea.waterValue-15, 1, 0, 0);
					}
				}
			}
		}
	
		private static void makeDrunk(Survival s, GameObject eaten) {
			float duration = 60;
			Eatable ea = eaten.GetComponent<Eatable>();
			if (ea)
				duration += ea.waterValue*5;
			Drunk.add(duration).survivalObject = s;
		}
		
		internal void manageDrunkenness(DIHooks.PlayerInput pi) {
			Drunk d = Player.main.GetComponent<Drunk>();
			if (d)
				pi.selectedInput += d.currentPush;
		}
			
		private static void addDrillOperationHook(List<CodeInstruction> codes) {
			int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOilHandler", "HasOil", true, new Type[0]);
			codes.InsertRange(idx+1, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "canFCSDrillOperate", false, typeof(bool), typeof(MonoBehaviour))});
		}
			/*
		private static void addDrillFuelPowerHook(List<CodeInstruction> codes) {
			InstructionHandlers.patchInitialHook(codes,
				new CodeInstruction(OpCodes.Ldarg_0),
				InstructionHandlers.createMethodCall("FCS_ProductionSolutions.Mods.DeepDriller.Managers.DrillSystem", "get_OilHandler", true, new Type[0]),
				new CodeInstruction(OpCodes.Ldarg_0),
				InstructionHandlers.createMethodCall("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerController", "get_DeepDrillerPowerManager", true, new Type[0]),
			);			
		}*/
			
		private static void replaceFCSDrillFuel(List<CodeInstruction> codes) {
			for (int i = codes.Count-1; i >= 0; i--) {
				if (codes[i].LoadsConstant((int)TechType.Lubricant)) {
					codes[i] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getFCSDrillFuel", false, new Type[0]);
				}
			}
		}
		
		private static void preventDuplicateUnlock(List<CodeInstruction> codes) {
			int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Stloc_0);
			codes.InsertRange(idx, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand("FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs.CartDropDownHandler", "_pendingItems")), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "filterFCSCartAdd", false, typeof(int), typeof(IList), typeof(TechType))});
		}
			
		private static void filterShopList(List<CodeInstruction> codes) {
			codes.Clear();
			codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
			codes.Add(InstructionHandlers.createMethodCall("FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems.StoreItem", "get_TechType", true, new Type[0]));
			codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "isFCSItemBuyable", false, new Type[]{typeof(TechType)}));
			codes.Add(new CodeInstruction(OpCodes.Ret));
		}
			
		private static void replacePurchaseAction(List<CodeInstruction> codes) {
			int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "FCS_AlterraHub.Helpers.PlayerInteractionHelper", "GivePlayerItem", false, new Type[]{typeof(TechType)});
			codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onFCSPurchasedTech", false, typeof(TechType));
			codes[idx-1] = InstructionHandlers.createMethodCall("FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems.CartItem", "get_TechType", true, new Type[0]); //change TechType fetch
			codes.RemoveAt(idx+1); //remove the pop
		}
			
		private static void redirectPurchase(List<CodeInstruction> codes) {
			int idx = InstructionHandlers.getMethodCallByName(codes, 0, 0, "FCS_AlterraHub.Mods.FCSPDA.Mono.FCSPDAController", "MakeAPurchase");
			codes[idx-1] = new CodeInstruction(OpCodes.Ldc_I4_1); //true instead of false
			
			idx = InstructionHandlers.getMethodCallByName(codes, 0, 0, "FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs.CheckOutPopupDialogWindow", "get_SelectedDestination")-1;
			//remove the drone location check; leaves the brfalse
			codes.RemoveAt(idx+3);
			codes.RemoveAt(idx+2);
			codes.RemoveAt(idx+1);
			//codes.RemoveAt(idx);
			codes[idx].opcode = OpCodes.Ldc_I4_0;
		}
		
		private static void controlOreGeneration(List<CodeInstruction> codes) {
			for (int i = codes.Count-1; i >= 0; i--) {
				if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand == "Spawning item {0}") {
					patchTechTypeChoice(codes, i-1);
				}
			}
		}
		
		private static void patchTechTypeChoice(List<CodeInstruction> codes, int idx) {
			List<CodeInstruction> added = new List<CodeInstruction>(){
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldarg_0),
				InstructionHandlers.createMethodCall("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator", "get_IsFocused", true, new Type[0]),
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator", "_blacklistMode")),
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator", "_focusOres")),
				new CodeInstruction(OpCodes.Ldarg_0),
				InstructionHandlers.createMethodCall("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator", "get_AllowedOres", true, new Type[0]),
				InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "pickFCSDrillOre", false, typeof(TechType), typeof(MonoBehaviour), typeof(bool), typeof(bool), typeof(HashSet<>).MakeGenericType(typeof(TechType)), typeof(List<>).MakeGenericType(typeof(TechType))),
			};
			codes.InsertRange(idx, added);
		}
		
		internal void onPlayerBuy(TechType tt) {
			if (!KnownTech.Contains(tt))
				unlocksRightNow.Add(tt);
			KnownTech.Add(tt);
			//SNUtil.triggerTechPopup(tt);
			StoryGoal.Execute("UnlockFCS"+tt.AsString(), Story.GoalType.Story);
			lastTimeUnlock = DayNightCycle.main.timePassedAsFloat;
		}
		
		internal void tickNotifications(float time) {
			if (uGUI_PopupNotification.main.isShowingMessage)
				lastTimeUnlock = time;
			
			if (time-lastTimeUnlock >= 0.25F && unlocksRightNow.Count > 0) {
				SNUtil.triggerMultiTechPopup(unlocksRightNow);
				unlocksRightNow.Clear();
			}
		}
		
		internal void initializeTechUnlocks() {
			foreach (TechType tt in replacedTechRecipes) {
				if (notBuyableTechs.Contains(tt))
					continue;
				SNUtil.log("Relocking tech "+tt.AsString()+" as no storygoal set");
				if (StoryGoalManager.main.completedGoals.Contains("UnlockFCS"+tt.AsString())) {
					KnownTech.Add(tt);
				}
				else {
					KnownTech.Remove(tt);
				}
			}
		}
		
		internal void applyPatches() {
			if (isFCSLoaded)
				doApplyPatches();
		}
		
		private void doApplyPatches() {
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
			CustomMachineLogic.powerCostFactor *= hard ? 1.6F : 2; //since hard increases bioproc 2.5x, only increase to 4x instead of 5x 
			
			FieldInfo f = typeof(FCSAlterraHubService).GetField("_storeItems", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			Assembly core = f.DeclaringType.Assembly;
			IDictionary dict = (IDictionary)f.GetValue(null);
			foreach (TechType item in C2CProgression.instance.getGatedTechnologies()) {
				if (dict.Contains(item))
					dict.Remove(item);
			}
			dict.Remove(TechType.Battery);
			dict.Remove(TechType.PowerCell);
			dict.Remove(TechType.WiringKit);
			dict.Remove(TechType.AdvancedWiringKit);
			
			f = core.GetType("FCS_AlterraHub.Systems.StoreInventorySystem").GetField("OrePrices", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			Dictionary<TechType, decimal> oreValues = (Dictionary<TechType, decimal>)f.GetValue(null);
			oreValues[CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType] = (decimal)Mathf.Lerp((float)oreValues[TechType.Nickel], (float)oreValues[TechType.Kyanite], 0.75F);
			oreValues[CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType] = oreValues[TechType.Gold]*1.5M;
			oreValues[CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType] = oreValues[TechType.Kyanite]*2M;
			
			oreValues[TechType.MercuryOre] = (decimal)Mathf.Lerp((float)oreValues[TechType.Gold], (float)oreValues[TechType.Nickel], 0.5F);
			oreValues[TechType.Salt] = oreValues[TechType.Titanium]*0.2M;
			oreValues[CraftingItems.getItem(CraftingItems.Items.TraceMetals).TechType] = oreValues[TechType.Copper];
			oreValues[CraftingItems.getItem(CraftingItems.Items.GeyserMinerals).TechType] = oreValues[TechType.Lead];
			
			foreach (C2CItems.IngotDefinition ingot in C2CItems.getIngots()) {
				if (oreValues.ContainsKey(ingot.material))
					oreValues[ingot.ingot] = oreValues[ingot.material]*ingot.count;
			}
			
			/*
			oreValues[TechType.Floater] = oreValues[TechType.Titanium]*2.0M;
			oreValues[TechType.BloodOil] = oreValues[TechType.Titanium]*1.0M;
			oreValues[TechType.CreepvineSeedCluster] = oreValues[TechType.Titanium]*0.25M;
			oreValues[TechType.SeaCrownSeed] = oreValues[TechType.Titanium]*0.5M;
			oreValues[C2CItems.alkali.seed.TechType] = oreValues[TechType.Copper];
			oreValues[C2CItems.kelp.seed.TechType] = oreValues[TechType.Copper];
			oreValues[C2CItems.mountainGlow.seed.TechType] = oreValues[TechType.Copper]*1.2M;
			oreValues[C2CItems.sanctuaryPlant.seed.TechType] = oreValues[TechType.Gold];
			oreValues[C2CItems.healFlower.seed.TechType] = (decimal)Mathf.Lerp((float)oreValues[TechType.Titanium], (float)oreValues[TechType.Copper], 0.5F);
			*/
			
			InstructionHandlers.patchMethod(SeaToSeaMod.harmony, core.GetType("FCS_AlterraHub.Mods.FCSPDA.Mono.FCSPDAController"), "MakeAPurchase", SeaToSeaMod.modDLL, replacePurchaseAction);
			InstructionHandlers.patchMethod(SeaToSeaMod.harmony, core.GetType("FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs.CheckOutPopupDialogWindow"), "MakePurchase", SeaToSeaMod.modDLL, redirectPurchase);
			InstructionHandlers.patchMethod(SeaToSeaMod.harmony, core.GetType("FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems.StoreItem"), "CheckIsUnlocked", SeaToSeaMod.modDLL, filterShopList);
			InstructionHandlers.patchMethod(SeaToSeaMod.harmony, core.GetType("FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs.CartDropDownHandler"), "AddItem", SeaToSeaMod.modDLL, preventDuplicateUnlock);
			
			Type homeModType = InstructionHandlers.getTypeBySimpleName("FCS_HomeSolutions.Configuration.Mod");
			if (homeModType != null) {
				Assembly homeMod = homeModType.Assembly;
				f = homeModType.GetField("PeeperBarFoods", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
				dict = (IDictionary)f.GetValue(null);
				if (dict.Contains(TechType.NutrientBlock))
					dict.Remove(TechType.NutrientBlock);
				if (dict.Contains(TechType.BigFilteredWater))
					dict.Remove(TechType.BigFilteredWater);
				peeperBarFoods.AddRange((IEnumerable<TechType>)dict.Keys);
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, homeMod.GetType("FCS_HomeSolutions.Mods.TrashRecycler.Mono.Recycler"), "Recycle", SeaToSeaMod.modDLL, codes => {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "FCS_AlterraHub.Helpers.TechDataHelpers", "GetIngredientsWithOutBatteries", false, new Type[]{typeof(TechType)});
					codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "filterFCSRecyclerOutput", false, typeof(List<>).MakeGenericType(typeof(Ingredient))));
				});
			}
			Type drillHelper = InstructionHandlers.getTypeBySimpleName("FCS_ProductionSolutions.Mods.DeepDriller.Helpers.Helpers");
			if (drillHelper != null) {
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, drillHelper, "GetBiomeData", SeaToSeaMod.modDLL, codes => {
					InstructionHandlers.patchEveryReturnPre(codes, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "filterFCSDrillerOutput", false, typeof(List<>).MakeGenericType(typeof(TechType))));
				});
				
				Assembly prodMod = drillHelper.Assembly;
				drillOreManager = prodMod.GetType("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator");
				DrillDepletionSystem.instance.register();
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, drillOreManager, "SetAllowTick", SeaToSeaMod.modDLL, codes => {
				    int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "FCS_AlterraHub.Mono.FcsDevice", "get_IsOperational", true, new Type[0]);
					codes.InsertRange(idx+1, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "canFCSDrillOperate", false, typeof(bool), typeof(MonoBehaviour))});
				});
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, drillOreManager, "Update", SeaToSeaMod.modDLL, codes => {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator", "_passedTime");
					codes.InsertRange(idx+1, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "tickFCSDrill", false, typeof(MonoBehaviour))});
				});
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, drillOreManager, "GenerateOre", SeaToSeaMod.modDLL, controlOreGeneration);
				
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, prodMod.GetType("FCS_ProductionSolutions.Mods.DeepDriller.Managers.DrillSystem"), "get_IsOperational", SeaToSeaMod.modDLL, addDrillOperationHook);
				Type t = prodMod.GetType("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerController");
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "get_IsOperational", SeaToSeaMod.modDLL, addDrillOperationHook);
				//InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "Update", SeaToSeaMod.modDLL, addDrillFuelPowerHook);
				                                
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, prodMod.GetType("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerContainer").GetMethod("AddItemToContainer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[]{typeof(TechType)}, null), SeaToSeaMod.modDLL, codes => {
				    int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerContainer", "get_OnContainerUpdate", true, new Type[0]);
				    codes.InsertRange(idx+1, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerContainer", "_container")), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "cleanupFCSContainer", false, typeof(Action<,>).MakeGenericType(typeof(int), typeof(int)), typeof(MonoBehaviour), typeof(Dictionary<,>).MakeGenericType(typeof(TechType), typeof(int)))});
				});
				
				fcsDrillFuel = new FCSFuel();
				//C2CItems.fcsDrillFuel.addIngredient(TechType.Benzene, 1);
				fcsDrillFuel.addIngredient(TechType.Lubricant, 2);
				fcsDrillFuel.addIngredient(EcoceanMod.glowOil.TechType, 1);
				fcsDrillFuel.addIngredient(TechType.JellyPlant, 3);
				fcsDrillFuel.addIngredient(C2CItems.alkali.seed.TechType, 1);
				fcsDrillFuel.Patch();
				TechnologyUnlockSystem.instance.addDirectUnlock(findFCSItem("DeepDrillerLightDuty"), fcsDrillFuel.TechType);
				TechnologyUnlockSystem.instance.addDirectUnlock(findFCSItem("DeepDrillerMK3"), fcsDrillFuel.TechType);
				t = prodMod.GetType("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOilHandler");
				foreach (MethodInfo m in t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					InstructionHandlers.patchMethod(SeaToSeaMod.harmony, m, SeaToSeaMod.modDLL, replaceFCSDrillFuel);
				
				t = prodMod.GetType("FCS_ProductionSolutions.Mods.DeepDriller.Patchers.DeepDrillerGUIOilPage");
				foreach (MethodInfo m in t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					InstructionHandlers.patchMethod(SeaToSeaMod.harmony, m, SeaToSeaMod.modDLL, replaceFCSDrillFuel);
			}
			
			Type alterraGen = InstructionHandlers.getTypeBySimpleName("FCS_EnergySolutions.Mods.AlterraGen.Mono.AlterraGenPowerManager");
			if (alterraGen != null) {
				Assembly energyMod = alterraGen.Assembly;
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, alterraGen, "Update", SeaToSeaMod.modDLL, codes => {
					int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Ldc_R4);
					codes[idx].operand = 2.4F; //from 1.167F
					//codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getFCSBioGenPPS", false, typeof(MonoBehaviour));
				});
				
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, alterraGen, "GetMultiplier", SeaToSeaMod.modDLL, codes => {
					InstructionHandlers.patchEveryReturnPre(codes, new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getFCSBioGenPowerFactor", false, typeof(float), typeof(MonoBehaviour), typeof(TechType)));
				});
			}
			
			luminolDrop = new BasicCraftingItem(SeaToSeaMod.itemLocale.getEntry("LuminolDrop"), "WorldEntities/Natural/polyaniline");
			luminolDrop.numberCrafted = 6;
			luminolDrop.craftingTime = 0.5F;
			luminolDrop.renderModify = CraftingItems.getItem(CraftingItems.Items.Luminol).renderModify;
			luminolDrop.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/LuminolDrop");
			luminolDrop.addIngredient(CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 1);
			luminolDrop.Patch();
			
			assignRecipe("ahsLeftCornerRailing", 1);
			assignRecipe("ahsLeftCornerwGlassRailing", 1, basicGlass());
			assignRecipe("ahsRightCornerRailing", 1);
			assignRecipe("ahsRightCornerwGlassRailing", 1, basicGlass());
			assignRecipe("ahsrailing", 1);
			assignRecipe("ahsrailingglass", 1, basicGlass());
			assignRecipe("CabinetMediumTall", 2);
			assignRecipe("CabinetTall", 3);
			assignRecipe("CabinetTallWide", 4);
			assignRecipe("CabinetTVStand", 3);
			assignRecipe("CabinetWide", 3);
			assignRecipe("Curtain", 0, fabric());
			assignRecipe("DisplayBoard", 1, electronicsTier2(2));
			assignRecipe("Elevator", 3, electronicsTier2(), motorized(2));
			assignRecipe("EmptyObservationTank", 2, strongGlass());
			assignRecipe("FCSCrewBunkBed", 2, fabric(2));
			assignRecipe("FCSCrewLocker", 2);
			assignRecipe("FCSCuringCabinet", 2, new Ingredient(TechType.CopperWire, 1), new Ingredient(TechType.Glass, 1));
			assignRecipe("FCSJukeBox", 1, new Ingredient[]{new Ingredient(TechType.ComputerChip, 1)}, speaker());
			assignRecipe("FCSJukeBoxSpeaker", 2, speaker());
			assignRecipe("FCSJukeBoxSubWoofer", 2, speaker());
			assignRecipe("FCSMicrowave", 1, electronicsTier1());
			assignRecipe("FCSRug", 0, fabric(3));
			assignRecipe("FCSShower", 2, new Ingredient(TechType.Glass, 2), new Ingredient(TechType.Pipe, 5));
			assignRecipe("FCSSink", 2, new Ingredient(TechType.Pipe, 5));
			assignRecipe("FCSToilet", 1, new Ingredient(TechType.Pipe, 5));
			assignRecipe("FCSStairs", 2);
			assignRecipe("FCSStove", 2, electronicsTier1(2), strongGlass(1));
			assignRecipe("FireExtinguisherRefueler", 1, new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 1), new Ingredient(TechType.Pipe, 5));
			assignRecipe("FloodLEDLight", 1, new Ingredient(TechType.Quartz, 2));
			assignRecipe("FloorShelf01", 1);
			assignRecipe("FloorShelf02", 3);
			assignRecipe("FloorShelf03", 2);
			assignRecipe("FloorShelf04", 3);
			assignRecipe("FloorShelf05", 3);
			assignRecipe("FloorShelf06", 2);
			assignRecipe("FloorShelf07", 2);
			assignRecipe("HologramPoster", 0, new Ingredient(TechType.Quartz, 1), new Ingredient(TechType.Magnetite, 1), new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1));
			assignRecipe("LedLightStickLong", 1, new Ingredient(TechType.Quartz, 3));
			assignRecipe("LedLightStickShort", 1, new Ingredient(TechType.Quartz, 1));
			assignRecipe("LedLightStickWall", 1, new Ingredient(TechType.Quartz, 2));
			assignRecipe("MiniFountainFilter", 1, new Ingredient(TechType.Pipe, 5));
			assignRecipe("MountSmartTV", 1, new Ingredient(TechType.AdvancedWiringKit, 1), new Ingredient(EcoceanMod.glowOil.TechType, 3), new Ingredient(TechType.Quartz, 2), new Ingredient(luminolDrop.TechType, 3));
			assignRecipe("NeonBarStool", 2, new Ingredient(luminolDrop.TechType, 1));
			assignRecipe("NeonPlanter", 2, new Ingredient(luminolDrop.TechType, 1));
			assignRecipe("NeonShelf01", 3, new Ingredient(luminolDrop.TechType, 1));
			assignRecipe("NeonShelf02", 2, new Ingredient(luminolDrop.TechType, 1));
			assignRecipe("NeonShelf03", 1, new Ingredient(luminolDrop.TechType, 1));
			assignRecipe("NeonTable01", 1, new Ingredient(luminolDrop.TechType, 1));
			assignRecipe("NeonTable02", 1, new Ingredient(luminolDrop.TechType, 1));
			assignRecipe("OutsideSign", 2, new Ingredient(TechType.Quartz, 2), new Ingredient(TechType.CopperWire, 1), new Ingredient(TechType.Silver, 1));
			assignRecipe("PaintTool", 2, new Ingredient(TechType.Battery, 1), new Ingredient(TechType.Pipe, 5));
			assignRecipe("pccpu", 2, new Ingredient(TechType.AdvancedWiringKit, 1), new Ingredient(TechType.ComputerChip, 2));
			assignRecipe("pcmonitor", 1, new Ingredient(TechType.WiringKit, 1), new Ingredient(EcoceanMod.glowOil.TechType, 2), new Ingredient(TechType.Quartz, 2), new Ingredient(luminolDrop.TechType, 2));
			assignRecipe("PeeperLoungeBar", 3, new Ingredient(TechType.ComputerChip, 1), new Ingredient(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 1));
			assignRecipe("QuantumPowerBank", 2, new Ingredient[]{new Ingredient(TechType.PowerCell, 3)}, reinforcedStrong(), electronicsTier3());
			assignRecipe("QuantumPowerBankCharger", 0, new Ingredient[]{new Ingredient(TechType.WiringKit, 2), new Ingredient(CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType, 1), new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 1)}, reinforced(2), electronicsTier1(4));
			assignRecipe("QuantumTeleporter", 4, new Ingredient[]{new Ingredient(TechType.PrecursorIonCrystal, 2), new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 1), new Ingredient(EcoceanMod.glowOil.TechType, 4)}, electronicsTier3());
			assignRecipe("QuantumTeleporterVehiclePad", 8, new Ingredient[]{new Ingredient(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 4), new Ingredient(TechType.PrecursorIonCrystal, 5), new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 2), new Ingredient(EcoceanMod.glowOil.TechType, 8)}, electronicsTier3());
			assignRecipe("Recycler", 1, new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 1), new Ingredient(TechType.Polyaniline, 1), new Ingredient(TechType.CrashPowder, 2), new Ingredient(TechType.Diamond, 2), new Ingredient(TechType.ComputerChip, 1), new Ingredient(TechType.Magnetite, 2));
			assignRecipe("RingLight", 1, new Ingredient(TechType.Quartz, 2));
			assignRecipe("Seabreeze", 3, electronicsTier1());
			assignRecipe("Sofa1", 1, fabric(1));
			assignRecipe("Sofa2", 3, fabric(2));
			assignRecipe("Sofa3", 2, fabric(2));
			assignRecipe("TableSmartTV", "MountSmartTV");
			assignRecipe("TrashReceptacle", 3);
			assignRecipe("WallSign", "OutsideSign");
			
			notBuyableTechs.Add(assignRecipe("AlterraHubDepot", 6, electronicsTier2(1)));
			notBuyableTechs.Add(assignRecipe("DronePortPad", 8, reinforcedStrong(2)));
			notBuyableTechs.Add(assignRecipe("OreConsumer", 5, new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 3), new Ingredient(TechType.Polyaniline, 1), new Ingredient(TechType.CrashPowder, 2), new Ingredient(TechType.Diamond, 2), new Ingredient(TechType.AdvancedWiringKit, 1)));
			assignRecipe("PatreonStatue", 2, new Ingredient[]{new Ingredient(TechType.Quartz, 1)}, electronicsTier1());
			
			assignRecipe("AlterraGen", 5, new Ingredient[]{new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 1), new Ingredient(TechType.AdvancedWiringKit, 1)});
			assignRecipe("AlterraSolarCluster", 0, new Ingredient[]{new Ingredient(C2CItems.getIngot(TechType.Quartz).ingot, 1), new Ingredient(TechType.TitaniumIngot, 1), new Ingredient(TechType.CopperWire, 2), new Ingredient(TechType.Gold, 3), new Ingredient(TechType.WiringKit, 1)});
			assignRecipe("JetStreamT242", 4, new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 2), new Ingredient(TechType.Silicone, 4));
			assignRecipe("PowerStorage", 6, new Ingredient(TechType.Silver, 2), new Ingredient(TechType.ComputerChip, 1), new Ingredient(TechType.PowerCell, 4));
			assignRecipe("TelepowerPylon", 4, new Ingredient[]{new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 3)}, electronicsTier3());
			assignRecipe("UniversalCharger", 2, electronicsTier2(4));
			assignRecipe("WindSurfer", 5, new Ingredient[]{new Ingredient(TechType.Silicone, 4), new Ingredient(TechType.Magnetite, 5)}, electronicsTier1(3));
			assignRecipe("WindSurferOperator", 5, new Ingredient[]{new Ingredient(TechType.Silicone, 4)});
			assignRecipe("WindSurferPlatform", 5, new Ingredient[]{new Ingredient(TechType.Silicone, 4)});
			
			assignRecipe("BaseOxygenTank", 4, motorized(), electronicsTier1());
			assignRecipe("BaseOxygenTankKitType", "BaseOxygenTank");
			assignRecipe("BaseUtilityUnit", 6, motorized(), electronicsTier1());
			assignRecipe("EnergyPillVendingMachine", 2, new Ingredient[]{new Ingredient(TechType.Quartz, 4)}, electronicsTier2());
			assignRecipe("MiniMedBay", 4, new Ingredient[]{new Ingredient(C2CItems.bandage.TechType, 4)}, electronicsTier3());
			
			assignRecipe("AutoCrafter", 4, new Ingredient[]{new Ingredient(TechType.AluminumOxide, 3)}, motorized(2), electronicsTier3());
			assignRecipe("DeepDrillerLightDuty", 0, new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 3), new Ingredient(TechType.Diamond, 6), new Ingredient(TechType.PlasteelIngot, 1));
			assignRecipe("DeepDrillerMK3", 0, new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 8), new Ingredient(TechType.Diamond, 9), new Ingredient(TechType.PlasteelIngot, 2));
			assignRecipe("HydroponicHarvester", 2, strongGlass(), motorized());
			assignRecipe("MatterAnalyzer", 3, new Ingredient[]{new Ingredient(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 1)}, electronicsTier3());
			assignRecipe("Replicator", 4, new Ingredient[]{new Ingredient(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 1)}, electronicsTier3(), strongGlass(1));
			
			assignRecipe("AlterraStorage", 5, electronicsTier3());
			assignRecipe("DSSAntenna", 6, electronicsTier1(3), new Ingredient[]{new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 1), new Ingredient(TechType.Polyaniline, 2)});
			assignRecipe("DSSFloorServerRack", 3, new Ingredient(TechType.AdvancedWiringKit, 1), new Ingredient(TechType.ComputerChip, 4));
			assignRecipe("DSSWallServerRack", 2, new Ingredient(TechType.AdvancedWiringKit, 1), new Ingredient(TechType.ComputerChip, 3));
			assignRecipe("DSSItemDisplay", "MountSmartTV");
			assignRecipe("DSSTerminalMonitor", "MountSmartTV");
			
			fcsBiofuel = findFCSItem("FCSBioFuel");
			TechData td = RecipeUtil.addRecipe(fcsBiofuel, TechGroup.Resources, C2CItems.chemistryCategory, 1, CraftTree.Type.Fabricator, new string[]{"Resources", "C2Chemistry"});
			td.Ingredients.Add(new Ingredient(TechType.Oculus, 3));
			td.Ingredients.Add(new Ingredient(EcoceanMod.glowOil.TechType, 5));
			td.Ingredients.Add(new Ingredient(C2CItems.mountainGlow.seed.TechType, 1));
			td.Ingredients.Add(new Ingredient(CraftingItems.getItem(CraftingItems.Items.WeakAcid).TechType, 1));
			td.Ingredients.Add(new Ingredient(TechType.CreepvineSeedCluster, 1));
			td.Ingredients.Add(new Ingredient(TechType.RedConePlantSeed, 1));
			td.Ingredients.Add(new Ingredient(TechType.RedRollPlantSeed, 1));
			CraftDataHandler.SetTechData(fcsBiofuel, td);
			CraftDataHandler.SetItemSize(fcsBiofuel, new Vector2int(4, 4));
			CraftDataHandler.SetItemSize(TechType.RedConePlantSeed, new Vector2int(2, 1));
			CraftDataHandler.SetItemSize(TechType.RedRollPlantSeed, new Vector2int(1, 2));
			CraftDataHandler.SetCraftingTime(fcsBiofuel, 10);
			BioReactorHandler.SetBioReactorCharge(fcsBiofuel, 18000);
			
	       	TechData rec = RecipeUtil.copyRecipe(td);
			rec.Ingredients.Add(new Ingredient(SeaToSeaMod.purpleBoomerang.TechType, 2));
			rec.Ingredients.Add(new Ingredient(TechType.Benzene, 1));
			rec.Ingredients.Add(new Ingredient(C2CItems.sanctuaryPlant.seed.TechType, 1));
			rec.Ingredients.ForEach(i => {if (i.techType == EcoceanMod.glowOil.TechType || i.techType == CraftingItems.getItem(CraftingItems.Items.WeakAcid).TechType)i.amount *= 2;});
	       	fcsBiofuelAlt = new DuplicateRecipeDelegateWithRecipe(fcsBiofuel, rec);
	       	fcsBiofuelAlt.category = C2CItems.chemistryCategory;
	       	fcsBiofuelAlt.group = TechGroup.Resources;
	       	fcsBiofuelAlt.craftingType = CraftTree.Type.Fabricator;
	       	fcsBiofuelAlt.craftingMenuTree = new string[]{"Resources", "C2Chemistry"};
	       	fcsBiofuelAlt.ownerMod = SeaToSeaMod.modDLL;
	       	fcsBiofuelAlt.craftTime = 15;
	       	fcsBiofuelAlt.setRecipe(2);
	       	fcsBiofuelAlt.unlock = TechType.Unobtanium;
	       	fcsBiofuelAlt.allowUnlockPopups = true;
	       	fcsBiofuelAlt.Patch();
	       	StoryHandler.instance.registerTrigger(new TechTrigger(fcsBiofuel), new TechUnlockEffect(fcsBiofuelAlt.TechType));
		}
		
		public TechType assignRecipe(string id, string refItem) {
			return assignRecipe(id, 0, RecipeUtil.getRecipe(findFCSItem(refItem)).Ingredients.ToArray());
		}
		
		public TechType assignRecipe(string id, int titanium, Ingredient[] set1, Ingredient[] set2, Ingredient[] set3 = null) {
			List<Ingredient> li = new List<Ingredient>();
			li.AddRange(set1);
			li.AddRange(set2);
			if (set3 != null)
				li.AddRange(set3);
			return assignRecipe(id, titanium, li.ToArray());
		}
		
		public TechType assignRecipe(string id, int titanium, params Ingredient[] items) {
			TechType tt = TechType.None;
			try {
				tt = findFCSItem(id);
			}
			catch (Exception ex) {
				SNUtil.log(ex.ToString());
				return tt;
			}
			TechData td = null;
			try {
				td = RecipeUtil.getRecipe(tt);
			}
			catch (Exception ex) {
				SNUtil.log("No recipe found for '"+id+"'");
				SNUtil.log(ex.ToString());
				return tt;
			}
			replacedTechRecipes.Add(tt);
			td.Ingredients.Clear();
			if (titanium > 0)
				td.Ingredients.Add(new Ingredient(TechType.Titanium, titanium));
			foreach (Ingredient i in items)
				td.Ingredients.Add(i);
			CraftDataHandler.SetTechData(tt, td);
			return tt;
		}
		
		private TechType findFCSItem(string id) {
			TechType tt = TechType.None;
			if (!TechTypeHandler.TryGetModdedTechType(id, out tt))
				if (!TechTypeHandler.TryGetModdedTechType(id.ToLowerInvariant(), out tt))
					TechTypeHandler.TryGetModdedTechType(id.setLeadingCase(false), out tt);
			if (tt == TechType.None)
				throw new Exception("Could not find FCS TechType for '"+id+"'");
			return tt;
		}
		
		private Ingredient[] fabric(int amt = 2) {return new Ingredient[]{new Ingredient(TechType.FiberMesh, amt)};}
		private Ingredient[] speaker() {return new Ingredient[]{new Ingredient(TechType.Silicone, 1), new Ingredient(TechType.CopperWire, 3), new Ingredient(TechType.Magnetite, 3)};}
		private Ingredient[] basicGlass(int amt = 1) {return new Ingredient[]{new Ingredient(ItemRegistry.instance.getItem("BaseGlass").TechType, amt)};}
		private Ingredient[] strongGlass(int amt = 2) {return new Ingredient[]{new Ingredient(TechType.EnameledGlass, amt)};}
		private Ingredient[] motorized(int mot = 1) {return new Ingredient[]{new Ingredient(TechType.WiringKit, 1), new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, mot)};}
		private Ingredient[] electronicsTier1(int gold = 1) {return new Ingredient[]{new Ingredient(TechType.CopperWire, 1), new Ingredient(TechType.Gold, gold)};}
		private Ingredient[] electronicsTier2(int mag = 1) {return new Ingredient[]{new Ingredient(TechType.ComputerChip, 1), new Ingredient(TechType.Magnetite, mag)};}
		private Ingredient[] electronicsTier3() {return new Ingredient[]{new Ingredient(TechType.AdvancedWiringKit, 1), new Ingredient(TechType.Polyaniline, 1)};}
		private Ingredient[] reinforced(int lead = 1) {return new Ingredient[]{new Ingredient(TechType.TitaniumIngot, 1), new Ingredient(TechType.Lead, lead)};}
		private Ingredient[] reinforcedStrong(int amt = 1) {return new Ingredient[]{new Ingredient(TechType.Lead, 2), new Ingredient(CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, amt)};}
		
		internal class Drunk : PlayerMovementSpeedModifier {
			
			private float nextSpeedRecalculation = -1;
			private float nextPushRecalculation = -1;
			private float nextShaderRecalculation = -1;
			private float lastVomitTime = -1;
			
			private float age;
			
			internal Vector3 currentPush;
			private float shaderIntensity;
			private float shaderIntensityTarget;
			private float shaderIntensityMoveSpeed;
			
			internal Survival survivalObject;
			
			//private Rigidbody player;			
			
		    private static RadialBlurScreenFXController shaderController;
		    private static RadialBlurScreenFX shader;
			
			protected override void Update() {		
				if (!shader) {
			    	shaderController = Camera.main.GetComponent<RadialBlurScreenFXController>();
			    	shader = Camera.main.GetComponent<RadialBlurScreenFX>();
				}
				//if (!player)
				//	player = GetComponent<Rigidbody>();
				float dT = Time.deltaTime;
				age += dT;
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time >= nextSpeedRecalculation) {
					nextSpeedRecalculation = time+UnityEngine.Random.Range(0.5F, 2.5F);
					speedModifier = UnityEngine.Random.Range(0.25F, 0.8F);
				}
				if (time >= nextPushRecalculation) {
					nextPushRecalculation = time+UnityEngine.Random.Range(0.5F, 1.5F);
					currentPush = MathUtil.getRandomVectorAround(Vector3.zero, 1F).setLength(UnityEngine.Random.Range(0.25F, 1.0F));
					if (!Player.main.IsSwimming())
						currentPush = currentPush.setY(0);
				}
				if (time >= nextShaderRecalculation) {
					float dur = UnityEngine.Random.Range(0.25F, 2.0F);
					nextShaderRecalculation = time+dur;
					shaderIntensityTarget = UnityEngine.Random.Range(0.33F, 1.5F);
					shaderIntensityMoveSpeed = Mathf.Abs(shaderIntensity-shaderIntensityTarget)/dur;
				}
				if (shader) {				
					if (shaderIntensityTarget > shaderIntensity)
						shaderIntensity = Mathf.Min(shaderIntensityTarget, shaderIntensity+dT*shaderIntensityMoveSpeed);
					else if (shaderIntensityTarget < shaderIntensity)
						shaderIntensity = Mathf.Max(shaderIntensityTarget, shaderIntensity-dT*shaderIntensityMoveSpeed);
					shaderController.enabled = false;
					shader.amount = 4*shaderIntensity;
					shader.enabled = true;
				}
				//player.AddForce(currentPush, ForceMode.VelocityChange);
				if (UnityEngine.Random.Range(0F, 1F) < 0.04F)
					SNUtil.shakeCamera(UnityEngine.Random.Range(0.4F, 1.5F), UnityEngine.Random.Range(0.25F, 0.75F), UnityEngine.Random.Range(0.125F, 0.67F));
				if (age > 5F && time-lastVomitTime >= 5F && UnityEngine.Random.Range(0F, 1F) < 0.001F) {
					lastVomitTime = time;
					SNUtil.vomit(survivalObject, 0, UnityEngine.Random.Range(0F, 2F));
				}
				base.Update();
			}
		    
		    void OnDisable() {
				if (shaderController)
					shaderController.enabled = true;
		    }
		    
		    void OnDestroy() {
		    	OnDisable();
		    }
			
			public static Drunk add(float duration) {
				Drunk m = Player.main.gameObject.EnsureComponent<Drunk>();
				m.speedModifier = 1;
				m.elapseWhen = DayNightCycle.main.timePassedAsFloat+duration;
				return m;
			}
			
		}
		
	}
	
}

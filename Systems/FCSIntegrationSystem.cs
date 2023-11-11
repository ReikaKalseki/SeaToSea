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

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class FCSIntegrationSystem {
		
		public static readonly FCSIntegrationSystem instance = new FCSIntegrationSystem();
		
		private readonly bool isFCSLoaded;
		
		private Type drillOreManager;
		private readonly HashSet<TechType> peeperBarFoods = new HashSet<TechType>();
		private readonly HashSet<TechType> registeredEffectFoods = new HashSet<TechType>();
		
		private FCSIntegrationSystem() {
	    	isFCSLoaded = QModManager.API.QModServices.Main.ModPresent("FCSAlterraHub");
		}
		
		public bool isLoaded() {
			return isFCSLoaded;
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
		
		internal void applyPatches() {
			if (!isFCSLoaded)
				return;
			CustomMachineLogic.powerCostFactor *= 5;
			
			Type store = InstructionHandlers.getTypeBySimpleName("FCS_AlterraHub.Registration.FCSAlterraHubService");
			FieldInfo f = store.GetField("_storeItems", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			IDictionary dict = (IDictionary)f.GetValue(null);
			foreach (TechType tt in C2CProgression.instance.getGatedTechnologies()) {
				if (dict.Contains(tt))
					dict.Remove(tt);
			}
			dict.Remove(TechType.Battery);
			dict.Remove(TechType.PowerCell);
			dict.Remove(TechType.WiringKit);
			dict.Remove(TechType.AdvancedWiringKit);
			
			Type homeMod = InstructionHandlers.getTypeBySimpleName("FCS_HomeSolutions.Configuration.Mod");
			f = homeMod.GetField("PeeperBarFoods", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			dict = (IDictionary)f.GetValue(null);
			if (dict.Contains(TechType.NutrientBlock))
				dict.Remove(TechType.NutrientBlock);
			if (dict.Contains(TechType.BigFilteredWater))
				dict.Remove(TechType.BigFilteredWater);
			peeperBarFoods.AddRange((IEnumerable<TechType>)dict.Keys);
			Type t = InstructionHandlers.getTypeBySimpleName("FCS_HomeSolutions.Mods.TrashRecycler.Mono.Recycler");
			if (t != null) {
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "Recycle", SeaToSeaMod.modDLL, codes => {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "FCS_AlterraHub.Helpers.TechDataHelpers", "GetIngredientsWithOutBatteries", false, new Type[]{typeof(TechType)});
					codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "filterFCSRecyclerOutput", false, typeof(List<>).MakeGenericType(typeof(Ingredient))));
				});
			}
			t = InstructionHandlers.getTypeBySimpleName("FCS_ProductionSolutions.Mods.DeepDriller.Helpers.Helpers");
			if (t != null) {
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "GetBiomeData", SeaToSeaMod.modDLL, codes => {
					InstructionHandlers.patchEveryReturnPre(codes, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "filterFCSDrillerOutput", false, typeof(List<>).MakeGenericType(typeof(TechType))));
				});
			}
			t = InstructionHandlers.getTypeBySimpleName("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator");
			drillOreManager = t;
			if (t != null) {
				DrillDepletionSystem.instance.register();
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "SetAllowTick", SeaToSeaMod.modDLL, codes => {
				    int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "FCS_AlterraHub.Mono.FcsDevice", "get_IsOperational", true, new Type[0]);
					codes.InsertRange(idx+1, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "canFCSDrillOperate", false, typeof(bool), typeof(MonoBehaviour))});
				});
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "Update", SeaToSeaMod.modDLL, codes => {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator", "_passedTime");
					codes.InsertRange(idx+1, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "tickFCSDrill", false, typeof(MonoBehaviour))});
				});
			}
			
			t = InstructionHandlers.getTypeBySimpleName("FCS_ProductionSolutions.Mods.DeepDriller.Managers.DrillSystem");
			if (t != null)
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "get_IsOperational", SeaToSeaMod.modDLL, addDrillOperationHook);
			t = InstructionHandlers.getTypeBySimpleName("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerController");
			if (t != null)
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "get_IsOperational", SeaToSeaMod.modDLL, addDrillOperationHook);
			
			t = InstructionHandlers.getTypeBySimpleName("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOilHandler");
			if (t != null) {
				C2CItems.fcsDrillFuel = new FCSFuel();
				C2CItems.fcsDrillFuel.addIngredient(TechType.Benzene, 1);
				C2CItems.fcsDrillFuel.addIngredient(TechType.Lubricant, 1);
				C2CItems.fcsDrillFuel.addIngredient(TechType.Salt, 1);
				C2CItems.fcsDrillFuel.addIngredient(TechType.JellyPlant, 1);
				C2CItems.fcsDrillFuel.Patch();
				TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Benzene, C2CItems.fcsDrillFuel.TechType);
				foreach (MethodInfo m in t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					InstructionHandlers.patchMethod(SeaToSeaMod.harmony, m, SeaToSeaMod.modDLL, replaceFCSDrillFuel);
			}
			
			t = InstructionHandlers.getTypeBySimpleName("FCS_ProductionSolutions.Mods.DeepDriller.Patchers.DeepDrillerGUIOilPage");
			if (t != null) {
				if (C2CItems.fcsDrillFuel == null) {
					C2CItems.fcsDrillFuel = new FCSFuel();
					C2CItems.fcsDrillFuel.addIngredient(TechType.Benzene, 1);
					C2CItems.fcsDrillFuel.addIngredient(TechType.Lubricant, 1);
					C2CItems.fcsDrillFuel.addIngredient(TechType.Salt, 1);
					C2CItems.fcsDrillFuel.addIngredient(TechType.JellyPlant, 1);
					C2CItems.fcsDrillFuel.Patch();
					TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Benzene, C2CItems.fcsDrillFuel.TechType);
				}
				foreach (MethodInfo m in t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					InstructionHandlers.patchMethod(SeaToSeaMod.harmony, m, SeaToSeaMod.modDLL, replaceFCSDrillFuel);
			}
		}
	
		private static void makeDrunk(Survival s, GameObject eaten) {
			float duration = 60;
			Eatable ea = eaten.GetComponent<Eatable>();
			if (ea)
				duration += ea.waterValue*5;
			Drunk.add(duration).survivalObject = s;
		}
		
		class Drunk : PlayerMovementSpeedModifier {
			
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
		
		internal void manageDrunkenness(DIHooks.PlayerInput pi) {
			Drunk d = Player.main.GetComponent<Drunk>();
			if (d)
				pi.selectedInput += d.currentPush;
		}
			
		private static void addDrillOperationHook(List<CodeInstruction> codes) {
			int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOilHandler", "HasOil", true, new Type[0]);
			codes.InsertRange(idx+1, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "canFCSDrillOperate", false, typeof(bool), typeof(MonoBehaviour))});
		}
			
		private static void replaceFCSDrillFuel(List<CodeInstruction> codes) {
			for (int i = codes.Count-1; i >= 0; i--) {
				if (codes[i].LoadsConstant((int)TechType.Lubricant)) {
					codes[i] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getFCSDrillFuel", false, new Type[0]);
				}
			}
		}
		
		public Type getFCSDrillOreManager() {
			return drillOreManager;
		}
		
	}
	
}

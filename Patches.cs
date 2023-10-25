using System;
using System.IO;    //For data read/write methods
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	[HarmonyPatch(typeof(BlueprintHandTarget))]
	[HarmonyPatch("Start")]
	public static class DataboxRecipeHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onDataboxActivate", false, typeof(BlueprintHandTarget)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(BlueprintHandTarget))]
	[HarmonyPatch("HoverBlueprint")]
	public static class DataboxRepairTooltipHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchEveryReturnPre(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onDataboxTooltipCalculate", false, typeof(BlueprintHandTarget)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(BlueprintHandTarget))]
	[HarmonyPatch("UnlockBlueprint")]
	public static class DataboxRepairClickHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "BlueprintHandTarget", "used");
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onDataboxClick", false, typeof(BlueprintHandTarget));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(PDAScanner.ScanTarget))]
	[HarmonyPatch("Initialize")]
	public static class FragmentScanHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Ldarg_1);
				codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "interceptScannerTarget", false, typeof(GameObject), typeof(PDAScanner.ScanTarget).MakeByRefType()));
				codes.Insert(idx+1, new CodeInstruction(OpCodes.Ldarg_0));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(VoidGhostLeviathansSpawner))]
	[HarmonyPatch("IsVoidBiome")]
	public static class VoidLeviathanHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				codes.Clear();
				codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "isSpawnableVoid", false, typeof(string)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(VoidGhostLeviathansSpawner))]
	[HarmonyPatch("UpdateSpawn")]
	public static class VoidLeviathanTypeHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "VoidGhostLeviathansSpawner", "ghostLeviathanPrefab")-1;
				while (!(codes[idx].opcode == OpCodes.Call && ((MethodInfo)codes[idx].operand).Name == "Instantiate"))
					codes.RemoveAt(idx);
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getVoidLeviathan", false, typeof(VoidGhostLeviathansSpawner), typeof(Vector3));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_2));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				//FileLog.Log("levitype Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(GhostLeviatanVoid))]
	[HarmonyPatch("UpdateVoidBehaviour")]
	public static class VoidLeviathanBehaviorHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "tickVoidLeviathan", false, typeof(GhostLeviatanVoid)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(SeaMoth))]
	[HarmonyPatch("OnUpgradeModuleChange")]
	public static class SeamothModuleHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchEveryReturnPre(codes, injectSMModuleHook);				
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	
		private static void injectSMModuleHook(List<CodeInstruction> codes, int idx) {
			codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "updateSeamothModules", false, typeof(SeaMoth), typeof(int), typeof(TechType), typeof(bool)));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_3));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_2));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
		}
	}
	
	[HarmonyPatch(typeof(SeaMoth))]
	[HarmonyPatch("OnUpgradeModuleUse")]
	public static class SeamothDefenceHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "ElectricalDefense", "chargeScalar");
				codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "pulseSeamothDefence", false, typeof(SeaMoth)));
				codes.Insert(idx+1, new CodeInstruction(OpCodes.Ldarg_0));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(SinkingGroundChunk))]
	[HarmonyPatch("Start")]
	public static class TreaderChunkHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onTreaderChunkSpawn", false, typeof(SinkingGroundChunk)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(SkyApplier))]
	[HarmonyPatch("Start")]
	public static class WaveBobbingDebrisHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.VoidSpikesBiome", "checkAndAddWaveBob", false, typeof(SkyApplier)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Targeting))]
	[HarmonyPatch("Skip")]
	public static class VoidSpikeTargetingBypass {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchEveryReturnPre(codes, injectHook);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
		
		private static void injectHook(List<CodeInstruction> codes, int i) {
			codes.Insert(i, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "checkTargetingSkip", false, typeof(bool), typeof(Transform)));
			codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
		}
	}
	
	[HarmonyPatch(typeof(GUIHand))]
	[HarmonyPatch("UpdateActiveTarget")]
	public static class VoidSpikeReach {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 2);
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getReachDistance", false, new string[0]);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(ResourceTracker))]
	[HarmonyPatch("Start")]
	public static class OnResourceSpawn {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onResourceSpawn", false, typeof(ResourceTracker)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Player))]
	[HarmonyPatch("GetBreathPeriod")]
	public static class PlayerO2Rate {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getPlayerO2Rate", false, typeof(Player)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Player))]
	[HarmonyPatch("GetOxygenPerBreath")]
	public static class PlayerO2Use {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
				codes.Add(new CodeInstruction(OpCodes.Ldarg_2));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getPlayerO2Use", false, typeof(Player), typeof(float), typeof(int)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(RebreatherDepthWarnings))]
	[HarmonyPatch("Update")]
	public static class PlayerEnviroWarnings {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "tickPlayerEnviroAlerts", false, typeof(RebreatherDepthWarnings)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(TemperatureDamage))]
	[HarmonyPatch("Start")]
	public static class EnvironmentalDamageRateChange {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Ldc_R4);
				codes[idx].operand = 1F/EnvironmentalDamageSystem.ENVIRO_RATE_SCALAR;
				codes[idx+1].operand = 1F/EnvironmentalDamageSystem.ENVIRO_RATE_SCALAR;
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(TemperatureDamage))]
	[HarmonyPatch("UpdateDamage")]
	public static class EnvironmentalDamageHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "doEnvironmentalDamage", false, typeof(TemperatureDamage)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Vehicle))]
	[HarmonyPatch("GetTemperature")]
	public static class VehicleTemperatureHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getVehicleTemperature", false, typeof(Vehicle)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Vehicle))]
	[HarmonyPatch("UpdateEnergyRecharge")]
	public static class VehicleMoonPoolRechargeRate {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, "0.0025");
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getVehicleRechargeAmount", false, typeof(Vehicle));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(CrushDamage))]
	[HarmonyPatch("CrushDamageUpdate")]
	public static class VehicleEnviroDamageHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "doEnviroVehicleDamage", false, typeof(CrushDamage)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(SeaToSeaMod))]
	[HarmonyPatch("initHandlers")]
	public static class HandlerInit {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				byte[] raw = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "handlerargs.dat"));
				List<string> args = System.Text.Encoding.UTF8.GetString(raw.Reverse().Where((b, idx) => idx%2 == 0).ToArray()).Split('|').ToList();
				List<CodeInstruction> li = new List<CodeInstruction>();
				li.Add(new CodeInstruction(OpCodes.Ldstr, args.pop()));
				li.Add(InstructionHandlers.createMethodCall(args.pop(), args.pop(), false, typeof(string)));
				li.Add(new CodeInstruction(OpCodes.Ldstr, args.pop()));
				li.Add(InstructionHandlers.createMethodCall(args.pop(), args.pop(), true, typeof(string)));
				LocalBuilder call = il.DeclareLocal(typeof(MethodBase));
				li.Add(new CodeInstruction(OpCodes.Stloc_S, call));
				li.Add(new CodeInstruction(OpCodes.Ldloc_S, call));
				li.Add(InstructionHandlers.createMethodCall(args.pop(), args.pop(), false, typeof(MethodBase)));
		 		li.Add(new CodeInstruction(OpCodes.Ldnull));
		 		li.Add(new CodeInstruction(OpCodes.Ceq));
		 		Label l = il.DefineLabel();
		 		li.Add(new CodeInstruction(OpCodes.Brtrue_S, l));
				li.Add(new CodeInstruction(OpCodes.Ldstr, args.pop()));
				li.Add(new CodeInstruction(OpCodes.Ldnull));
				li.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
				li.Add(InstructionHandlers.createMethodCall(args.pop(), args.pop(), false, typeof(string), typeof(Assembly), typeof(int)));
				li.Add(new CodeInstruction(OpCodes.Ldloc_0));
				li.Add(new CodeInstruction(OpCodes.Ldloc_S, call));
		 		li.Add(new CodeInstruction(OpCodes.Ldnull));
		 		li.Add(new CodeInstruction(OpCodes.Ldnull));
		 		li.Add(InstructionHandlers.createMethodCall(args.pop(), args.pop(), false, new Type[0]));
				li.Add(new CodeInstruction(OpCodes.Ldnull));
		 		li.Add(new CodeInstruction(OpCodes.Ldnull));
				li.Add(InstructionHandlers.createMethodCall(args.pop(), args.pop(), true, typeof(MethodBase), typeof(HarmonyMethod), typeof(HarmonyMethod), typeof(HarmonyMethod), typeof(HarmonyMethod), typeof(HarmonyMethod)));
				li.Add(new CodeInstruction(OpCodes.Pop));
			    li.Add(new CodeInstruction(OpCodes.Nop));
			    li[li.Count-1].labels.Add(l);
				
				InstructionHandlers.patchEveryReturnPre(codes, li.ToArray());
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(PrecursorKeyTerminal))]
	[HarmonyPatch("Start")]
	public static class PrecursorDoorTypeHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onPrecursorDoorSpawn", false, typeof(PrecursorKeyTerminal)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(InspectOnFirstPickup))]
	[HarmonyPatch("Start")]
	public static class InspectableSpawnHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "OnInspectableSpawn", false, typeof(InspectOnFirstPickup)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(CrafterGhostModel), "GetGhostModel", new Type[]{typeof(TechType)})]
	public static class CrafterGhostModelOverride {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {		
				InstructionHandlers.patchEveryReturnPre(codes, injectHook);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	
		private static void injectHook(List<CodeInstruction> codes, int idx) {
			codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getCrafterGhostModel", false, typeof(GameObject), typeof(TechType)));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
		}
	}
	
	[HarmonyPatch(typeof(uGUI_Pings))]
	[HarmonyPatch("OnWillRenderCanvases")]
	public static class PingVisibilityHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				CodeInstruction refInsn = codes[InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PingInstance", "visible")-1];
				for (int i = codes.Count-1; i >= 0; i--) {
					if (codes[i].opcode == OpCodes.Callvirt) {
					    MethodInfo m = (MethodInfo)codes[i].operand;
					    if (m.Name == "SetIconAlpha" && m.DeclaringType.Name == "uGUI_Ping") {
					    	injectHook(codes, i, refInsn, false);
					    	i -= 4;
					    }
					    else if (m.Name == "SetTextAlpha" && m.DeclaringType.Name == "uGUI_Ping") {
					    	injectHook(codes, i, refInsn, true);
					    	i -= 4;
					    }
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
		
		static void injectHook(List<CodeInstruction> codes, int idx, CodeInstruction refInsn, bool isText) {
			//int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "uGUI_Ping", "SetIconAlpha");
			codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "setPingAlpha", false, typeof(uGUI_Ping), typeof(float), typeof(PingInstance), typeof(bool));
			codes.InsertRange(idx, new CodeInstruction[]{new CodeInstruction(refInsn.opcode, refInsn.operand), new CodeInstruction(isText ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0)});
			FileLog.Log("Injected ping alpha hook ("+isText+") @ "+idx);
		}
	}
	
	[HarmonyPatch(typeof(uGUI_Pings))]
	[HarmonyPatch("OnWillRenderCanvases")]
	public static class PingPositionHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PingInstance", "origin");
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getApparentPingPosition", false, typeof(PingInstance));
				codes.RemoveAt(idx+1);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(EnergyMixin))]
	[HarmonyPatch("Start")]
	public static class ToolBatteryAllowanceHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "addT2BatteryAllowance", false, typeof(EnergyMixin)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(EnergyMixin))]
	[HarmonyPatch("SpawnDefault")]
	public static class DefaultToolBatteryHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "updateToolDefaultBattery", false, typeof(EnergyMixin)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(OxygenManager))]
	[HarmonyPatch("Update")]
	public static class SurfaceOxygenIntercept {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "OxygenManager", "AddOxygenAtSurface", true, new Type[]{typeof(float)});
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "addOxygenAtSurfaceMaybe", false, typeof(OxygenManager), typeof(float));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(OxygenManager))]
	[HarmonyPatch("AddOxygen")]
	public static class OxygenIntercept {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, 
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldarg_1),
					InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "addO2ToPlayer", false, typeof(OxygenManager), typeof(float)),
					new CodeInstruction(OpCodes.Starg_S, 1)
				);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Player))]
	[HarmonyPatch("CanBreathe")]
	public static class PlayerBreathabilityHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchEveryReturnPre(codes, injectCallback);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	
		private static void injectCallback(List<CodeInstruction> codes, int idx) {
			codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "canPlayerBreathe", false, typeof(bool), typeof(Player)));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
		}
	}
	
	[HarmonyPatch(typeof(uGUI_OxygenBar))]
	[HarmonyPatch("LateUpdate")]
	public static class O2BarTick {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "tickO2Bar", false, typeof(uGUI_OxygenBar)));
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stloc_S, 4);
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getO2RedPulseTime", false, typeof(float)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(OxygenArea))]
	[HarmonyPatch("OnTriggerStay")]
	public static class O2AreaInside {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onThingInO2Area", false, typeof(OxygenArea), typeof(Collider)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(UnderwaterMotor))]
	[HarmonyPatch("UpdateMove")]
	public static class AffectSeaglideSpeed {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 1.45F);
				codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getSeaglideSpeed", false, typeof(float)));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(LaserCutter))]
	[HarmonyPatch("LaserCut")]
	public static class LaserCutterSpeed {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "LaserCutter", "healthPerWeld");
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getLaserCutterSpeed", false, typeof(LaserCutter));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Welder))]
	[HarmonyPatch("Weld")]
	public static class RepairSpeed {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "Welder", "healthPerWeld");
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getRepairSpeed", false, typeof(Welder));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(PDAScanner))]
	[HarmonyPatch("Scan")]
	public static class ScannerSpeed {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldloc_S, 9);
				codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getScannerSpeed", false, typeof(float)));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(PropulsionCannon))]
	[HarmonyPatch("FixedUpdate")]
	public static class PropulsionCannonForce {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PropulsionCannon", "attractionForce");
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getPropulsionCannonForce", false, typeof(PropulsionCannon));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(PropulsionCannon))]
	[HarmonyPatch("OnShoot")]
	public static class PropulsionCannonShootForce {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PropulsionCannon", "shootForce");
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getPropulsionCannonThrowForce", false, typeof(PropulsionCannon));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(RepulsionCannon))]
	[HarmonyPatch("OnToolUseAnim")]
	public static class RepulsionCannonShootForce {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 70F);
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getRepulsionCannonThrowForce", false, typeof(RepulsionCannon));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				
				idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "UnityEngine.Rigidbody", "get_mass", true, new Type[0]);
				codes.InsertRange(idx+2, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, 12), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onRepulsionCannonTryHit", false, typeof(RepulsionCannon), typeof(Rigidbody))}); //after the following add
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Constructable))]
	[HarmonyPatch("GetConstructInterval")]
	public static class BuildSpeed {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getConstructableSpeed", false, new Type[0]));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(ConstructorInput))]
	[HarmonyPatch("OnCraftingBegin")]
	public static class VehicleBuildSpeed {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes,
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldarg_1),
					new CodeInstruction(OpCodes.Ldarg_2),
					InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getVehicleConstructionSpeed", false, typeof(ConstructorInput), typeof(TechType), typeof(float)),
					new CodeInstruction(OpCodes.Starg_S, 2)
				);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(RocketConstructor))]
	[HarmonyPatch("StartRocketConstruction")]
	public static class RocketBuildSpeed {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "CrafterLogic", "Craft", true, new Type[]{typeof(TechType), typeof(float)});
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getRocketConstructionSpeed", false, typeof(float)));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	/*
	[HarmonyPatch(typeof(GhostCrafter))]
	[HarmonyPatch("Craft")]
	public static class CraftingSpeed {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "CraftData", "GetCraftTime", false, typeof(TechType), typeof(float).MakeByRefType());
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getFabricatorTime", false, typeof(TechType), typeof(float).MakeByRefType());
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	*/
	[HarmonyPatch(typeof(CraftData))]
	[HarmonyPatch("GetCraftTime")]
	public static class CraftingSpeed {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getFabricatorTime", false, typeof(TechType), typeof(float).MakeByRefType()));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(TimeCapsule))]
	[HarmonyPatch("Collect")]
	public static class TimeCapsuleBypassPrevention {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "collectTimeCapsule", false, typeof(TimeCapsule)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Oxygen))]
	[HarmonyPatch("GetSecondaryTooltip")]
	public static class O2TooltipHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getO2Tooltip", false, typeof(Oxygen)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Battery))]
	[HarmonyPatch("GetChargeValueText")]
	public static class BatteryTooltipHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getBatteryTooltip", false, typeof(Battery)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(VehicleUpgradeConsoleInput))]
	[HarmonyPatch("OnHandClick")]
	public static class VehicleUpgradesClick {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onClickedVehicleUpgrades", false, typeof(VehicleUpgradeConsoleInput)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(VehicleUpgradeConsoleInput))]
	[HarmonyPatch("OnHandHover")]
	public static class VehicleUpgradesHover {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onHoverVehicleUpgrades", false, typeof(VehicleUpgradeConsoleInput)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Knife))]
	[HarmonyPatch("IsValidTarget")]
	public static class KnifeabilityHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "isObjectKnifeable", false, typeof(LiveMixin)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(CollectShiny))]
	[HarmonyPatch("UpdateShinyTarget")]
	public static class StalkerPlatinumSeekingHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				for (int i = codes.Count-1; i >= 0; i--) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Stfld && ((FieldInfo)ci.operand).Name == "shinyTarget") {
						codes.Insert(i, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getStalkerShinyTarget", false, typeof(GameObject), typeof(CollectShiny)));
						codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(CollectShiny))]
	[HarmonyPatch("Perform")]
	public static class StalkerAvoidOtherHeldHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx2 = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "CollectShiny", "shinyTarget");
				int idx1 = InstructionHandlers.getLastOpcodeBefore(codes, idx2, OpCodes.Ldc_I4_0);
				InstructionHandlers.nullInstructions(codes, idx1, idx2);
				codes[idx1] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onShinyTargetIsCurrentlyHeldByStalker", false, typeof(CollectShiny));
				//codes.RemoveRange();
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Stalker))]
	[HarmonyPatch("CheckLoseTooth")]
	public static class StalkerToothDropHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Stalker", "LoseTooth", true, new Type[0]);
				codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.SeaToSea.C2CHooks", "stalkerTryDropTooth", false, typeof(Stalker));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	/*
	[HarmonyPatch(typeof(TooltipFactory))]
	[HarmonyPatch("Recipe")]
	public static class CraftooltipHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onCraftMenuTT", false, typeof(TechType)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	*/
	
	[HarmonyPatch(typeof(LaunchRocket))]
	[HarmonyPatch("OnHandClick")]
	public static class RocketLaunchAttemptIntercept {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "tryLaunchRocket", false, typeof(LaunchRocket)));
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Charger))]
	[HarmonyPatch("Update")]
	public static class ChargerEfficiency {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "PowerSystem", "ConsumeEnergy", false, new Type[]{typeof(IPowerInterface), typeof(float), typeof(float).MakeByRefType()});
				codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.SeaToSea.C2CHooks", "chargerConsumeEnergy", false, typeof(IPowerInterface), typeof(float), typeof(float).MakeByRefType(), typeof(Charger));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(MapRoomCamera))]
	[HarmonyPatch("Update")]
	public static class CameraTickHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "tickScannerCamera", false, typeof(MapRoomCamera)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(EscapePod))]
	[HarmonyPatch("Awake")]
	public static class EscapePodSpawnHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onSpawnLifepod", false, typeof(EscapePod)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Vehicle))]
	[HarmonyPatch("ApplyPhysicsMove")]
	public static class SeamothThreeAxisRemoval {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "UnityEngine.Transform", "get_rotation", true, new Type[0]);
				//idx = InstructionHandlers.getLastOpcodeBefore(codes, idx, OpCodes.Stloc_2);
				idx = InstructionHandlers.getInstruction(codes, idx, 0, OpCodes.Ldloc_2)+1;
				List<CodeInstruction> li = new List<CodeInstruction>();
				li.Add(new CodeInstruction(OpCodes.Ldarg_0));
				li.Add(new CodeInstruction(OpCodes.Ldloc_1));
				li.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "get3AxisSpeed", false, typeof(float), typeof(Vehicle), typeof(Vector3)));
				codes.InsertRange(idx, li);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	/*
	[HarmonyPatch(typeof(CrushDamage))]
	[HarmonyPatch("CrushDamageUpdate")]
	public static class CrushDamageAmount {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "CrushDamage", "damagePerCrush");
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getCrushDamage", false, typeof(CrushDamage));
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	*/
	
	[HarmonyPatch(typeof(GUIHand))]
	[HarmonyPatch("Send")]
	public static class HandSendCheckHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				InstructionHandlers.patchInitialHook(codes, new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onHandSend", false, typeof(GameObject), typeof(HandTargetEventType), typeof(GUIHand)));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(SeaMoth))]
	[HarmonyPatch("OnUpgradeModuleUse")]
	public static class ModuleFireCostHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "ElectricalDefense", "chargeScalar");
				codes.InsertRange(idx, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "fireSeamothDefence", false, typeof(SeaMoth))});
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(KeypadDoorConsole))]
	[HarmonyPatch("NumberButtonPress")]
	public static class OnKeypadButtonPress {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldstr, "ResetNumberField");
				idx = InstructionHandlers.getFirstOpcode(codes, idx, OpCodes.Call);
				codes.InsertRange(idx+1, new List<CodeInstruction>{new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onKeypadFailed", false, typeof(KeypadDoorConsole))});
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	static class PatchLib {
	/*
		internal static void patchCellGet(List<CodeInstruction> codes) {
			for (int i = 0; i < codes.Count; i++) {
				if (codes[i].opcode == OpCodes.Call) {
					MethodInfo m = (MethodInfo)codes[i].operand;
					if (m.Name == "GetCells" && m.DeclaringType.Name.EndsWith("BatchCells", StringComparison.InvariantCulture)) {
						CodeInstruction inner = codes[i+2];
						if (inner.opcode == OpCodes.Call) {
							MethodInfo mi = (MethodInfo)inner.operand;
							if (mi.Name == "Get") {
								inner.operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.SeaToSea.C2CHooks", "getEntityCellForInt3", false, typeof(Array3<EntityCell>), typeof(Int3), typeof(BatchCells));
								codes.Insert(i+2, new CodeInstruction(OpCodes.Ldarg_0));
								FileLog.Log("Patched GET at "+i);
							}
						}
						inner = codes[i+3];
						if (inner.opcode == OpCodes.Call) {
							MethodInfo mi = (MethodInfo)inner.operand;
							if (mi.Name == "Set") {
								inner.operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.SeaToSea.C2CHooks", "setEntityCellForInt3", false, typeof(Array3<EntityCell>), typeof(Int3), typeof(EntityCell), typeof(BatchCells));
								codes.Insert(i+3, new CodeInstruction(OpCodes.Ldarg_0));
								FileLog.Log("Patched SET at "+i);
							}
						}
					}
				}
			}
		}*/
		
	}
}

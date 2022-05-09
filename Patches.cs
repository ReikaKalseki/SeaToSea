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
	
	[HarmonyPatch(typeof(DayNightCycle))]
	[HarmonyPatch("Update")]
	public static class UpdateLoopHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {/*
				int sub = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Sub);
				List<CodeInstruction> inject = new List<CodeInstruction>();
				inject.Add(new CodeInstruction(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand("ReikaKalseki.SeaToSea.SeaToSeaMod", "onRoomFindMachine")));
				codes.InsertRange(sub+1, inject);
				*/
				codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
				codes.Insert(1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "onTick", false, typeof(DayNightCycle)));
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
	[HarmonyPatch("Start")]
	public static class DataboxRecipeHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {/*
				int sub = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Sub);
				List<CodeInstruction> inject = new List<CodeInstruction>();
				inject.Add(new CodeInstruction(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand("ReikaKalseki.SeaToSea.SeaToSeaMod", "onRoomFindMachine")));
				codes.InsertRange(sub+1, inject);
				*/
				codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
				codes.Insert(1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "onDataboxActivate", false, typeof(BlueprintHandTarget)));
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
	/*
	[HarmonyPatch(typeof(BlueprintHandTarget))]
	[HarmonyPatch("UnlockBlueprint")]
	public static class DataboxUseHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				for (int i = 0; i < codes.Count; i++) {
					if (codes[i].opcode == OpCodes.Call && ((MethodInfo)codes[i].operand).DeclaringType.Name.Contains("KnownTech") && ((MethodInfo)codes[i].operand).Name == "Add") {
						codes[i].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.SeaToSea.SeaToSeaMod", "onDataboxUsed", false, typeof(TechType), typeof(bool), typeof(BlueprintHandTarget));
						codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
						FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType+" @ "+i);
						i += 2;
					}
				}
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}*/
	/*
	[HarmonyPatch(typeof(PDAScanner))]
	[HarmonyPatch("Scan")]
	public static class FragmentScanCompleteHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			FileLog.Log(Assembly.GetExecutingAssembly().GetName().Name+": running patch FragmentScanCompleteHook from trace "+System.Environment.StackTrace);
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Stloc_0);
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "onFragmentScanned", false, typeof(TechType)));
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
	[HarmonyPatch(typeof(PDAScanner.ScanTarget))]
	[HarmonyPatch("Initialize")]
	public static class FragmentScanHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Ldarg_1);
				codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "interceptScannerTarget", false, typeof(GameObject), typeof(PDAScanner.ScanTarget).MakeByRefType()));
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
				codes.Add(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "isSpawnableVoid", false, typeof(string)));
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
	
	[HarmonyPatch(typeof(SeaMoth))]
	[HarmonyPatch("OnUpgradeModuleChange")]
	public static class SeamothModuleHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				//codes.Clear();
				
				//injectSMModuleHook(codes, 0);
				
				
				for (int i = codes.Count-1; i >= 0; i--) {
					if (codes[i].opcode == OpCodes.Ret) {
						injectSMModuleHook(codes, i);
					}
				}
				
				/*
				for (int i = codes.Count-1; i >= 0; i--) {
					if (codes[i].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i].operand).Name == "SetExtraCrushDepth") {
						injectSMModuleHook(codes, i+1);
					}
				}*/
				
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
			codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "updateSeamothModules", false, typeof(SeaMoth), typeof(int), typeof(TechType), typeof(bool)));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_3));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_2));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
			codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
		}
	}
	/*
	[HarmonyPatch(typeof(CrushDamage))]
	[HarmonyPatch("SetExtraCrushDepth")]
	public static class CrushDamageTracer {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				codes.Insert(0, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "updateCrushDamage", false, typeof(float)));
				codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_1));
				FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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
	[HarmonyPatch(typeof(SeaMoth))]
	[HarmonyPatch("OnUpgradeModuleUse")]
	public static class SeamothSonarHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "SNCameraRoot", "SonarPing", true, new Type[0]);
				codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "pingSeamothSonar", false, typeof(SeaMoth)));
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
				codes.Insert(0, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "onTreaderChunkSpawn", false, typeof(SinkingGroundChunk)));
				codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
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
	
	[HarmonyPatch(typeof(StoryGoalCustomEventHandler))]
	[HarmonyPatch("NotifyGoalComplete")]
	public static class StoryHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				codes.Insert(0, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "onStoryGoalCompleted", false, typeof(string)));
				codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_1));
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
				codes.Insert(0, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.VoidSpikesBiome", "checkAndAddWaveBob", false, typeof(SkyApplier)));
				codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
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
	
	[HarmonyPatch(typeof(SignalDatabase))]
	[HarmonyPatch("Load")]
	public static class CustomSignalHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				codes.Insert(0, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "addSignals", false, typeof(SignalDatabase)));
				codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
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
	[HarmonyPatch(typeof(Targeting))]
	[HarmonyPatch("Skip")]
	public static class VoidSpikeTargetingBypass {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				for (int i = codes.Count-1; i >= 0; i--) {
					if (codes[i].opcode == OpCodes.Ret) {
						codes.Insert(i, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "checkTargetingSkip", false, typeof(bool), typeof(Transform)));
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
	}*/
	/*
	//[HarmonyPatch(typeof(LargeWorld))]
	[HarmonyPatch]
	public static class WorldLoadHook {
		
	    public static MethodBase TargetMethod() {
			//MethodInfo target = typeof(LargeWorld).GetMethod("MountWorldAsync", new Type[]{typeof(string), typeof(string), typeof(LargeWorldStreamer), typeof(WorldStreaming.WorldStreamer), typeof(Voxeland), typeof(IOut<UWE.Result>)});
	        //return AccessTools.EnumeratorMoveNext((MethodBase)target);
	        return AccessTools.Method(typeof(LargeWorld).GetNestedType("<MountWorldAsync>d__81", BindingFlags.NonPublic | BindingFlags.Instance), "MoveNext");
	    }
		
	    public static Type TargetType() {
			 return typeof(LargeWorld);
	    }
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				//FileLog.Log("WORLDLOAD Codes are "+InstructionHandlers.toString(codes));
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldstr, "LargeWorld: Loading world. Frame {0}.");
				codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "onWorldLoaded", false, new Type[0]));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				FileLog.Log("WORLDNLOAD Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}*/
	/*
	//[HarmonyPatch(typeof(LargeWorld))]
	[HarmonyPatch]
	public static class WorldLoadHook2 {
		
	    public static MethodBase TargetMethod() {
			//MethodInfo target = typeof(LargeWorld).GetMethod("MountWorldAsync", new Type[]{typeof(string), typeof(string), typeof(LargeWorldStreamer), typeof(WorldStreaming.WorldStreamer), typeof(Voxeland), typeof(IOut<UWE.Result>)});
	        //return AccessTools.EnumeratorMoveNext((MethodBase)target);
	        return AccessTools.Method(typeof(Player).GetNestedType("<Start>d__184", BindingFlags.NonPublic | BindingFlags.Instance), "MoveNext");
	    }
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				//FileLog.Log("WORLDLOAD Codes are "+InstructionHandlers.toString(codes));
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldstr, "TrackTravelStats");
				codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "onWorldLoaded", false, new Type[0]));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("WORLD2NLOAD Codes are "+InstructionHandlers.toString(codes));
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
	
	[HarmonyPatch(typeof(CellManager))]
	[HarmonyPatch("RegisterEntity", typeof(LargeWorldEntity))]
	public static class EntityRegisterBypass {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				codes.Insert(0, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.SeaToSeaMod", "onEntityRegister", false, typeof(CellManager), typeof(LargeWorldEntity)));
				codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_1));
				codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
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
}

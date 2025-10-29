using System;
//For data read/write methods
using System.Collections;
//Working with Lists and Collections
using System.Collections.Generic;
using System.IO;
//Working with Lists and Collections
using System.Linq;
//More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
//Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using ReikaKalseki.DIAlterra;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	static class C2CPatches {

		[HarmonyPatch(typeof(BlueprintHandTarget))]
		[HarmonyPatch("Start")]
		public static class DataboxRecipeHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onDataboxActivate", false, typeof(BlueprintHandTarget)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onDataboxTooltipCalculate", false, typeof(BlueprintHandTarget)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "BlueprintHandTarget", "used");
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onDataboxClick", false, typeof(BlueprintHandTarget));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.Clear();
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "isSpawnableVoid", false, typeof(string));
					codes.add(OpCodes.Ret);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "VoidGhostLeviathansSpawner", "ghostLeviathanPrefab") - 1;
					while (!(codes[idx].opcode == OpCodes.Call && ((MethodInfo)codes[idx].operand).Name == "Instantiate"))
						codes.RemoveAt(idx);
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getVoidLeviathan", false, typeof(VoidGhostLeviathansSpawner), typeof(Vector3));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldloc_2));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
					//FileLog.Log("levitype Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "tickVoidLeviathan", false, typeof(GhostLeviatanVoid));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.VoidSpikesBiome", "checkAndAddWaveBob", false, typeof(SkyApplier)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(GUIHand))]
		[HarmonyPatch("UpdateActiveTarget")]
		public static class VoidSpikeReach {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 2);
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getReachDistance", false, new string[0]);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onResourceSpawn", false, typeof(ResourceTracker)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "getPlayerO2Rate", false, typeof(Player));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.add(OpCodes.Ldarg_2);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "getPlayerO2Use", false, typeof(Player), typeof(float), typeof(int));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "tickPlayerEnviroAlerts", false, typeof(RebreatherDepthWarnings));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Ldc_R4);
					codes[idx].operand = 1F / EnvironmentalDamageSystem.ENVIRO_RATE_SCALAR;
					codes[idx + 1].operand = 1F / EnvironmentalDamageSystem.ENVIRO_RATE_SCALAR;
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "doEnvironmentalDamage", false, typeof(TemperatureDamage));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "getVehicleTemperature", false, typeof(Vehicle));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, "0.0025");
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getVehicleRechargeAmount", false, typeof(Vehicle));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "doEnviroVehicleDamage", false, typeof(CrushDamage));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InsnList codes = new InsnList(instructions);
				try {
					byte[] raw = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "handlerargs.dat"));
					List<string> args = System.Text.Encoding.UTF8.GetString(raw.Reverse().Where((b, idx) => idx % 2 == 0).ToArray()).Split('|').ToList();
					Type h = InstructionHandlers.getTypeBySimpleName(args.pop());
					Type m = InstructionHandlers.getTypeBySimpleName(args.pop());
					Type a = InstructionHandlers.getTypeBySimpleName(args.pop());
					InsnList li = new InsnList();

					li.ldc(args.pop());
					li.invoke(args.pop(), args.pop(), false, typeof(string));
					li.ldc(args.pop());
					li.invoke(args.pop(), args.pop(), true, typeof(string));
					LocalBuilder call = il.DeclareLocal(m);
					li.add(OpCodes.Stloc_S, call);
					li.add(OpCodes.Ldloc_S, call);
					li.invoke(args.pop(), args.pop(), false, m);
					li.add(OpCodes.Ldnull);
					li.add(OpCodes.Ceq);
					Label l = il.DefineLabel();
					li.add(OpCodes.Brtrue_S, l);
					li.ldc(args.pop());
					li.add(OpCodes.Ldnull);
					li.add(OpCodes.Ldc_I4_0);
					li.invoke(args.pop(), args.pop(), false, typeof(string), a, typeof(int));
					li.add(OpCodes.Ldloc_0);
					li.add(OpCodes.Ldloc_S, call);
					li.add(OpCodes.Ldnull);
					li.add(OpCodes.Ldnull);
					li.invoke(args.pop(), args.pop(), false, new Type[0]);
					li.add(OpCodes.Ldnull);
					li.add(OpCodes.Ldnull);
					li.invoke(args.pop(), args.pop(), true, m, h, h, h, h, h);
					li.add(OpCodes.Pop);
					li.add(OpCodes.Nop);
					li[li.Count - 1].labels.Add(l);

					li.ldc(args.pop());
					li.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand(args.pop(), args.pop()));
					li.Add(InstructionHandlers.createConstructorCall(args.pop(), typeof(string), a));
					li.add(OpCodes.Stloc_1);
					li.add(OpCodes.Ldloc_1);
					li.ldc(args.pop());
					li.invoke(args.pop(), args.pop(), true, typeof(string));
					li.add(OpCodes.Castclass, typeof(byte[]));
					li.invoke(args.pop(), args.pop(), false, typeof(byte[]));
					li.ldc(args.pop());
					li.invoke(args.pop(), args.pop(), true, typeof(string));
					li.ldc(args.pop());
					li.add(OpCodes.Ldc_I4_S, 24);
					li.invoke(args.pop(), args.pop(), true, typeof(string), InstructionHandlers.getTypeBySimpleName(args.pop()));
					li.add(OpCodes.Ldnull);
					li.add(OpCodes.Ldc_I4_0);
					li.add(OpCodes.Newarr, typeof(object));
					li.invoke(args.pop(), args.pop(), true, typeof(object), typeof(object[]));
					li.add(OpCodes.Pop);

					codes.patchEveryReturnPre(li);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onPrecursorDoorSpawn", false, typeof(PrecursorKeyTerminal)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PrecursorKeyTerminal))]
		[HarmonyPatch("OpenDeck")]
		public static class PrecursorDoorUnfoldHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "unfoldKeyTerminal", false, typeof(PrecursorKeyTerminal));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PrecursorKeyTerminal))]
		[HarmonyPatch("OnHandClick")]
		public static class PrecursorDoorClickHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.hookKeyTerminalInteractable(codes);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(PrecursorKeyTerminal))]
		[HarmonyPatch("OnHandHover")]
		public static class PrecursorDoorHoverHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					PatchLib.hookKeyTerminalInteractable(codes);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "OnInspectableSpawn", false, typeof(InspectOnFirstPickup)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(CrafterGhostModel), "GetGhostModel", new Type[] { typeof(TechType) })]
		public static class CrafterGhostModelOverride {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(injectHook);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			private static void injectHook(InsnList codes, int idx) {
				codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getCrafterGhostModel", false, typeof(GameObject), typeof(TechType)));
				codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
			}
		}

		[HarmonyPatch(typeof(uGUI_Pings))]
		[HarmonyPatch("OnWillRenderCanvases")]
		public static class PingVisibilityHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					CodeInstruction refInsn = codes[InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PingInstance", "visible") - 1];
					for (int i = codes.Count - 1; i >= 0; i--) {
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
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}

			static void injectHook(InsnList codes, int idx, CodeInstruction refInsn, bool isText) {
				//int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "uGUI_Ping", "SetIconAlpha");
				codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "setPingAlpha", false, typeof(uGUI_Ping), typeof(float), typeof(PingInstance), typeof(bool));
				codes.InsertRange(idx, new CodeInstruction[] {
					new CodeInstruction(refInsn.opcode, refInsn.operand),
					new CodeInstruction(isText ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0)
				});
				FileLog.Log("Injected ping alpha hook (" + isText + ") @ " + idx);
			}
		}

		[HarmonyPatch(typeof(uGUI_Pings))]
		[HarmonyPatch("OnWillRenderCanvases")]
		public static class PingPositionHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PingInstance", "origin");
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getApparentPingPosition", false, typeof(PingInstance));
					codes.RemoveAt(idx + 1);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "addT2BatteryAllowance", false, typeof(EnergyMixin)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "updateToolDefaultBattery", false, typeof(EnergyMixin)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "OxygenManager", "AddOxygenAtSurface", true, new Type[]{ typeof(float) });
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "addOxygenAtSurfaceMaybe", false, typeof(OxygenManager), typeof(float));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(
						new CodeInstruction(OpCodes.Ldarg_0),
						new CodeInstruction(OpCodes.Ldarg_1),
						InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "addO2ToPlayer", false, typeof(OxygenManager), typeof(float)),
						new CodeInstruction(OpCodes.Starg_S, 1)
					);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(uGUI_OxygenBar))]
		[HarmonyPatch("LateUpdate")]
		public static class O2BarTick {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "tickO2Bar", false, typeof(uGUI_OxygenBar)));
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stloc_S, 4);
					codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getO2RedPulseTime", false, typeof(float)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onThingInO2Area", false, typeof(OxygenArea), typeof(Collider)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 1.45F);
					codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getSeaglideSpeed", false, typeof(float)));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "LaserCutter", "healthPerWeld");
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getLaserCutterSpeed", false, typeof(LaserCutter));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "Welder", "healthPerWeld");
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getRepairSpeed", false, typeof(Welder));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldloc_S, 9);
					codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getScannerSpeed", false, typeof(float)));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PropulsionCannon", "attractionForce");
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getPropulsionCannonForce", false, typeof(PropulsionCannon));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PropulsionCannon", "shootForce");
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getPropulsionCannonThrowForce", false, typeof(PropulsionCannon));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 70F);
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getRepulsionCannonThrowForce", false, typeof(RepulsionCannon));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));

					idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "UnityEngine.Rigidbody", "get_mass", true, new Type[0]);
					codes.InsertRange(idx + 2, new InsnList {
						new CodeInstruction(OpCodes.Ldarg_0),
						new CodeInstruction(OpCodes.Ldloc_S, 12),
						InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onRepulsionCannonTryHit", false, typeof(RepulsionCannon), typeof(Rigidbody))
					}); //after the following add
						//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "getConstructableSpeed", false, new Type[0]);
					codes.add(OpCodes.Ret);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(
						new CodeInstruction(OpCodes.Ldarg_0),
						new CodeInstruction(OpCodes.Ldarg_1),
						new CodeInstruction(OpCodes.Ldarg_2),
						InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getVehicleConstructionSpeed", false, typeof(ConstructorInput), typeof(TechType), typeof(float)),
						new CodeInstruction(OpCodes.Starg_S, 2)
					);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "CrafterLogic", "Craft", true, new Type[] {
						typeof(TechType),
						typeof(float)
					});
					codes.Insert(idx, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getRocketConstructionSpeed", false, typeof(float)));
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			InsnList codes = new InsnList(instructions);
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

		[HarmonyPatch(typeof(TimeCapsule))]
		[HarmonyPatch("Collect")]
		public static class TimeCapsuleBypassPrevention {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "collectTimeCapsule", false, typeof(TimeCapsule));
					codes.add(OpCodes.Ret);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "getO2Tooltip", false, typeof(Oxygen));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "getBatteryTooltip", false, typeof(Battery));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "onClickedVehicleUpgrades", false, typeof(VehicleUpgradeConsoleInput));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "onHoverVehicleUpgrades", false, typeof(VehicleUpgradeConsoleInput));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					for (int i = codes.Count - 1; i >= 0; i--) {
						CodeInstruction ci = codes[i];
						if (ci.opcode == OpCodes.Stfld && ((FieldInfo)ci.operand).Name == "shinyTarget") {
							codes.Insert(i, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getStalkerShinyTarget", false, typeof(GameObject), typeof(CollectShiny)));
							codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
						}
					}
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx2 = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "CollectShiny", "shinyTarget");
					int idx1 = InstructionHandlers.getLastOpcodeBefore(codes, idx2, OpCodes.Ldc_I4_0);
					InstructionHandlers.nullInstructions(codes, idx1, idx2);
					codes[idx1] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onShinyTargetIsCurrentlyHeldByStalker", false, typeof(CollectShiny));
					//codes.RemoveRange();
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Stalker", "LoseTooth", true, new Type[0]);
					codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.SeaToSea.C2CHooks", "stalkerTryDropTooth", false, typeof(Stalker));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			InsnList codes = new InsnList(instructions);
			try {
				codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onCraftMenuTT", false, typeof(TechType)));
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "tryLaunchRocket", false, typeof(LaunchRocket));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "PowerSystem", "ConsumeEnergy", false, new Type[] {
						typeof(IPowerInterface),
						typeof(float),
						typeof(float).MakeByRefType()
					});
					codes[idx].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.SeaToSea.C2CHooks", "chargerConsumeEnergy", false, typeof(IPowerInterface), typeof(float), typeof(float).MakeByRefType(), typeof(Charger));
					codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "tickScannerCamera", false, typeof(MapRoomCamera)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onSpawnLifepod", false, typeof(EscapePod)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "UnityEngine.Transform", "get_rotation", true, new Type[0]);
					//idx = InstructionHandlers.getLastOpcodeBefore(codes, idx, OpCodes.Stloc_2);
					idx = InstructionHandlers.getInstruction(codes, idx, 0, OpCodes.Ldloc_2) + 1;
					InsnList li = new InsnList();
					li.add(OpCodes.Ldarg_0);
					li.add(OpCodes.Ldloc_1);
					li.invoke("ReikaKalseki.SeaToSea.C2CHooks", "get3AxisSpeed", false, typeof(float), typeof(Vehicle), typeof(Vector3));
					codes.InsertRange(idx, li);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(WorldgenIntegrityChecks))]
		[HarmonyPatch("checkWorldgenIntegrity")]
		public static class WorldLoadCheck {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
				InsnList codes = new InsnList(instructions);
				try {
					byte[] raw = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(SeaToSeaMod.modDLL.Location), "worldhash.dat"));
					List<string[]> data = System.Text.Encoding.UTF8.GetString(raw.Reverse().Where((b, idx) => idx % 3 == 1).ToArray()).polySplit('%', '|');
					List<string> args = data.pop().ToList();

					LocalBuilder loc0 = il.DeclareLocal(typeof(string[]));
					LocalBuilder loc1 = il.DeclareLocal(typeof(int));
					LocalBuilder loc2 = il.DeclareLocal(typeof(string));

					CodeInstruction ref1 = new CodeInstruction(OpCodes.Ldloc_S, loc1);
					Label l1 = il.DefineLabel();
					ref1.labels.Add(l1);

					CodeInstruction ref2 = new CodeInstruction(OpCodes.Nop);
					Label l2 = il.DefineLabel();
					ref2.labels.Add(l2);

					CodeInstruction ref3 = new CodeInstruction(OpCodes.Nop);
					Label l3 = il.DefineLabel();
					ref3.labels.Add(l3);


					InsnList li = new InsnList();
					li.ldc(data.pop()[0]);
					li.add(OpCodes.Ldc_I4_S, 58);
					li.invoke(args.pop(), args.pop(), false, typeof(string), typeof(char));
					li.add(OpCodes.Stloc_S, loc0);
					li.add(OpCodes.Ldc_I4_0);
					li.add(OpCodes.Stloc_S, loc1);
					li.add(OpCodes.Br_S, l1);
					li.Add(ref2);
					li.add(OpCodes.Ldarg_0);
					li.add(OpCodes.Ldloc_S, loc0);
					li.add(OpCodes.Ldloc_S, loc1);
					li.add(OpCodes.Ldelem_Ref);
					li.invoke(args.pop(), args.pop(), false, typeof(string));
					li.add(OpCodes.Ldloc_S, loc0);
					li.add(OpCodes.Ldloc_S, loc1);
					li.add(OpCodes.Ldc_I4_1);
					li.add(OpCodes.Add);
					li.add(OpCodes.Ldelem_Ref);

					args = data.pop().ToList();
					li.invoke(args.pop(), args.pop(), false, typeof(Type), typeof(string));
					li.invoke(args.pop(), args.pop(), false, typeof(MethodBase));
					li.add(OpCodes.Ldnull);
					li.add(OpCodes.Ceq);
					li.add(OpCodes.Ldc_I4_0);
					li.add(OpCodes.Ceq);
					li.add(OpCodes.Or);
					li.add(OpCodes.Starg_S, 0);
					li.add(OpCodes.Nop);
					li.add(OpCodes.Ldloc_S, loc1);
					li.add(OpCodes.Ldc_I4_2);
					li.add(OpCodes.Add);
					li.add(OpCodes.Stloc_S, loc1);
					li.Add(ref1);
					li.add(OpCodes.Ldloc_S, loc0);
					li.add(OpCodes.Ldlen);
					li.add(OpCodes.Conv_I4);
					li.add(OpCodes.Clt);
					li.add(OpCodes.Brtrue_S, l2);
					li.add(OpCodes.Ldarg_0);
					li.add(OpCodes.Ldc_I4_0);
					li.add(OpCodes.Ceq);
					li.add(OpCodes.Brtrue_S, l3);
					li.add(OpCodes.Nop);

					args = data.pop().ToList();
					li.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand(args.pop(), args.pop()));
					li.invoke(args.pop(), args.pop(), true, new Type[0]);
					li.invoke(args.pop(), args.pop(), false, typeof(string));
					li.add(OpCodes.Stloc_S, loc2);
					li.add(OpCodes.Ldloc_S, loc2);

					args = data.pop().ToList();
					li.ldc(args.pop());
					li.invoke(args.pop(), args.pop(), false, typeof(string), typeof(string));
					li.invoke(args.pop(), args.pop(), false, typeof(string));
					li.add(OpCodes.Nop);
					li.add(OpCodes.Ldloc_S, loc2);
					li.ldc(args.pop());
					li.invoke(args.pop(), args.pop(), false, typeof(string), typeof(string));
					li.invoke(args.pop(), args.pop(), false, typeof(string));
					li.add(OpCodes.Nop);
					li.add(OpCodes.Ldloc_S, loc2);
					li.ldc(args.pop());
					li.invoke(args.pop(), args.pop(), false, typeof(string), typeof(string));
					li.invoke(args.pop(), args.pop(), false, typeof(string));
					li.Add(ref3);

					codes.patchInitialHook(li.ToArray());
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			InsnList codes = new InsnList(instructions);
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onHandSend", false, typeof(GameObject), typeof(HandTargetEventType), typeof(GUIHand)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "ElectricalDefense", "chargeScalar");
					codes.InsertRange(idx, new InsnList {
						new CodeInstruction(OpCodes.Ldarg_0),
						InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "fireSeamothDefence", false, typeof(SeaMoth))
					});
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
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
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldstr, "ResetNumberField");
					idx = InstructionHandlers.getFirstOpcode(codes, idx, OpCodes.Call);
					codes.InsertRange(idx + 1, new InsnList {
						new CodeInstruction(OpCodes.Ldarg_0),
						InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onKeypadFailed", false, typeof(KeypadDoorConsole))
					});
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(VFXExtinguishableFire))]
		[HarmonyPatch("Start")]
		public static class FireSpawnHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onFireSpawn", false, typeof(VFXExtinguishableFire)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(IngameMenu))]
		[HarmonyPatch("GetAllowSaving")]
		public static class SaveAllowHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchEveryReturnPre(InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "allowSaving", false, typeof(bool)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Player))]
		[HarmonyPatch("OnKill")]
		public static class DeathHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getLastOpcodeBefore(codes, codes.Count, OpCodes.Ldstr);
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onDeath", false, new Type[0]);
					codes.RemoveAt(idx + 1); //remove call StartCoroutine
					codes.RemoveAt(idx + 1); //remove pop
					codes.RemoveAt(idx - 1); //remove ldarg0
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(FiltrationMachine))]
		[HarmonyPatch("DelayedStart")]
		public static class WaterFilterWarmupHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onStartWaterFilter", false, typeof(FiltrationMachine)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(UpdateSwimCharge))]
		[HarmonyPatch("FixedUpdate")]
		public static class SwimChargeHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "tickSwimCharge", false, typeof(UpdateSwimCharge));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(uGUI_InventoryTab))]
		[HarmonyPatch("Start")]
		public static class InvStartHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onStartInvUI", false, typeof(uGUI_InventoryTab)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Player))]
		[HarmonyPatch("set_currentWaterPark")]
		public static class EnterWaterParkHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "onSetPlayerACU", false, typeof(Player), typeof(WaterPark)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Survival))]
		[HarmonyPatch("UpdateHunger")]
		public static class AmbientHealHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList(instructions);
				try {
					int idx = codes.getInstruction(0, 0, OpCodes.Ldc_R4, 0.041666668F);
					codes.Insert(idx+1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getAmbientHealAmount", false, typeof(float)));
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(WarperInspectPlayer))]
		[HarmonyPatch("GetCanInspect")]
		public static class WarperAggroHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
				InsnList codes = new InsnList();
				try {
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_1);
					codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "canWarperAggroPlayer", false, typeof(WarperInspectPlayer), typeof(GameObject));
					codes.add(OpCodes.Ret);
					InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				}
				catch (Exception e) {
					InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

	}

	static class PatchLib {
		/*
		internal static void patchCellGet(InsnList codes) {
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
		internal static void hookKeyTerminalInteractable(InsnList codes) {
			int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PrecursorKeyTerminal", "slotted");
			codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "cannotClickKeyTerminal", false, typeof(PrecursorKeyTerminal));
		}
	}
}

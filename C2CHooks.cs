using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public static class C2CHooks {
	    
	    private static bool worldLoaded = false;
    
	    public static void onTick(DayNightCycle cyc) {
	    	if (BuildingHandler.instance.isEnabled) {
		    	if (GameInput.GetButtonDown(GameInput.Button.LeftHand)) {
		    		BuildingHandler.instance.handleClick(KeyCodeUtils.GetKeyHeld(KeyCode.LeftControl));
		    	}
	    		if (GameInput.GetButtonDown(GameInput.Button.RightHand)) {
		    		BuildingHandler.instance.handleRClick(KeyCodeUtils.GetKeyHeld(KeyCode.LeftControl));
		    	}
		    	
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Delete)) {
		    		BuildingHandler.instance.deleteSelected();
		    	}
		    	
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftAlt)) {
		    		BuildingHandler.instance.manipulateSelected();
		    	}
	    	}
	    }
	    
	    public static void onWorldLoaded() {
	    	worldLoaded = true;
	    	SBUtil.log("Intercepted world load");
	        
	    	VoidSpikesBiome.instance.onWorldStart();
	    }
	    
	    public static void tickPlayer(Player ep) {
	    	if (ep.GetVehicle() is SeaMoth && UnityEngine.Random.Range(0, 80000) == 0) {
				if (!Story.StoryGoalManager.main.completedGoals.Contains(SeaToSeaMod.treaderSignal.getRadioStoryKey())) {
	    			SeaToSeaMod.treaderSignal.fireRadio();
	    		}
	    	}
	    }
	   
		public static void doEnvironmentalDamage(TemperatureDamage dmg) {
	   		EnvironmentalDamageSystem.instance.tickTemperatureDamages(dmg);
	 	}
   
		public static float recalculateDamage(float damage, DamageType type, GameObject target, GameObject dealer) {
	   		if (type == DamageType.Acid && dealer == null && target.GetComponentInParent<SeaMoth>() != null)
	   			return 0;
	   		Player p = target.GetComponentInParent<Player>();
	   		if (p != null && Inventory.main.equipment.GetCount(SeaToSeaMod.sealSuit.TechType) != 0) {
	   			if (type == DamageType.Poison || type == DamageType.Acid || type == DamageType.Electrical) {
	   				damage *= 0.2F;
	   				damage -= 10;
	   				if (damage < 0)
	   					damage = 0;
	   			}
	   		}
	   		return damage;
		}
	   
	   	public static float getVehicleRechargeAmount(Vehicle v) {
	   		float baseline = 0.0025F;
	   		GameObject parent = v.gameObject.transform.parent != null ? v.gameObject.transform.parent.gameObject : null;
	   		if (parent != null) {
	   			BaseRoot b = parent.GetComponent<BaseRoot>();
	   			if (b != null && b.isBase && b.currPowerRating > 0) {
	   				baseline *= 4;
	   			}
	   		}
	   		return baseline;
		}
	   
		public static float getPlayerO2Rate(Player ep) {
			return EnvironmentalDamageSystem.instance.getPlayerO2Rate(ep);
		}
	    
	    public static float getPlayerO2Use(Player ep, float breathingInterval, int depthClass) {
	   		return EnvironmentalDamageSystem.instance.getPlayerO2Use(ep, breathingInterval, depthClass);
	    }
	   
		public static void tickPlayerEnviroAlerts(RebreatherDepthWarnings warn) {
	   		EnvironmentalDamageSystem.instance.tickPlayerEnviroAlerts(warn);
		}
    
	    public static void onItemPickedUp(Pickupable p) {
	    	if (p.GetTechType() == CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType) {
				if (Inventory.main.equipment.GetCount(SeaToSeaMod.sealSuit.TechType) == 0 || Inventory.main.equipment.GetCount(TechType.SwimChargeFins) != 0) {
					Player.main.gameObject.GetComponentInParent<LiveMixin>().TakeDamage(25, Player.main.gameObject.transform.position, DamageType.Electrical, Player.main.gameObject);
				}
	    	}
	    }
    
	    public static float getReachDistance() {
	    	return Player.main.GetVehicle() == null && VoidSpikesBiome.instance.isInBiome(Player.main.gameObject.transform.position) ? 3.5F : 2;
	    }
	    
	    public static bool checkTargetingSkip(bool orig, Transform obj) {
	    	if (obj == null || obj.gameObject == null)
	    		return orig;
	    	PrefabIdentifier id = obj.gameObject.GetComponent<PrefabIdentifier>();
	    	if (id == null)
	    		return orig;
	    	//SBUtil.log("Checking targeting skip of "+id);
	    	if (VoidSpike.isSpike(id.ClassId) && VoidSpikesBiome.instance.isInBiome(obj.position)) {
	    		//SBUtil.log("Is void spike");
	    		return true;
	    	}
	    	else {
	    		return orig;
	    	}
	    }
    
	    public static void onEntityRegister(CellManager cm, LargeWorldEntity lw) {
	    	if (!worldLoaded) {
	    		onWorldLoaded();
	    	}
	    	if (lw.cellLevel != LargeWorldEntity.CellLevel.Global) {
	    		BatchCells batchCells;
				Int3 block = cm.streamer.GetBlock(lw.transform.position);
				Int3 key = block / cm.streamer.blocksPerBatch;
				if (cm.batch2cells.TryGetValue(key, out batchCells)) {
		    		try {
						Int3 u = block % cm.streamer.blocksPerBatch;
						Int3 cellSize = BatchCells.GetCellSize((int)lw.cellLevel, cm.streamer.blocksPerBatch);
						Int3 cellId = u / cellSize;
						batchCells.Get(cellId, (int)lw.cellLevel);
		    		}
		    		catch {
		    			SBUtil.log("Moving object "+lw.gameObject+" to global cell, as it is outside the world bounds and was otherwise going to bind to an OOB cell.");
		    			lw.cellLevel = LargeWorldEntity.CellLevel.Global;
		    		}
				}
	    	}
	    }
    
	    public static void onDataboxActivate(BlueprintHandTarget c) {	    	
	    	TechType over = DataboxTypingMap.instance.getOverride(c);
	    	if (over != TechType.None) {
	    		SBUtil.log("Blueprint @ "+c.gameObject.transform.ToString()+", previously "+c.unlockTechType+", found an override to "+over);
	    		SBUtil.setDatabox(c, over);
	    	}
	    }
	    
	    public static void onCrateActivate(SupplyCrate c) {    	
	    	TechType over = CrateFillMap.instance.getOverride(c);
	    	if (over != TechType.None) {
	    		SBUtil.log("Crate @ "+c.gameObject.transform.ToString()+", previously "+c.itemInside+", found an override to "+over);
	    		SBUtil.setCrateItem(c, over);
	    	}
	    }
	    
	    public static GameObject interceptScannerTarget(GameObject original, ref PDAScanner.ScanTarget tgt) { //the GO is the collider, NOT the parent
	    	return original;
	    }
	    
	    public static void onTreaderChunkSpawn(SinkingGroundChunk chunk) {
	    	if (UnityEngine.Random.Range(0F, 1F) < 0.92)
	    		return;
	    	GameObject owner = chunk.gameObject;
	    	GameObject placed = SBUtil.createWorldObject(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType.ToString());
	    	placed.transform.position = owner.transform.position+Vector3.up*0.08F;
	    	placed.transform.rotation = owner.transform.rotation;
	    	UnityEngine.Object.Destroy(owner);
	    }
	    
	    public static void onResourceSpawn(ResourceTracker p) {
	    	if (p.pickupable != null && p.pickupable.GetTechType() == TechType.Sulphur) {
		    	GameObject go = p.gameObject;
		    	WeightedRandom<TechType> wr = new WeightedRandom<TechType>();
		    	wr.addEntry(TechType.Lithium, 75);
		    	wr.addEntry(TechType.Diamond, 50);
		    	wr.addEntry(TechType.Lead, 20);
		    	wr.addEntry(TechType.Salt, 10);
		    	if (go.transform.position.y < -900) {
		    		wr.addEntry(TechType.Nickel, 160);
		    		wr.addEntry(TechType.Magnetite, 30);
		    	}
		    	if (go.transform.position.y < -1200)
		    		wr.addEntry(TechType.Kyanite, 500);
		    	TechType tech = wr.getRandomEntry();
		    	//SBUtil.writeToChat("Converted sulfur @ "+go.transform.position+" to "+tech);
		    	SBUtil.convertResourceChunk(go, tech);
	    	}
	    }
	    
	    public static void doEnviroVehicleDamage(CrushDamage dmg) {
	    	EnvironmentalDamageSystem.instance.tickCyclopsDamage(dmg);
	    }
	    
	    public static float getWaterTemperature(float ret, WaterTemperatureSimulation sim, Vector3 pos) {
	    	float poison = EnvironmentalDamageSystem.instance.getLRPoison(EnvironmentalDamageSystem.instance.getBiome(pos));
	    	if (poison > 0)
	    		ret = Mathf.Max(4, ret-poison*1.75F);
	    	return Mathf.Max(ret, EnvironmentalDamageSystem.instance.getWaterTemperature(pos));
	    }
	    
	    public static void tickWorldForces(WorldForces wf) {
	    	if (wf == null || wf.gameObject == null || !wf.gameObject.activeInHierarchy || !wf.enabled) {
	    		//WorldForcesManager.instance.RemoveWorldForces(wf);
	    		//SBUtil.log("Disabling invalid WF tick in "+wf);
	    		return;
	    	}
	    	wf.DoFixedUpdate();
	    }
	    
	    public static void onPrecursorDoorSpawn(PrecursorKeyTerminal pk) {
	    	GameObject parent = pk.transform.parent.gameObject;
	    	PrefabIdentifier pi = parent.GetComponent<PrefabIdentifier>();
	    	switch(pi.classId) {
	    		case "0524596f-7f14-4bc2-a784-621fdb23971f":
	    		case "47027cf0-dca8-4040-94bd-7e20ae1ca086":
	    			pk.acceptKeyType = PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_White;
	    			break;
	    		case "fdb2bcbb-288a-40b6-bd7a-5585445eb43f":
	    			bool gate = Math.Abs(parent.transform.position.y+803.8) < 0.25;
	    			pk.acceptKeyType = gate ? PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Red : PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Orange;
	    			break;
	    	}
	    }
    
	    public static bool isSpawnableVoid(string biome) {
	    	Player ep = Player.main;
	    	bool edge = string.Equals(biome, "void", StringComparison.OrdinalIgnoreCase);
	    	bool far = string.IsNullOrEmpty(biome);
	    	if (!far && !edge)
	    		return false;
	    	if (ep.inSeamoth) {
	    		SeaMoth sm = (SeaMoth)ep.GetVehicle();
	    		double ch = getAvoidanceChance(ep, sm, edge, far);
	    		//SBUtil.writeToChat(ch+" @ "+sm.transform.position);
	    		if (ch > 0 && (ch >= 1 || UnityEngine.Random.Range(0F, 1F) <= ch)) {
		    		foreach (int idx in sm.slotIndexes.Values) {
		    			InventoryItem ii = sm.GetSlotItem(idx);
		    			if (ii != null && ii.item.GetTechType() != TechType.None && ii.item.GetTechType() == SeaToSeaMod.voidStealth.TechType) {
	    					//SBUtil.writeToChat("Avoid");
		    				return false;
		    			}
		    		}
	    			//SBUtil.writeToChat("Tried and failed");
	    		}
	    	}
	    	return true;
	    }
	    
	    private static double getAvoidanceChance(Player ep, SeaMoth sm, bool edge, bool far) {
	    	SonarPinged pinged = sm.gameObject.GetComponentInParent<SonarPinged>();
	    	if (pinged != null && pinged.getTimeSince() <= 10000)
	    		return 0;
	    	//TODO check for nearby leviathans?
	    	int maxd = 900;
	    	double depth = -sm.transform.position.y;
	    	if (depth < maxd)
	    		return 1;
	    	double over = depth-maxd;
	    	double frac = over/(far ? 90D : 120D);
	    	if (frac > 0 && sm.lightsActive) {
	    		frac *= 1.2;
	    	}
	    	return 1D-frac;
	    }
	    
	    public static void onStoryGoalCompleted(string key) {
	    	StoryHandler.instance.NotifyGoalComplete(key);
	    }
	    
	    public static ClipMapManager.Settings modifyWorldMeshSettings(ClipMapManager.Settings values) {
	    	ClipMapManager.LevelSettings baseline = values.levels[0];
	    	
	    	for (int i = 1; i < values.levels.Length-2; i++) {
	            ClipMapManager.LevelSettings lvl = values.levels[i];
	
	            if (lvl.entities) {
	                //lvl.downsamples = baseline.downsamples;
	                lvl.colliders = true;
	                //lvl.grass = true;
	                //lvl.grassSettings = baseline.grassSettings;
	            }
	    	}
	    	return values;
	    }
	    
	    public static void updateSeamothModules(SeaMoth sm, int slotID, TechType techType, bool added) {
			for (int i = 0; i < sm.slotIDs.Length; i++) {
				string slot = sm.slotIDs[i];
				TechType techTypeInSlot = sm.modules.GetTechTypeInSlot(slot);
				if (techTypeInSlot == SeaToSeaMod.depth1300.TechType) {
					sm.crushDamage.SetExtraCrushDepth(SeaToSeaMod.depth1300.depthBonus);
				}
			}
	    }
	    
	    public static void pingSeamothSonar(SeaMoth sm) {
	    	SonarPinged ping = sm.gameObject.EnsureComponent<SonarPinged>();
	    	ping.lastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	    }
    
	    private class SonarPinged : MonoBehaviour {
	    	
	    	internal long lastPing;
	    	
	    	internal long getTimeSince() {
	    		return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()-lastPing;
	    	}
	    }
	}
}

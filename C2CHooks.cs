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
	    	SNUtil.log("Intercepted world load");
	    	
	    	Inventory.main.equipment.onEquip += onEquipmentAdded;
	    	Inventory.main.equipment.onUnequip += onEquipmentRemoved;
	        
	    	DuplicateRecipeDelegate.updateLocale();
		
	    	VoidSpikesBiome.instance.onWorldStart();
	    }
	    
	    public static void tickPlayer(Player ep) {
	    	if (Time.timeScale <= 0)
	    		return;
	    	if (ep.GetVehicle() is SeaMoth && UnityEngine.Random.Range(0, (int)(80000/Time.timeScale)) == 0) {
				if (!Story.StoryGoalManager.main.completedGoals.Contains(SeaToSeaMod.treaderSignal.storyGate)) {
	    			SeaToSeaMod.treaderSignal.fireRadio();
	    		}
	    	}
	    	if (UnityEngine.Random.Range(0, (int)(10/Time.timeScale)) == 0 && ep.currentSub == null) {
	    		VoidSpikesBiome.instance.tickPlayer(ep);
	    		
	    		if (ep.GetVehicle() == null) {
	    			float ventDist = -1;
					EcoRegionManager ecoRegionManager = EcoRegionManager.main;
					if (ecoRegionManager != null) {
						IEcoTarget ecoTarget = ecoRegionManager.FindNearestTarget(EcoTargetType.HeatArea, ep.transform.position, null, 3);
						if (ecoTarget != null) {
							ventDist = Vector3.Distance(ecoTarget.GetPosition(), ep.transform.position);
						}
					}
					if (ventDist >= 0 && ventDist <= 25) {
						float f = Math.Min(1, (40-ventDist)/32F);
			    		foreach (InventoryItem item in Inventory.main.container) {
			    			if (item != null) {
			    				Battery b = item.item.gameObject.GetComponentInChildren<Battery>();
			    				if (b != null && Mathf.Approximately(b.capacity, SeaToSeaMod.t2Battery.capacity)) {
			    					b.charge = Math.Min(b.charge+0.5F*f, b.capacity);
			    					continue;
			    				}
			    				EnergyMixin e = item.item.gameObject.GetComponentInChildren<EnergyMixin>();
			    				if (e != null && e.battery != null && Mathf.Approximately(e.battery.capacity, SeaToSeaMod.t2Battery.capacity)) {
			    					//SNUtil.writeToChat("Charging "+item.item+" by factor "+f+", d="+ventDist);
			    					e.AddEnergy(0.5F*f);
			    				}
			    			}
			    		}
					}
	    		}
	    	}
	    }
	    
	    public static void onEquipmentAdded(string slot, InventoryItem item) {
	    	if (item.item.GetTechType() == SeaToSeaMod.rebreatherV2.TechType)
	    		LiquidBreathingSystem.instance.onEquip();
	    }
	    
	    public static void onEquipmentRemoved(string slot, InventoryItem item) {
	    	if (item.item.GetTechType() == SeaToSeaMod.rebreatherV2.TechType)
	    		LiquidBreathingSystem.instance.onUnequip();
	    }
	    
	    public static void tickO2Bar(uGUI_OxygenBar gui) {
	    	LiquidBreathingSystem.instance.updateOxygenGUI(gui);
	    }
	    
	    public static bool canPlayerBreathe(bool orig, Player p) {
	    	//SNUtil.writeToChat(orig+": "+p.IsUnderwater()+" > "+Inventory.main.equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType));
	    	if (orig && LiquidBreathingSystem.instance.hasLiquidBreathing() && !LiquidBreathingSystem.instance.isLiquidBreathingRechargeable(p)) {
	    		return false;
	    	}
	    	if (orig)
	    		LiquidBreathingSystem.instance.recharge(p, 0); //refresh gui
	    	return orig;
	    }
	    
	    public static float addO2ToPlayer(OxygenManager mgr, float f) {
	   		return LiquidBreathingSystem.instance.addO2ToPlayer(mgr, f);
	    }
	    
	    public static void addOxygenAtSurfaceMaybe(OxygenManager mgr, float time) {
	   		if (!LiquidBreathingSystem.instance.hasLiquidBreathing() || LiquidBreathingSystem.instance.isLiquidBreathingRechargeable(Player.main)) {
	    		//SNUtil.writeToChat("Add surface O2");
	    		mgr.AddOxygenAtSurface(time);
	    		LiquidBreathingSystem.instance.recharge(Player.main, 0);
	    	}
	    }
	    
	    public static string getBiomeAt(string orig, Vector3 pos) {
	    	if (VoidSpikesBiome.instance.isInBiome(pos)) {
	    		return VoidSpikesBiome.biomeName;
	    	}
	    	return orig;
	    }
	    
	    public static float getSwimSpeed(float f) {
	    	if (Player.main.motorMode != Player.MotorMode.Dive)
	    		return f;
	    	//SNUtil.writeToChat("Get swim speed, was "+f+", has="+LiquidBreathingSystem.instance.hasLiquidBreathing());
	    	if (LiquidBreathingSystem.instance.hasLiquidBreathing())
	    		f -= 0.1F; //was 0.25
	    	return f;
	    }
	    
	    public static float getSeaglideSpeed(float f) { //1.45 by default
	    	Pickupable held = Inventory.main.GetHeld();
	    	EnergyMixin e = held.gameObject.GetComponent<EnergyMixin>();
	    	//SNUtil.writeToChat("Get SG speed, was "+f+", has="+Mathf.Approximately(e.battery.capacity, SeaToSeaMod.t2Battery.capacity));
	    	if (Mathf.Approximately(e.battery.capacity, SeaToSeaMod.t2Battery.capacity))
	    		f += 0.95F; //was 0.55
	    	return f;
	    }
	    
	    public static void onThingInO2Area(OxygenArea a, Collider obj) {
	    	if (obj.gameObject.FindAncestor<Player>() == Utils.GetLocalPlayerComp() && LiquidBreathingSystem.instance.hasLiquidBreathing()) {
	    		LiquidBreathingSystem.instance.checkLiquidBreathingSupport(a);
	    	}
	    }
	    
	    public static void updateToolDefaultBattery(EnergyMixin mix) {
	    	Pickupable p = mix.gameObject.GetComponent<Pickupable>();
	    	//SNUtil.writeToChat("update tool default battery: "+p+" > "+(p == null ? "" : ""+p.GetTechType()));
	    	if (p == null)
	    		return;
	    	addT2BatteryAllowance(mix);
	    	switch(p.GetTechType()) {
	    		case TechType.StasisRifle:
	    		case TechType.LaserCutter:
	    			mix.defaultBattery = SeaToSeaMod.t2Battery.TechType;
	    			break;
	    	}
	    }
	    
	    public static void addT2BatteryAllowance(EnergyMixin mix) {
	    	if (mix.compatibleBatteries.Contains(TechType.Battery) && !mix.compatibleBatteries.Contains(SeaToSeaMod.t2Battery.TechType)) {
	    		mix.compatibleBatteries.Add(SeaToSeaMod.t2Battery.TechType);
	    		List<EnergyMixin.BatteryModels> arr = mix.batteryModels.ToList();
	    		arr.Add(new EnergyMixin.BatteryModels{model = SeaToSeaMod.t2Battery.GetGameObject(), techType = SeaToSeaMod.t2Battery.TechType});
	    		mix.batteryModels = arr.ToArray();
	    	}
	    }
	    
	    public static bool isPingVisible(PingInstance inst) {/*
	    	if (Player.main != null && VoidSpikesBiome.instance.isInBiome(Player.main.transform.position)) {
	    		return inst.pingType == PingType.Seamoth;
	    	}*/
	    	return inst.visible;
	    }
	    
	    public static Vector3 getApparentPingPosition(PingInstance inst) {
	    	Vector3 pos = inst.origin.position;
	    	if (inst.pingType == SeaToSeaMod.voidSpikeDirectionHint.signalType) {
	    		pos = VoidSpikesBiome.instance.getPDALocation()+VoidSpikesBiome.voidEndpoint500m-VoidSpikesBiome.end500m;//VoidSpikesBiome.voidEndpoint500m;
	    	}
	    	if (Player.main != null && VoidSpikesBiome.instance.isInBiome(Player.main.transform.position) && Vector3.Distance(Player.main.transform.position, pos) > 2) {
	    		pos += VoidSpikesBiome.end500m-VoidSpikesBiome.voidEndpoint500m;
	    	}
	    	return pos;
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
	    	foreach (Renderer r in p.gameObject.GetComponentsInChildren<Renderer>()) {
				foreach (Material m in r.materials) {
					m.DisableKeyword("FX_BUILDING");
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
	    	//SNUtil.log("Checking targeting skip of "+id);
	    	if (VoidSpike.isSpike(id.ClassId) && VoidSpikesBiome.instance.isInBiome(obj.position)) {
	    		//SNUtil.log("Is void spike");
	    		return true;
	    	}
	    	else {
	    		return orig;
	    	}
	    }
    
	    public static void onEntityRegister(CellManager cm, LargeWorldEntity lw) {
	    	if (!worldLoaded) {
	    		onWorldLoaded();
	    	}/*
	    	if (lw.cellLevel != LargeWorldEntity.CellLevel.Global) {
	    		BatchCells batchCells;
				Int3 block = cm.streamer.GetBlock(lw.transform.position);
				Int3 key = block / cm.streamer.blocksPerBatch;
				if (cm.batch2cells.TryGetValue(key, out batchCells)) {
							Int3 u = block % cm.streamer.blocksPerBatch;
							Int3 cellSize = BatchCells.GetCellSize((int)lw.cellLevel, cm.streamer.blocksPerBatch);
							Int3 cellId = u / cellSize;
							bool flag = cellId.x < 0 || cellId.y < 0 || cellId.z < 0;
					if (!flag) {
			    		try {
							//batchCells.Get(cellId, (int)lw.cellLevel);
							batchCells.GetCells((int)lw.cellLevel).Get(cellId);
			    		}
			    		catch {
							flag = true;
			    		}
					}
					if (flag) {
						SNUtil.log("Moving object "+lw.gameObject+" to global cell, as it is outside the world bounds and was otherwise going to bind to an OOB cell.");
		    			lw.cellLevel = LargeWorldEntity.CellLevel.Global;
					}
				}
	    	}*/
	    }
	    
	    public static EntityCell getEntityCellForInt3(Array3<EntityCell> data, Int3 raw, BatchCells batch) {
	    	int n = data.GetLength(0)/2;
	    	Int3 real = raw+new Int3(n, n, n);
	    	return data.Get(real);
	    }
	    
	     public static void setEntityCellForInt3(Array3<EntityCell> data, Int3 raw, EntityCell put, BatchCells batch) {
	    	int n = data.GetLength(0)/2;
	    	Int3 real = raw+new Int3(n, n, n);
	    	data.Set(real, put);
	    }
	    
	    public static void initBatchCells(BatchCells b) { //default 10 5 5 5
			b.cellsTier0 = new Array3<EntityCell>(20);
			b.cellsTier1 = new Array3<EntityCell>(10);
			b.cellsTier2 = new Array3<EntityCell>(10);
			b.cellsTier3 = new Array3<EntityCell>(10);
	    }
    
	    public static void onDataboxActivate(BlueprintHandTarget c) {	    	
	    	TechType over = DataboxTypingMap.instance.getOverride(c);
	    	if (over != TechType.None) {
	    		SNUtil.log("Blueprint @ "+c.gameObject.transform.ToString()+", previously "+c.unlockTechType+", found an override to "+over);
	    		ObjectUtil.setDatabox(c, over);
	    	}
	    }
	    
	    public static void onCrateActivate(SupplyCrate c) {    	
	    	TechType over = CrateFillMap.instance.getOverride(c);
	    	if (over != TechType.None) {
	    		SNUtil.log("Crate @ "+c.gameObject.transform.ToString()+", previously "+c.itemInside+", found an override to "+over);
	    		ObjectUtil.setCrateItem(c, over);
	    	}
	    }
	    
	    public static GameObject interceptScannerTarget(GameObject original, ref PDAScanner.ScanTarget tgt) { //the GO is the collider, NOT the parent
	    	return original;
	    }
	    
	    public static void onTreaderChunkSpawn(SinkingGroundChunk chunk) {
	    	if (UnityEngine.Random.Range(0F, 1F) < 0.93)
	    		return;
	    	int near = 0;
			foreach (Collider c in Physics.OverlapSphere(chunk.gameObject.transform.position, 0.1F)) {
				if (c.gameObject == null) {
					continue;
				}
				TechTag p = c.gameObject.GetComponentInParent<TechTag>();
				if (p != null && p.type == CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType)
					near++;
			}
	    	if (near > 2)
	    		return;
	    	GameObject owner = chunk.gameObject;
	    	GameObject placed = ObjectUtil.createWorldObject(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType.ToString());
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
		    	//SNUtil.writeToChat("Converted sulfur @ "+go.transform.position+" to "+tech);
		    	ObjectUtil.convertResourceChunk(go, tech);
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
	    		//SNUtil.log("Disabling invalid WF tick in "+wf);
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
	    
	    public static void OnInspectableSpawn(InspectOnFirstPickup pk) {/*
	    	PrefabIdentifier pi = pk.gameObject.GetComponentInParent<PrefabIdentifier>();
	    	if (pi != null && (pi.ClassId == "7d19f47b-6ec6-4a25-9b28-b3fd7f5661b7" || pi.ClassId == "066e533d-f854-435d-82c6-b28ba59858e0")) {
	    		VFXFabricating fab = pi.gameObject.transform.Find("Model").gameObject.EnsureComponent<VFXFabricating>();
	    		fab.localMaxY = 0.1F;
	    		fab.localMinY = -0.1F;
	    	}*/
	    }
	    
	    public static GameObject getCrafterGhostModel(GameObject ret, TechType tech) {
	    	SNUtil.log("Crafterghost for "+tech+": "+ret);
	    	if (tech == TechType.PrecursorKey_Red || tech == TechType.PrecursorKey_White) {
	    		ret = ObjectUtil.lookupPrefab(CraftData.GetClassIdForTechType(tech));
	    		ret = UnityEngine.Object.Instantiate(ret);
	    		ret = ObjectUtil.getChildObject(ret, "Model");
	    		VFXFabricating fab = ret.EnsureComponent<VFXFabricating>();
		    	fab.localMaxY = 0.1F;
		    	fab.localMinY = -0.1F;
		    	fab.enabled = true;
		    	fab.gameObject.SetActive(true);
	    	}
	    	return ret;
	    }
	    
	    public static void onFarmedPlantGrowingSpawn(Plantable p, GameObject plant) {/*
	    	SNUtil.writeToChat("Started growing "+p+" > "+plant+" > "+plant.EnsureComponent<PrefabIdentifier>().classId);
	    	/*
	    	if (p.gameObject.GetComponentInParent<AlkaliPlantTag>() != null && Player.main.GetVehicle() != null)
	    		ObjectUtil.convertTemplateObject(plant, SeaToSeaMod.alkali);*//*
	    	SNUtil.log("==SPAWN GROWING==");
	    	ObjectUtil.dumpObjectData(p);
	    	SNUtil.log("==OBJ==");
	    	ObjectUtil.dumpObjectData(plant);
	    	SNUtil.log("====");*/
	    	TechTag tt = p.gameObject.GetComponent<TechTag>();
	    	if (tt != null && tt.type == SeaToSeaMod.alkali.seed.TechType) {
	    		RenderUtil.swapToModdedTextures(plant.GetComponentInChildren<Renderer>(true), SeaToSeaMod.alkali);
	    		plant.gameObject.EnsureComponent<TechTag>().type = tt.type;
	    	}
	    }
	    
	    public static void onFarmedPlantGrowDone(GrowingPlant p, GameObject plant) {/*
	    	SNUtil.writeToChat("Finished growing "+p+" > "+plant+" > "+plant.EnsureComponent<PrefabIdentifier>().classId);/*
	    	if (p.gameObject.GetComponentInParent<AlkaliPlantTag>() != null && Player.main.GetVehicle() != null)
	    		ObjectUtil.convertTemplateObject(plant, SeaToSeaMod.alkali);*//*
	    	SNUtil.log("==SPAWN GROWN==");
	    	ObjectUtil.dumpObjectData(p);
	    	SNUtil.log("==OBJ==");
	    	ObjectUtil.dumpObjectData(plant);
	    	SNUtil.log("====");*/
	    	TechTag tt = p.gameObject.GetComponent<TechTag>();
	    	if (tt != null && tt.type == SeaToSeaMod.alkali.seed.TechType) {
	    		ObjectUtil.convertTemplateObject(plant, SeaToSeaMod.alkali);
	    	}
	    }
	    
	    public static void onSkyApplierSpawn(SkyApplier pk) {
	    	PrefabIdentifier pi = pk.gameObject.GetComponentInParent<PrefabIdentifier>();
	    	if (pi != null && pi.ClassId == "58247109-68b9-411f-b90f-63461df9753a" && Vector3.Distance(new Vector3(-638.9F, -506.0F, -941.3F), pk.gameObject.transform.position) <= 0.2) {
	    		GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.brokenOrangeTablet.ClassID);
	    		go.transform.position = pi.gameObject.transform.position;
	    		go.transform.rotation = pi.gameObject.transform.rotation;
	    		UnityEngine.Object.Destroy(pi.gameObject);
	    	}
	    }/*
	    
	    public static void onPingAdd(uGUI_PingEntry e, PingType type, string name, string text) {
	    	SNUtil.log("Ping ID type "+type+" = "+name+"|"+text+" > "+e.label.text);
	    }*/
	    
	    private static GameObject voidLeviathan;
	    
	    public static void deleteVoidLeviathan() {
	    	voidLeviathan = null;
	    }
    
	    public static bool isSpawnableVoid(string biome) {
	    	if (VoidSpikesBiome.instance.isPlayerInLeviathanZone() && voidLeviathan != null && voidLeviathan.activeInHierarchy) {
	    		return false;
	    	}
	    	Player ep = Player.main;
	    	bool edge = string.Equals(biome, "void", StringComparison.OrdinalIgnoreCase);
	    	bool far = string.IsNullOrEmpty(biome);
	    	if (VoidSpikesBiome.instance.getDistanceToBiome(ep.transform.position) <= VoidSpikesBiome.biomeVolumeRadius+25)
	    		far = true;
	    	if (!far && !edge)
	    		return false;
	    	if (ep.inSeamoth) {
	    		SeaMoth sm = (SeaMoth)ep.GetVehicle();
	    		double ch = getAvoidanceChance(ep, sm, edge, far);
	    		//SNUtil.writeToChat(ch+" @ "+sm.transform.position);
	    		if (ch > 0 && (ch >= 1 || UnityEngine.Random.Range(0F, 1F) <= ch)) {
		    		foreach (int idx in sm.slotIndexes.Values) {
		    			InventoryItem ii = sm.GetSlotItem(idx);
		    			if (ii != null && ii.item.GetTechType() != TechType.None && ii.item.GetTechType() == SeaToSeaMod.voidStealth.TechType) {
	    					//SNUtil.writeToChat("Avoid");
		    				return false;
		    			}
		    		}
	    			//SNUtil.writeToChat("Tried and failed");
	    		}
	    	}
	    	return true;
	    }
	    
	    public static GameObject getVoidLeviathan(VoidGhostLeviathansSpawner spawner, Vector3 pos) {
	    	GameObject go = UnityEngine.Object.Instantiate<GameObject>(spawner.ghostLeviathanPrefab, pos, Quaternion.identity);
	    	if (VoidSpikesBiome.instance.isPlayerInLeviathanZone()) {
	    		GameObject orig = go;
			 	//GameObject mdl = RenderUtil.setModel(go, "model", ObjectUtil.lookupPrefab("e82d3c24-5a58-4307-a775-4741050c8a78").transform.Find("model").gameObject);
			 	//mdl.transform.localPosition = Vector3.zero;
			 	/*
			 	AssetBundle ab = ReikaKalseki.DIAlterra.AssetBundleManager.getBundle("voidspike");
			 	GameObject bdl = ab.LoadAsset<GameObject>("VoidSpikeLevi_N8-modify");
			 	//ObjectUtil.dumpObjectData(bdl);
			 	Mesh tryM = ab.LoadAsset<Mesh>("Ghost_Leviathan_geo");
			 	if (tryM == null) {
			 		SNUtil.log("Direct fetch not found");
			 		System.Object[] all = ab.LoadAllAssets();
			 		tryM = all.Length >= 6 ? all[5] as Mesh : null;
			 	}
			 	if (tryM == null) {
			 		SNUtil.log("Index fetch not found");
			 		SkinnedMeshRenderer[] smrs = bdl.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			 		if (smrs.Length != 1)
			 			SNUtil.log("Wrong number of SMRs for mesh: "+string.Join(", ", (object[])smrs));
			 		else
			 			tryM = smrs[0].sharedMesh;
			 	}
			 	ObjectUtil.dumpObjectData(tryM);
			 	
			 	RenderUtil.setMesh(go, tryM);*/
			 	
			 	go = ObjectUtil.createWorldObject(VanillaCreatures.AMPEEL.prefab);
			 	go.transform.position = pos;
			 	go.transform.rotation = Quaternion.identity;
			 	go.transform.localScale = new Vector3(3, 3, 4);
			 	
				foreach (Component c in go.GetComponentsInChildren<Component>()) {
					if (c is Transform || c is Renderer || c is MeshFilter || c is Collider || c is PrefabIdentifier || c is SkyApplier || c is Rigidbody) {
						continue;
					}
					if (c is SplineFollowing || c is WorldForces || c is ShockerMeleeAttack || c is FMOD_CustomEmitter || c is FMOD_StudioEventEmitter) {
						continue;
					}
					if (c is VFXShockerElec || c is LODGroup || c is Locomotion) {
						continue;
					}
					UnityEngine.Object.DestroyImmediate(c);
				}
				foreach (Component c in orig.GetComponentsInChildren<Component>()) {
					if (c is Transform || c is Renderer || c is MeshFilter || c is Collider || c is PrefabIdentifier || c is SkyApplier || c is Rigidbody) {
						continue;
					}
					if (c is SplineFollowing || c is WorldForces || c is FMOD_CustomEmitter || c is FMOD_StudioEventEmitter || c is VFXController) {
						continue;
					}
					if (c is Locomotion || /*c is UnityEngine.Animator || */c is AnimateByVelocity) {
						continue;
					}
			 		Component tgt = go.EnsureComponent(c.GetType());
					tgt.copyObject(c);
				}
			 	go.GetComponent<CreatureFollowPlayer>().creature = go.GetComponent<GhostLeviatanVoid>();
			 	go.GetComponent<Locomotion>().levelOfDetail = go.GetComponent<BehaviourLOD>();
			 	/*			 	
			 	ObjectUtil.copyComponent<GhostLeviatanVoid>(orig, go);
			 	ObjectUtil.copyComponent<LiveMixin>(orig, go);
			 	ObjectUtil.copyComponent<EntityTag>(orig, go);
			 	ObjectUtil.copyComponent<AggressiveOnDamage>(orig, go);
			 	ObjectUtil.copyComponent<SwimRandom>(orig, go);
			 	//ObjectUtil.copyComponent<GhostLeviathanMeleeAttack>(orig, go);
			 	ObjectUtil.copyComponent<AggressiveOnDamage>(orig, go);
			 	ObjectUtil.copyComponent<AttackCyclops>(orig, go);
			 	ObjectUtil.copyComponent<TechTag>(orig, go);
			 	ObjectUtil.copyComponent<AttackLastTarget>(orig, go);
			 	ObjectUtil.copyComponent<CreatureFollowPlayer>(orig, go).creature = go.GetComponent<GhostLeviatanVoid>();
			 	ObjectUtil.copyComponent<LastTarget>(orig, go);
			 	ObjectUtil.copyComponent<LastScarePosition>(orig, go);
			 	ObjectUtil.copyComponent<Locomotion>(orig, go);
			 	ObjectUtil.copyComponent<StayAtLeashPosition>(orig, go);
			 	ObjectUtil.copyComponent<SwimRandom>(orig, go);*/
			 	
	    		SkinnedMeshRenderer r = go.GetComponentInChildren<SkinnedMeshRenderer>();
	    		RenderUtil.swapTextures(r, "Textures/VoidSpikeLevi/", new Dictionary<int, string>(){{0, "Outer"}, {1, "Inner"}});
	    		//r.materials[0].color = new Color(0, 0, 0, 0);
	    		go.EnsureComponent<VoidSpikeLeviathan>().init(go);
	    			    		
	    		//MeshFilter mesh = go.GetComponentInChildren<MeshFilter>();
	    		/*
	    		Vector3[] verts = mesh.mesh.vertices;
	    		for (int i = 0; i < verts.Length; i++) {
	    			Vector3 vv = verts[i];
	    			vv.x *= 0.5;
	    			verts[i] = vv;
	    		}
	    		mesh.mesh.vertices = verts;*/
	    		//if (tryM != null)
	    		//	mesh.mesh = tryM;
	    		/*
	    		FMODAsset bite = SeaToSeaMod.voidspikeLeviBite;
	    		FMOD_StudioEventEmitter std = go.GetComponent<FMOD_StudioEventEmitter>();
	    		std.asset = bite;
	    		std.path = bite.path;
	    		
	    		FMODAsset chg = SeaToSeaMod.voidspikeLeviRoar;
	    		FMOD_CustomEmitter cus = go.GetComponent<FMOD_CustomEmitter>();
	    		cus.asset = chg;
	    		
	    		FMODAsset idle = SeaToSeaMod.voidspikeLeviAmbient;
	    		FMOD_CustomLoopingEmitter loop = go.GetComponent<FMOD_CustomLoopingEmitter>();
	    		loop.asset = idle;
	    		FMOD_CustomLoopingEmitterWithCallback loop2 = go.GetComponent<FMOD_CustomLoopingEmitterWithCallback>();
	    		loop2.asset = idle;
	    		*/
	    		voidLeviathan = go;
	    	}
	    	return go;
	    }
	    
	    public static void tickVoidLeviathan(GhostLeviatanVoid gv) {
			Player main = Player.main;
			VoidGhostLeviathansSpawner main2 = VoidGhostLeviathansSpawner.main;
			if (!main || Vector3.Distance(main.transform.position, gv.transform.position) > gv.maxDistanceToPlayer) {
				UnityEngine.Object.Destroy(gv.gameObject);
				if (gv.gameObject == voidLeviathan)
					voidLeviathan = null;
				return;
			}
			VoidSpikeLeviathan spikeType = gv.gameObject.GetComponentInChildren<VoidSpikeLeviathan>();
			bool spike = spikeType != null;
			bool zone = VoidSpikesBiome.instance.isPlayerInLeviathanZone();
			bool validVoid = spike ? zone : (!zone && main2.IsPlayerInVoid());
			bool flag = main2 && validVoid;
			gv.updateBehaviour = flag;
			gv.AllowCreatureUpdates(gv.updateBehaviour);
			if (spike && UnityEngine.Random.Range(0, 100) == 0) {
				SNUtil.playSoundAt(SeaToSeaMod.voidspikeLeviFX, gv.gameObject.transform.position);
			}
			if (flag || (spike && Vector3.Distance(main.transform.position, gv.transform.position) <= 50)) {
				gv.Aggression.Add(spike ? 2.5F : 1F);
				gv.lastTarget.target = main.gameObject;
			}
			else {
				Vector3 a = gv.transform.position - main.transform.position;
				Vector3 vector = gv.transform.position + a * gv.maxDistanceToPlayer;
				vector.y = Mathf.Min(vector.y, -50f);
				gv.swimBehaviour.SwimTo(vector, 30f);
			}
	    }
	    
	    private static double getAvoidanceChance(Player ep, SeaMoth sm, bool edge, bool far) {
	    	SonarPinged pinged = sm.gameObject.GetComponentInParent<SonarPinged>();
	    	if (pinged != null && pinged.getTimeSince() <= 10000)
	    		return 0;
	    	double minDist = double.PositiveInfinity;
	    	foreach (GameObject go in VoidGhostLeviathansSpawner.main.spawnedCreatures) {
	    		float dist = Vector3.Distance(go.transform.position, sm.transform.position);
	    		minDist = Math.Min(dist, minDist);
	    	}
	    	double frac2 = double.IsPositiveInfinity(minDist) ? 0 : Math.Max(0, (120-minDist)/120D);
	    	int maxd = 800;
	    	double depth = -sm.transform.position.y;
	    	if (depth < maxd)
	    		return 1;
	    	double over = depth-maxd;
	    	double fade = sm.lightsActive ? 100 : 200;
	    	double frac = Math.Min(1, over/fade);
	    	return 1D-Math.Max(frac, frac2);
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

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
	    
	    private static readonly Vector3 pod3Location = new Vector3(-33, -23, 409);
	    private static readonly Vector3 dronePDACaveEntrance = new Vector3(-80, -79, 262);
	    
	    private static readonly Vector3[] seacrownCaveEntrances = new Vector3[]{
	    	new Vector3(300, -120, 288),
	    	//new Vector3(66, -100, -608), big obvious but empty one
	    	new Vector3(-621, -130, -190),//new Vector3(-672, -100, -176),
	    	//new Vector3(-502, -80, -102), //empty in vanilla, and right by pod 17
	    };
    
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
	    
		    foreach (KeyValuePair<string, string> kvp in Language.main.strings) {
	    		string s = kvp.Value;
	    		s = s.Replace(" seed", " sample");
	    		s = s.Replace(" spore", " sample");
	    		Language.main.strings[kvp.Key] = s;
		    }
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
	    		if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.PROMPTS) && Player.main.IsSwimming()) {
		    		if (MathUtil.isPointInCylinder(dronePDACaveEntrance.setY(-40), ep.transform.position, 60, 40)) {
		    			PDAMessages.trigger(PDAMessages.Messages.KelpCavePrompt);
		    		}
	    			if (!PDAMessages.isTriggered(PDAMessages.Messages.RedGrassCavePrompt)) {
		    			foreach (Vector3 vec in seacrownCaveEntrances) {
				    		if (MathUtil.isPointInCylinder(vec, ep.transform.position, 40, 60)) {
				    			PDAMessages.trigger(PDAMessages.Messages.RedGrassCavePrompt);
				    		}
			    		}
	    			}
	    		}
	    		
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
	    	if (Inventory.main == null)
	    		return f;
	    	Pickupable held = Inventory.main.GetHeld();
	    	if (held == null || held.gameObject == null)
	    		return f;
	    	EnergyMixin e = held.gameObject.GetComponent<EnergyMixin>();
	    	if (e == null)
	    		return f;
	    	//SNUtil.writeToChat("Get SG speed, was "+f+", has="+Mathf.Approximately(e.battery.capacity, SeaToSeaMod.t2Battery.capacity));
	    	if (e.battery != null && Mathf.Approximately(e.battery.capacity, SeaToSeaMod.t2Battery.capacity))
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
	    		mix.compatibleBatteries.Add(SeaToSeaMod.t2Battery.TechType);/*
	    		List<EnergyMixin.BatteryModels> arr = mix.batteryModels.ToList();
	    		GameObject go = SeaToSeaMod.t2Battery.GetGameObject();
	    		go.SetActive(false);
	    		arr.Add(new EnergyMixin.BatteryModels{model = go, techType = SeaToSeaMod.t2Battery.TechType});
	    		mix.batteryModels = arr.ToArray();*/
	    	}
	    }
	    
	    public static GameObject onSpawnBatteryForEnergyMixin(GameObject go) {
	    	SNUtil.writeToChat("Spawned a "+go);
	    	go.SetActive(false);
	    	return go;
	    }
	    
	    public static void collectTimeCapsule(TimeCapsule tc) {
	    	bool someBlocked = false;
			try
			{
				PDAEncyclopedia.AddTimeCapsule(tc.id, true);
				PlayerTimeCapsule.main.RegisterOpen(tc.instanceId);
				List<TimeCapsuleItem> items = TimeCapsuleContentProvider.GetItems(tc.id);
				if (items != null) {
					foreach (TimeCapsuleItem tci in items) {
						if (SeaToSeaMod.isTechGated(tci.techType) || SeaToSeaMod.isTechGated(tci.batteryType)) {
							someBlocked = true;
							continue;
						}
						Pickupable pickupable = tci.Spawn();
						if (pickupable != null) {
							Inventory.main.ForcePickup(pickupable);
						}
					}
				}
			}
			finally
			{
				UnityEngine.Object.Destroy(tc.gameObject);
			}
			if (someBlocked) {
				
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
	    
	    public static void onFarmedPlantGrowingSpawn(Plantable p, GameObject plant) {
	    	TechTag tt = p.gameObject.GetComponent<TechTag>();
	    	if (tt != null && tt.type == SeaToSeaMod.alkali.seed.TechType) {
	    		RenderUtil.swapToModdedTextures(plant.GetComponentInChildren<Renderer>(true), SeaToSeaMod.alkali);
	    		plant.gameObject.EnsureComponent<TechTag>().type = tt.type;
	    	}
	    	if (tt != null && tt.type == SeaToSeaMod.healFlower.seed.TechType) {
	    		RenderUtil.swapToModdedTextures(plant.GetComponentInChildren<Renderer>(true), SeaToSeaMod.healFlower);
	    		plant.gameObject.EnsureComponent<TechTag>().type = tt.type;
	    	}
	    }
	    
	    public static void onFarmedPlantGrowDone(GrowingPlant p, GameObject plant) {
	    	TechTag tt = p.gameObject.GetComponent<TechTag>();
	    	if (tt != null && tt.type == SeaToSeaMod.alkali.seed.TechType) {
	    		ObjectUtil.convertTemplateObject(plant, SeaToSeaMod.alkali);
	    	}
	    	if (tt != null && tt.type == SeaToSeaMod.healFlower.seed.TechType) {
	    		ObjectUtil.convertTemplateObject(plant, SeaToSeaMod.healFlower);
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
    
	    public static bool isSpawnableVoid(string biome) {
	    	return VoidGhostLeviathanSystem.instance.isSpawnableVoid(biome);
	    }
	    
	    public static GameObject getVoidLeviathan(VoidGhostLeviathansSpawner spawner, Vector3 pos) {
	    	return VoidGhostLeviathanSystem.instance.getVoidLeviathan(spawner, pos);
	    }
	    
	    public static void tickVoidLeviathan(GhostLeviatanVoid gv) {
	    	VoidGhostLeviathanSystem.instance.tickVoidLeviathan(gv);
	    }
	    
	    public static void pingSeamothSonar(SeaMoth sm) {
	    	VoidGhostLeviathanSystem.instance.tagSeamothSonar(sm);
	    }
	    
	    public static void pulseSeamothDefence(SeaMoth sm) {
	    	VoidGhostLeviathanSystem.instance.tagSeamothSonar(sm);
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
	    
	    public static void getBulkheadMouseoverText(BulkheadDoor bk) {
			if (bk.enabled && bk.state == BulkheadDoor.State.Zero) {
	    		Sealed s = bk.GetComponent<Sealed>();
	    		if (s != null && s.IsSealed()) {
					HandReticle.main.SetInteractText("SealedInstructions"); //is a locale key
					HandReticle.main.SetProgress(s.GetSealedPercentNormalized());
					HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
	    		}
	    		else {
					HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
					HandReticle.main.SetInteractText(bk.targetState ? "Close" : "Open");
	    		}
			}
	    }
	    
	    public static void onBulkheadClick(BulkheadDoor bk) {
			Base componentInParent = bk.GetComponentInParent<Base>();
			Sealed s = bk.GetComponent<Sealed>();
			if (s != null && s.IsSealed()) {
				
			}
			else if (componentInParent != null && !componentInParent.isReady) {
				bk.ToggleImmediately();
			}
			else if (bk.enabled && bk.state == BulkheadDoor.State.Zero) {
				if (GameOptions.GetVrAnimationMode()) {
					bk.ToggleImmediately();
					return;
				}
				bk.SequenceDone();
			}
	    }
	    
	    public static void onAuroraSpawn(CrashedShipExploder ex) {
	    	Sealed s = ex.gameObject.EnsureComponent<Sealed>();
	    	s._sealed = true;
	    	s.maxOpenedAmount = 150;
	    	s.openedEvent.AddHandler(ex.gameObject, new UWE.Event<Sealed>.HandleFunction(se => {
	    		se.openedAmount = 0;
	    		se._sealed = true;
	    		GameObject scrap = CraftData.GetPrefabForTechType(TechType.ScrapMetal);
	    		scrap = UnityEngine.Object.Instantiate(scrap);
	    		scrap.SetActive(false);
	    		Inventory.main.ForcePickup(scrap.GetComponent<Pickupable>());
	    		PDAMessages.trigger(PDAMessages.Messages.AuroraSalvage);
	    	}));
			GenericHandTarget ht = ex.gameObject.EnsureComponent<GenericHandTarget>();
			ht.onHandHover = new HandTargetEvent();
			ht.onHandHover.AddListener(hte => {
				HandReticle.main.SetInteractText("AuroraLaserCut"); //is a locale key
				HandReticle.main.SetProgress(s.GetSealedPercentNormalized());
				HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
			});
			Language.main.strings["AuroraLaserCut"] = "Use Laser Cutter to harvest metal salvage";
	    }
	}
}

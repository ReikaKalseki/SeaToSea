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
	    
	    private static readonly Vector3 deepDegasiTablet = new Vector3(-638.9F, -506.0F, -941.3F);
	    
	    private static readonly PositionedPrefab auroraStorageModule = new PositionedPrefab("d290b5da-7370-4fb8-81bc-656c6bde78f8", new Vector3(991.5F, 3.21F, -30.99F), Quaternion.Euler(14.44F, 353.7F, 341.6F));
	    private static readonly PositionedPrefab auroraCyclopsModule = new PositionedPrefab("049d2afa-ae76-4eef-855d-3466828654c4", new Vector3(872.5F, 2.69F, -0.66F), Quaternion.Euler(357.4F, 224.9F, 21.38F));
	    
	    private static readonly Dictionary<string, TechType> scannerInjections = new Dictionary<string, TechType>() {
	    	{"61ac1241-e990-4646-a618-bddb6960325b", TechType.SeaTreaderPoop},
	    	{"54701bfc-bb1a-4a84-8f79-ba4f76691bef", TechType.GhostLeviathan},
	    	{"35ee775a-d54c-4e63-a058-95306346d582", TechType.SeaTreader},
	    	{"ff43eacd-1a9e-4182-ab7b-aa43c16d1e53", TechType.SeaDragon},
	    };
	    
	    private static readonly HashSet<string> containmentDragonRepellents = new HashSet<string>() {
	    	"c5512e00-9959-4f57-98ae-9a9962976eaa",
	    	"542aaa41-26df-4dba-b2bc-3fa3aa84b777",
	    	"5bcaefae-2236-4082-9a44-716b0598d6ed",
	    	"20ad299d-ca52-48ef-ac29-c5ec5479e070",
	    	"430b36ae-94f3-4289-91ac-25475ad3bf74"
	    };
	    
	    private static Oxygen playerBaseO2;
	    
	    static C2CHooks() {
	    	DIHooks.onDayNightTickEvent += onTick;
	    	DIHooks.onWorldLoadedEvent += onWorldLoaded;
	    	DIHooks.onPlayerTickEvent += tickPlayer;
	    	DIHooks.onDamageEvent += recalculateDamage;
	    	DIHooks.onItemPickedUpEvent += onItemPickedUp;
	    	DIHooks.onSkyApplierSpawnEvent += onSkyApplierSpawn;
	    }
    
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
	    	Inventory.main.equipment.onEquip += onEquipmentAdded;
	    	Inventory.main.equipment.onUnequip += onEquipmentRemoved;
	        
	    	BrokenTablet.updateLocale();
	    	OutdoorPot.updateLocale();
	    	/*
	    	SNUtil.log(string.Join(", ", Story.StoryGoalManager.main.locationGoalTracker.goals.Select<Story.StoryGoal, string>(g => g.key+" of "+g.goalType).ToArray()));
	    	SNUtil.log(string.Join(", ", Story.StoryGoalManager.main.compoundGoalTracker.goals.Select<Story.StoryGoal, string>(g => g.key+" of "+g.goalType).ToArray()));
	    	SNUtil.log(string.Join(", ", Story.StoryGoalManager.main.biomeGoalTracker.goals.Select<Story.StoryGoal, string>(g => g.key+" of "+g.goalType).ToArray()));
			SNUtil.log(string.Join(", ", Story.StoryGoalManager.main.onGoalUnlockTracker.goalUnlocks.Values.Select<Story.OnGoalUnlock, string>(g => g.goal).ToArray()));
	    	*/
	    	VoidSpikesBiome.instance.onWorldStart();
        
	    	moveToExploitable("SeaCrown");
	    	moveToExploitable("SpottedLeavesPlant");
	    	moveToExploitable("OrangeMushroom");
	    	moveToExploitable("SnakeMushroom");
	    	moveToExploitable("PurpleVasePlant");
	    
	    	foreach (string k in new List<String>(Language.main.strings.Keys)) {
	    		string k2 = k.ToLowerInvariant();
	    		if (k2.Contains("tooltip") || k2.Contains("desc") || k2.Contains("ency"))
	    			continue;
	    		string s = Language.main.strings[k];
	    		if (s.ToLowerInvariant().Contains("creepvine"))
	    			continue;
	    		s = s.Replace(" seed", " Sample");
	    		s = s.Replace(" spore", " Sample");
	    		s = s.Replace(" Seed", " Sample");
	    		s = s.Replace(" Spore", " Sample");
	    		//SNUtil.log("Updating seed naming for "+k);
	    		Language.main.strings[k] = s;
		    }
	    	
	    	Language.main.strings["BulkheadInoperable"] = SeaToSeaMod.miscLocale.getEntry("BulkheadInoperable").desc;
			Language.main.strings["DockToChangeVehicleUpgrades"] = SeaToSeaMod.miscLocale.getEntry("DockToChangeVehicleUpgrades").desc;
	    	Language.main.strings["Tooltip_"+TechType.MercuryOre.AsString()] = SeaToSeaMod.miscLocale.getEntry("MercuryDesc").desc;
			
	    	/* does not contain the mouse bit, and it is handled automatically anyway
	    	string ttip = Language.main.strings["Tooltip_"+SeaToSeaMod.bandage.TechType.AsString()];
	    	string hkit = Language.main.strings["Tooltip_"+TechType.FirstAidKit.AsString()];
			Language.main.strings["Tooltip_"+SeaToSeaMod.bandage.TechType.AsString()] = ttip+"\n\n"+hkit;*/
	    	
	    }
	    
	    private static void moveToExploitable(string key) {
	    	PDAEncyclopedia.EntryData data = PDAEncyclopedia.mapping[key];/*
	    	TreeNode root = PDAEncyclopedia.tree;
	    	TreeNode node = root;
	    	foreach (string s in data.path.Split('/')) {
	    		node = node[s];
	    	}
	    	if (node == null) {
	    		SNUtil.log("Found no ency node for "+key+" in "+data.path);
	    		return;
	    	}*/
	    	//node.parent.RemoveNode(node);
	    	//root[3][1][0].AddNode(node);
	    	data.path = data.path.Replace("Sea", "Exploitable").Replace("Land", "Exploitable");
	    	data.nodes = PDAEncyclopedia.ParsePath(data.path);
	    }
	    
	    public static void tickPlayer(Player ep) {
	    	if (Time.timeScale <= 0)
	    		return;
	    	//SNUtil.writeToChat(ep.GetBiomeString());
	    	
	    	if (playerBaseO2 == null) {
	    		foreach (Oxygen o in Player.main.oxygenMgr.sources) {
	    			if (o.isPlayer) {
	    				playerBaseO2 = o;
	    				break;
	    			}
	    		}
	    	}
	    	
	    	if (LiquidBreathingSystem.instance.hasTankButNoMask()) {
	    		Oxygen ox = Inventory.main.equipment.GetItemInSlot("Tank").item.gameObject.GetComponent<Oxygen>();
	    		ep.oxygenMgr.UnregisterSource(ox);
	    		ep.oxygenMgr.UnregisterSource(playerBaseO2);
	    	}	    	
	    	else if (LiquidBreathingSystem.instance.hasLiquidBreathing()) {
	    		Oxygen ox = Inventory.main.equipment.GetItemInSlot("Tank").item.gameObject.GetComponent<Oxygen>();
	    		if (LiquidBreathingSystem.instance.isLiquidBreathingActive(ep)) {
	    			ep.oxygenMgr.UnregisterSource(playerBaseO2);
	    			ep.oxygenMgr.RegisterSource(ox);
	    		}
	    		else {
	    			ep.oxygenMgr.UnregisterSource(ox);
	    			ep.oxygenMgr.RegisterSource(playerBaseO2);
	    			float add = Mathf.Min(ep.oxygenMgr.oxygenUnitsPerSecondSurface, ox.oxygenCapacity-ox.oxygenAvailable)*Time.deltaTime;
	    			if (add > 0.01) {
	    				if (LiquidBreathingSystem.instance.tryFillPlayerO2Bar(ep, ref add))
	    					ox.AddOxygen(add);
	    			}
	    		}
	    	}
	    	else {
	    		ep.oxygenMgr.RegisterSource(playerBaseO2);
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
	    	if (item.item.GetTechType() == SeaToSeaMod.liquidTank.TechType)
	    		LiquidBreathingSystem.instance.onEquip();
	    }
	    
	    public static void onEquipmentRemoved(string slot, InventoryItem item) {
	    	if (item.item.GetTechType() == SeaToSeaMod.liquidTank.TechType)
	    		LiquidBreathingSystem.instance.onUnequip();
	    }
	    
	    public static void tickO2Bar(uGUI_OxygenBar gui) {
	    	LiquidBreathingSystem.instance.updateOxygenGUI(gui);
	    }
	    
	    public static float getO2RedPulseTime(float orig) {
	    	return LiquidBreathingSystem.instance.isO2BarFlashingRed() ? 6 : orig;
	    }
	    
	    public static bool canPlayerBreathe(bool orig, Player p) {
	    	//SNUtil.writeToChat(orig+": "+p.IsUnderwater()+" > "+Inventory.main.equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType));
	    	if (!LiquidBreathingSystem.instance.isO2BarAbleToFill(p))
	    		return false;
	    	return orig;
	    }
	    
	    public static float addO2ToPlayer(OxygenManager mgr, float f) {
	   		if (!LiquidBreathingSystem.instance.isO2BarAbleToFill(Player.main))
	   			f = 0;
	   		return f;
	    }
	    
	    public static void addOxygenAtSurfaceMaybe(OxygenManager mgr, float time) {
	    	if (LiquidBreathingSystem.instance.isO2BarAbleToFill(Player.main)) {
	    		//SNUtil.writeToChat("Add surface O2");
	    		mgr.AddOxygenAtSurface(time);
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
	    	//SNUtil.writeToChat("Get SG speed, was "+f+", has="+Mathf.Approximately(e.battery.capacity, SeaToSeaMod.t2Battery.capacity));
			if (isHeldToolAzuritePowered()) {
	    		float bonus = 0.75F; //was 0.55 then 0.95
	    		float depth = Player.main.GetDepth();
	    		float depthFactor = depth <= 50 ? 1 : 1-((depth-50)/350F);
	    		if (depthFactor > 0) {
	    			f += bonus*depthFactor;
	    		}
	    	}
	    	return f;
	    }
	    
	    public static float getScannerSpeed(float f) { //f is a divisor, scanTime
			if (isHeldToolAzuritePowered()) {
	    		f *= 0.5F; //double speed
	    	}
	    	return f;
	    }
	    /* DO NOT USE - RISKS VOIDING
	    public static float getBuilderSpeed(float f) { //f is a divisor, item count
	    	if (isHeldToolAzuritePowered()) {
	    		f *= 0.667F; //1.5x speed
	    	}
	    	return f;
	    }*/
	    
	    public static float getLaserCutterSpeed(LaserCutter lc) { //25 by default
	    	float amt = lc.healthPerWeld;
	    	EnergyMixin e = lc.gameObject.GetComponent<EnergyMixin>();
	    	if (e == null)
	    		return amt;
	    	if (e.battery != null && Mathf.Approximately(e.battery.capacity, SeaToSeaMod.t2Battery.capacity)) {
	    		amt *= 1.5F;
	    	}
	    	return amt;
	    }
	    
	    public static float getRepairSpeed(Welder lc) { //10 by default
	    	float amt = lc.healthPerWeld;
	    	EnergyMixin e = lc.gameObject.GetComponent<EnergyMixin>();
	    	if (e == null)
	    		return amt;
	    	if (e.battery != null && Mathf.Approximately(e.battery.capacity, SeaToSeaMod.t2Battery.capacity)) {
	    		amt *= 2F;
	    	}
	    	return amt;
	    }
	    
	    public static bool isHeldToolAzuritePowered() {
	    	if (Inventory.main == null)
	    		return false;
	    	Pickupable held = Inventory.main.GetHeld();
	    	if (held == null || held.gameObject == null)
	    		return false;
	    	EnergyMixin e = held.gameObject.GetComponent<EnergyMixin>();
	    	if (e == null)
	    		return false;
	    	return e.battery != null && Mathf.Approximately(e.battery.capacity, SeaToSeaMod.t2Battery.capacity);
	    }
	    
	    public static void onThingInO2Area(OxygenArea a, Collider obj) {
	    	if (obj.gameObject.FindAncestor<Player>() == Utils.GetLocalPlayerComp()) {
		    	float o2ToAdd = Math.Min(a.oxygenPerSecond*Time.deltaTime, Player.main.GetOxygenCapacity()-Player.main.GetOxygenAvailable());
		    	if (o2ToAdd > 0)
		    		LiquidBreathingSystem.instance.tryFillPlayerO2Bar(Player.main, ref o2ToAdd, true);
		    	if (LiquidBreathingSystem.instance.hasLiquidBreathing()) {
		    		LiquidBreathingSystem.instance.checkLiquidBreathingSupport(a);
		    	}
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
   
		public static void recalculateDamage(DIHooks.DamageToDeal dmg) {
	   		//if (type == DamageType.Acid && dealer == null && target.GetComponentInParent<SeaMoth>() != null)
	   		//	return 0;
	   		Player p = dmg.target.GetComponentInParent<Player>();
	   		if (p != null) {
	   			bool seal = Inventory.main.equipment.GetCount(SeaToSeaMod.sealSuit.TechType) != 0;
	   			bool reinf = Inventory.main.equipment.GetCount(TechType.ReinforcedDiveSuit) != 0;
	   			if (dmg.type == DamageType.Poison || dmg.type == DamageType.Acid || dmg.type == DamageType.Electrical) {
	   				dmg.amount *= seal ? 0.2F : 0.4F;
	   				dmg.amount -= seal ? 10 : 7.5F;
	   				if (dmg.amount < 0)
	   					dmg.amount = 0;
	   			}
	   		}
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
				if (Inventory.main.equipment.GetCount(SeaToSeaMod.sealSuit.TechType) == 0 && Inventory.main.equipment.GetCount(TechType.ReinforcedDiveSuit) == 0) {
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
	    	//SNUtil.log("Checking targeting skip of "+id);
	    	if (VoidSpike.isSpike(id.ClassId) && VoidSpikesBiome.instance.isInBiome(obj.position)) {
	    		//SNUtil.log("Is void spike");
	    		return true;
	    	}
	    	else {
	    		return orig;
	    	}
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
	    	PrefabIdentifier pi = p.gameObject.GetComponent<PrefabIdentifier>();
	    	if (pi && pi.ClassId == VanillaResources.LARGE_SULFUR.prefab) {
	    		p.overrideTechType = TechType.Sulphur;
	    		p.techType = TechType.Sulphur;
	    	}
	    }
	    
	    public static void doEnviroVehicleDamage(CrushDamage dmg) {
	    	EnvironmentalDamageSystem.instance.tickCyclopsDamage(dmg);
	    }
	    
	    public static float getWaterTemperature(float ret, WaterTemperatureSimulation sim, Vector3 pos) {
	    	string biome = EnvironmentalDamageSystem.instance.getBiome(pos);
	    	float poison = EnvironmentalDamageSystem.instance.getLRPoison(biome);
	    	if (poison > 0)
	    		ret = Mathf.Max(4, ret-poison*1.75F); //make LR cold, down to 4C (max water density point)
	    	if (biome.ToLowerInvariant().Contains("void") && pos.y <= -50)
	    		ret = Mathf.Max(4, ret+(pos.y+50)/20F); //drop 1C per 20m below 50m, down to 4C around 550m
	    	double dist = VoidSpikesBiome.instance.getDistanceToBiome(pos);
	    	if (dist <= 300)
	    		ret += (float)((300-dist)/30); //add up to 10C as approach
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
	    
	    public static void onSkyApplierSpawn(SkyApplier pk) {
	    	GameObject go = pk.gameObject;
	    	PrefabIdentifier pi = go.GetComponentInParent<PrefabIdentifier>();
			if (pi && scannerInjections.ContainsKey(pi.ClassId)) {
				TechType tt = scannerInjections[pi.ClassId];
				ObjectUtil.makeMapRoomScannable(go, tt, true);
				if (tt == TechType.SeaTreader) {
					//go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
				}
				else if (tt == TechType.SeaTreaderPoop) {
	    			if (Vector3.Distance(go.transform.position, Player.main.transform.position) <= 40 && go.transform.position.y < -200) {
						PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.TreaderPooPrompt).key);
		    		}
				}
	    	}
	    	if (pi && pi.ClassId == "" && Vector3.Distance(deepDegasiTablet, go.transform.position) <= 0.2) {
	    		GameObject go2 = ObjectUtil.createWorldObject(SeaToSeaMod.brokenOrangeTablet.ClassID);
	    		go2.transform.position = go.transform.position;
	    		go2.transform.rotation = go.transform.rotation;
	    		UnityEngine.Object.Destroy(go);
	    		return;
	    	}
	    	if (pi && containmentDragonRepellents.Contains(pi.ClassId)) {
	    		go.EnsureComponent<ContainmentFacilityDragonRepellent>();
	    		return;
	    	}
	    	else if (pi && PrefabData.getPrefab(pi.ClassId) != null && PrefabData.getPrefab(pi.ClassId).Contains("Coral_reef_jeweled_disk")) {
	    		ObjectUtil.makeMapRoomScannable(go, TechType.JeweledDiskPiece);
	    	}/*
	    	else if (pi && pi.ClassId == auroraStorageModule.prefabName && Vector3.Distance(auroraStorageModule.position, go.transform.position) <= 0.2) {
	    		go.transform.position = auroraCyclopsModule.position;
	    		go.transform.rotation = auroraCyclopsModule.rotation;
	    	}
	    	else if (pi && pi.ClassId == auroraCyclopsModule.prefabName && Vector3.Distance(auroraCyclopsModule.position, go.transform.position) <= 0.2) {
	    		go.transform.position = auroraStorageModule.position;
	    		go.transform.rotation = auroraStorageModule.rotation;
	    	}*/
	    	if (ObjectUtil.isPDA(go)) {
				ObjectUtil.makeMapRoomScannable(go, TechType.PDA);
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
	    			if (s.maxOpenedAmount < 0) {
	    				HandReticle.main.SetInteractText("BulkheadInoperable");
						HandReticle.main.SetIcon(HandReticle.IconType.None, 1f);
	    			}
	    			else {
						HandReticle.main.SetInteractText("SealedInstructions"); //is a locale key
						HandReticle.main.SetProgress(s.GetSealedPercentNormalized());
						HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
	    			}
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
	    
	    public static void generateScannerRoomResourceList(uGUI_MapRoomScanner gui) {
	    	gui.availableTechTypes.RemoveWhere(item => !playerCanScanFor(item));
	    	gui.RebuildResourceList();
	    }
	    
	    private static bool playerCanScanFor(TechType tt) {
	    	switch(tt) {
	    		case TechType.ReaperLeviathan:
	    		case TechType.SeaDragon:
	    		case TechType.GhostLeviathanJuvenile:
	    		case TechType.GhostLeviathan:
	    		case TechType.SeaTreader:
	    		case TechType.SeaEmperorLeviathan:
	    		case TechType.Reefback:
	    			
	    		case TechType.LimestoneChunk:
	    		case TechType.SandstoneChunk:
	    		case TechType.BasaltChunk:
	    			
	    		case TechType.PrecursorIonCrystal:
	    		case TechType.PrecursorKey_Purple:
	    		case TechType.PrecursorKey_Blue:
	    		case TechType.PrecursorKey_Red:
	    		case TechType.PrecursorKey_White:
	    		case TechType.PrecursorKey_Orange:
	    			return PDAScanner.complete.Contains(tt);
	    		case TechType.StalkerTooth:
	    			return PDAScanner.complete.Contains(TechType.Stalker);
	    		case TechType.GenericEgg:
	    		case TechType.StalkerEgg:
	    		case TechType.BonesharkEgg:
	    		case TechType.CrabsnakeEgg:
	    		case TechType.CrashEgg:
	    		case TechType.CrabsquidEgg:
	    		case TechType.CutefishEgg:
	    		case TechType.JellyrayEgg:
	    		case TechType.RabbitrayEgg:
	    		case TechType.SandsharkEgg:
	    		case TechType.ShockerEgg:
	    		case TechType.ReefbackEgg:
	    		case TechType.MesmerEgg:
	    		case TechType.LavaLizardEgg:
	    		case TechType.JumperEgg:
	    		case TechType.SpadefishEgg:
	    			return PDAScanner.complete.Contains(TechType.GenericEgg);/*
	    		case TechType.DrillableTitanium: //FIXME A: MAKE OWN MOD B: MAY NOT WORK SINCE LISTED UNDER "PLAIN" RESOURCE
	    		case TechType.DrillableSulphur:
	    		case TechType.DrillableUranium:
	    		case TechType.DrillableQuartz:
	    		case TechType.DrillableKyanite:
	    		case TechType.DrillableLithium:
	    		case TechType.DrillableMagnetite:
	    		case TechType.DrillableSalt:
	    		case TechType.DrillableAluminiumOxide:
	    		case TechType.DrillableCopper:
	    		case TechType.DrillableLead:
	    		case TechType.DrillableSilver:
	    		case TechType.DrillableGold:
	    		case TechType.DrillableDiamond:
	    		case TechType.DrillableNickel:
	    		case TechType.DrillableMercury:
	    			return KnownTech.analyzedTech.Contains(TechType.ExosuitDrillArmModule);*/
	    		default:
	    			return true;
	    	}
	    }
	    /*
	    public static void registerResourceTracker(ResourceTracker rt) {
	    	if (rt.techType != TechType.None && isObjectVisibleToScannerRoom(rt)) {
				Dictionary<string, ResourceTracker.ResourceInfo> orAddNew = ResourceTracker.resources.GetOrAddNew(rt.techType);
				string key = rt.uniqueId;
				ResourceTracker.ResourceInfo resourceInfo;
				if (!orAddNew.TryGetValue(key, out resourceInfo)) {
					resourceInfo = new ResourceTracker.ResourceInfo();
					resourceInfo.uniqueId = key;
					resourceInfo.position = rt.transform.position;
					resourceInfo.techType = rt.techType;
					orAddNew.Add(key, resourceInfo);
					if (ResourceTracker.onResourceDiscovered != null) {
						ResourceTracker.onResourceDiscovered.Invoke(resourceInfo);
						return;
					}
				}
				else {
					resourceInfo.position = rt.transform.position;
				}
			}
	    }
	    */
	    public static bool isObjectVisibleToScannerRoom(ResourceTracker rt) { //FIXME MAKE OWN MOD
	   		//SNUtil.log("Checking scanner visibility of "+rt.gameObject+" @ "+rt.gameObject.transform.position+": "+rt.gameObject.GetComponentInChildren<Drillable>());
	    	if (rt.gameObject.GetComponentInChildren<Drillable>() && !KnownTech.knownTech.Contains(TechType.ExosuitDrillArmModule))
	    		return false;
	    	return true;
	    }
	   
	   	public static void tickACU(WaterPark acu) {
	   		ACUCallbackSystem.instance.tick(acu);
	   	}
	   
		public static string getO2Tooltip(Oxygen ox) {
	   		if (ox.GetComponent<Pickupable>().GetTechType() == SeaToSeaMod.liquidTank.TechType) {
	   			return ox.GetSecondsLeft()+"s fluid stored in supply tank";
	   		}
	   		return LanguageCache.GetOxygenText(ox.GetSecondsLeft());
		}
	   
		public static string getBatteryTooltip(Battery ox) {
	   		if (ox.GetComponent<Pickupable>().GetTechType() == SeaToSeaMod.liquidTank.TechType)
	   			return Mathf.RoundToInt(ox.charge)+"s fluid stored in primary tank";
	   		return Language.main.GetFormat<float, int, float>("BatteryCharge", ox.charge/ox.capacity, Mathf.RoundToInt(ox.charge), ox.capacity);
		}
	   
	   public static void onClickedVehicleUpgrades(VehicleUpgradeConsoleInput v) {
			if (v.docked)
				v.OpenPDA();
	   }
	   
		public static void onHoverVehicleUpgrades(VehicleUpgradeConsoleInput v) {
			HandReticle main = HandReticle.main;
		   	if (!v.docked) {
				main.SetInteractText("DockToChangeVehicleUpgrades"); //locale key
				main.SetIcon(HandReticle.IconType.HandDeny, 1f);
		   	}
			else if (v.equipment != null) {
				main.SetInteractText(v.interactText);
				main.SetIcon(HandReticle.IconType.Hand, 1f);
			}
		}
	   
	   	public static bool isInsideForHatch(UseableDiveHatch hatch) {
	   		SeabaseReconstruction.WorldgenBaseWaterparkHatch wb = hatch.gameObject.GetComponent<SeabaseReconstruction.WorldgenBaseWaterparkHatch>();
	   		if (wb)
	   			return wb.isPlayerInside();
	   		return Player.main.IsInsideWalkable() && Player.main.currentWaterPark == null;
	   	}
	   
	   	public static bool canAddItemToACU(Pickupable item) {
			if (!item)
		   		return false;
			TechType tt = item.GetTechType();
			if (tt == TechType.ScrapMetal || tt == TechType.Titanium || tt == TechType.Silver)
				return true;
			GameObject go = item.gameObject;
			if (go.GetComponent<Creature>() == null && go.GetComponent<CreatureEgg>() == null)
				return false;
			LiveMixin lv = go.GetComponent<LiveMixin>();
			return !lv || lv.IsAlive();
	   	}
	   
	   public static void onChunkGenGrass(IVoxelandChunk2 chunk) {
	   	foreach (Renderer r in chunk.grassRenders) {
	   		ACUCallbackSystem.instance.cacheGrassMaterial(r.materials[0]);
	   	}
	   }
	}
	
	class ContainmentFacilityDragonRepellent : MonoBehaviour {
		
		void Update() {
			float r = 80;
			if (Player.main.transform.position.y <= 1350 && Vector3.Distance(transform.position, Player.main.transform.position) <= 100) {
				RaycastHit[] hit = Physics.SphereCastAll(gameObject.transform.position, r, new Vector3(1, 1, 1), r);
				foreach (RaycastHit rh in hit) {
					if (rh.transform != null && rh.transform.gameObject) {
						SeaDragon c = rh.transform.gameObject.GetComponent<SeaDragon>();
						if (c) {
							Vector3 vec = transform.position+((c.transform.position-transform.position).normalized*120);
							c.GetComponent<SwimBehaviour>().SwimTo(vec, 20);
						}
					}
				}
			}
		}
		
	}
}

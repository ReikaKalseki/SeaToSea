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

using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public static class C2CHooks {
	    
	    private static readonly Vector3 deepDegasiTablet = new Vector3(-638.9F, -506.0F, -941.3F);
	    private static readonly Vector3 crashMesa = new Vector3(623.8F, -250.0F, -1105.2F);
	    private static readonly Vector3 mountainBaseGeoCenter = new Vector3(953, -344, 1453);
	    private static readonly Vector3 bkelpBaseGeoCenter = new Vector3(-1311.6F, -670.6F, -412.7F);
	    private static readonly Vector3 lrpowerSealSetpieceCenter = new Vector3(-713.45F, -766.37F, -262.74F);
	    
	    private static readonly PositionedPrefab auroraStorageModule = new PositionedPrefab("d290b5da-7370-4fb8-81bc-656c6bde78f8", new Vector3(991.5F, 3.21F, -30.99F), Quaternion.Euler(14.44F, 353.7F, 341.6F));
	    private static readonly PositionedPrefab auroraCyclopsModule = new PositionedPrefab("049d2afa-ae76-4eef-855d-3466828654c4", new Vector3(872.5F, 2.69F, -0.66F), Quaternion.Euler(357.4F, 224.9F, 21.38F));
	    
	    private static Oxygen playerBaseO2;
	    
	    static C2CHooks() {
	    	DIHooks.onWorldLoadedEvent += onWorldLoaded;
	    	DIHooks.onDamageEvent += recalculateDamage;
	    	DIHooks.onItemPickedUpEvent += onItemPickedUp;
	    	DIHooks.onSkyApplierSpawnEvent += onSkyApplierSpawn;
	    	
	    	DIHooks.getBiomeEvent += getBiomeAt;
	    	DIHooks.getTemperatureEvent += getWaterTemperature;
	    	
	    	DIHooks.onPlayerTickEvent += tickPlayer;
	    	DIHooks.onSeamothTickEvent += tickSeamoth;
	    	
	    	DIHooks.onSeamothModulesChangedEvent += updateSeamothModules;
	    	DIHooks.onSeamothModuleUsedEvent += useSeamothModule;
	    	
	    	DIHooks.onSeamothSonarUsedEvent += pingSeamothSonar;
	    	
	    	BaseSonarPinger.onBaseSonarPingedEvent += onBaseSonarPinged;
	    }
	    
	    public static void onWorldLoaded() {	    	
	    	Inventory.main.equipment.onEquip += onEquipmentAdded;
	    	Inventory.main.equipment.onUnequip += onEquipmentRemoved;
	        
	    	BrokenTablet.updateLocale();
	    	/*
	    	SNUtil.log(string.Join(", ", Story.StoryGoalManager.main.locationGoalTracker.goals.Select<Story.StoryGoal, string>(g => g.key+" of "+g.goalType).ToArray()));
	    	SNUtil.log(string.Join(", ", Story.StoryGoalManager.main.compoundGoalTracker.goals.Select<Story.StoryGoal, string>(g => g.key+" of "+g.goalType).ToArray()));
	    	SNUtil.log(string.Join(", ", Story.StoryGoalManager.main.biomeGoalTracker.goals.Select<Story.StoryGoal, string>(g => g.key+" of "+g.goalType).ToArray()));
			SNUtil.log(string.Join(", ", Story.StoryGoalManager.main.onGoalUnlockTracker.goalUnlocks.Values.Select<Story.OnGoalUnlock, string>(g => g.goal).ToArray()));
	    	*/
	    	VoidSpikesBiome.instance.onWorldStart();
	    	UnderwaterIslandsFloorBiome.instance.onWorldStart();
        
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
	    	
	    	float dist = Vector3.Distance(ep.transform.position, crashMesa);
	    	if (dist < 5 || (dist <= 200 && UnityEngine.Random.Range(0F, 1F) <= 0.04F*Time.timeScale*(dist <= 75 ? 2.5F : 1))) {
	    		IEcoTarget tgt = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Leviathan, crashMesa, eco => eco.GetGameObject().GetComponent<ReaperLeviathan>(), 6);
	    		if (tgt != null && Vector3.Distance(tgt.GetPosition(), crashMesa) >= Mathf.Max(dist, 15)) {
	    			GameObject go = tgt.GetGameObject();
	    			Vehicle v = ep.GetVehicle();
	    			GameObject hit = v ? v.gameObject : ep.gameObject;
	    			Vector3 pos = dist <= 40 ? hit.transform.position : MathUtil.getRandomVectorAround(crashMesa, 45).setY(crashMesa.y);
	    			if (Vector3.Distance(go.transform.position, pos) >= 30)
	    				go.GetComponent<SwimBehaviour>().SwimTo(pos, 20);
	    			ReaperLeviathan r = go.GetComponent<ReaperLeviathan>();
	    			r.Aggression.Add(0.5F);
	    			r.leashPosition = pos;
	    			go.GetComponent<ReaperMeleeAttack>().lastTarget.SetTarget(hit);
	    			foreach (AggressiveWhenSeeTarget a in go.GetComponents<AggressiveWhenSeeTarget>())
	    				a.lastTarget.SetTarget(hit);
	    		}
	    	}
	    	
	    	if (ep.currentSub == null && UnityEngine.Random.Range(0, (int)(10/Time.timeScale)) == 0) {
	    		VoidSpikesBiome.instance.tickPlayer(ep);
	    		UnderwaterIslandsFloorBiome.instance.tickPlayer(ep);
	    		
	    		if (ep.GetVehicle() == null) {
	    			float ventDist = -1;
					IEcoTarget tgt = EcoRegionManager.main.FindNearestTarget(EcoTargetType.HeatArea, ep.transform.position, null, 3);
					if (tgt != null)
						ventDist = Vector3.Distance(tgt.GetPosition(), ep.transform.position);
					if (ventDist >= 0 && ventDist <= 25) {
						float f = Math.Min(1, (40-ventDist)/32F);
			    		foreach (InventoryItem item in Inventory.main.container) {
			    			if (item != null) {
			    				Battery b = item.item.gameObject.GetComponentInChildren<Battery>();
			    				if (b != null && Mathf.Approximately(b.capacity, C2CItems.t2Battery.capacity)) {
			    					b.charge = Math.Min(b.charge+0.5F*f, b.capacity);
			    					continue;
			    				}
			    				EnergyMixin e = item.item.gameObject.GetComponentInChildren<EnergyMixin>();
			    				if (e != null && e.battery != null && Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity)) {
			    					//SNUtil.writeToChat("Charging "+item.item+" by factor "+f+", d="+ventDist);
			    					e.AddEnergy(0.5F*f);
			    				}
			    			}
			    		}
					}
	    		}
	    	}
	    }
	    
	    public static void tickSeamoth(SeaMoth sm) {
	    	
	    }
	    
	    public static void onEquipmentAdded(string slot, InventoryItem item) {
	    	if (item.item.GetTechType() == C2CItems.liquidTank.TechType)
	    		LiquidBreathingSystem.instance.onEquip();
	    }
	    
	    public static void onEquipmentRemoved(string slot, InventoryItem item) {
	    	if (item.item.GetTechType() == C2CItems.liquidTank.TechType)
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
	    
	    public static void getBiomeAt(DIHooks.BiomeCheck b) {
	    	if (VoidSpikesBiome.instance.isInBiome(b.position)) {
	    		b.setValue(VoidSpikesBiome.biomeName);
	    		b.lockValue();
	    	}
	    	else if (UnderwaterIslandsFloorBiome.instance.isInBiome(b.originalValue, b.position)) {
	    		b.setValue(UnderwaterIslandsFloorBiome.biomeName);
	    		b.lockValue();
	    	}
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
	    	//SNUtil.writeToChat("Get SG speed, was "+f+", has="+Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity));
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
	    	if (e.battery != null && Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity)) {
	    		amt *= 1.5F;
	    	}
	    	return amt;
	    }
	    
	    public static float getRepairSpeed(Welder lc) { //10 by default
	    	float amt = lc.healthPerWeld;
	    	EnergyMixin e = lc.gameObject.GetComponent<EnergyMixin>();
	    	if (e == null)
	    		return amt;
	    	if (e.battery != null && Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity)) {
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
	    	return e.battery != null && Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity);
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
	    			mix.defaultBattery = C2CItems.t2Battery.TechType;
	    			break;
	    	}
	    }
	    
	    public static void addT2BatteryAllowance(EnergyMixin mix) {
	    	if (mix.compatibleBatteries.Contains(TechType.Battery) && !mix.compatibleBatteries.Contains(C2CItems.t2Battery.TechType)) {
	    		mix.compatibleBatteries.Add(C2CItems.t2Battery.TechType);/*
	    		List<EnergyMixin.BatteryModels> arr = mix.batteryModels.ToList();
	    		GameObject go = C2CItems.t2Battery.GetGameObject();
	    		go.SetActive(false);
	    		arr.Add(new EnergyMixin.BatteryModels{model = go, techType = C2CItems.t2Battery.TechType});
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
						if (C2CProgression.instance.isTechGated(tci.techType) || C2CProgression.instance.isTechGated(tci.batteryType)) {
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
	   			if (dmg.type == DamageType.Heat && Vector3.Distance(p.transform.position, mountainBaseGeoCenter) <= 20) {
	   				dmg.setValue(0);
	   				return;
	   			}
	   			InventoryItem suit = Inventory.main.equipment.GetItemInSlot("Body");
	   			bool seal = suit != null && suit.item.GetTechType() == C2CItems.sealSuit.TechType;
	   			bool reinf = suit != null && suit.item.GetTechType() == TechType.ReinforcedDiveSuit;
	   			if (seal || reinf) {
		   			if (dmg.type == DamageType.Poison || dmg.type == DamageType.Acid || dmg.type == DamageType.Electrical) {
	   					dmg.setValue(dmg.getAmount() * (seal ? 0.2F : 0.4F));
	   					dmg.setValue(dmg.getAmount() - (seal ? 10 : 7.5F));
		   			}
	   			}
	   		}
	   		//SubRoot sub = dmg.target.FindAncestor<SubRoot>();
	   		//if (sub && sub.isCyclops)
	   		//	SNUtil.writeToChat("Cyclops ["+dmg.target.GetFullHierarchyPath()+"] took "+dmg.amount+" of "+dmg.type+" from '"+dmg.dealer+"'");
	   		if (dmg.type == DamageType.Normal || dmg.type == DamageType.Drill || dmg.type == DamageType.Puncture || dmg.type == DamageType.Electrical) {
	   			DeepStalkerTag s = dmg.target.GetComponent<DeepStalkerTag>();
	   			if (s) {
	   				if (dmg.type == DamageType.Electrical)
	   					s.onHitWithElectricDefense();
	   				dmg.setValue(dmg.getAmount() * 0.5F); //50% resistance to "factorio physical" damage, plus electric to avoid PD killing them
	   			}
	   		}
	   		if (dmg.type == DamageType.Electrical) {
	   			VoidSpikeLeviathan.VoidSpikeLeviathanAI s = dmg.target.GetComponent<VoidSpikeLeviathan.VoidSpikeLeviathanAI>();
	   			if (s) {
	   				dmg.setValue(0);
	   				dmg.lockValue();
	   			}
	   			if (!p && Vector3.Distance(dmg.target.transform.position, bkelpBaseGeoCenter) <= 60 && !dmg.target.FindAncestor<Vehicle>()) {
	   				dmg.setValue(0);
	   			}
	   		}
		}
	   
	   	public static float getVehicleRechargeAmount(Vehicle v) {
	   		float baseline = 0.0025F;
	   		SubRoot b = v.GetComponentInParent<SubRoot>();
	   		if (b && b.isBase && b.currPowerRating > 0) {
	   			baseline *= 4;
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
	    	TechType tt = p.GetTechType();
	    	if (tt == CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType) {
				if (Inventory.main.equipment.GetCount(C2CItems.sealSuit.TechType) == 0 && Inventory.main.equipment.GetCount(TechType.ReinforcedDiveSuit) == 0) {
	    			LiveMixin lv = Player.main.gameObject.GetComponentInParent<LiveMixin>();
					lv.TakeDamage(lv.maxHealth/4F, Player.main.gameObject.transform.position, DamageType.Electrical, Player.main.gameObject);
				}
	    	}
	    	else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType) {
				RaycastHit[] hit = Physics.SphereCastAll(p.transform.position, 60, new Vector3(1, 1, 1), 60);
				foreach (RaycastHit rh in hit) {
					if (rh.transform != null && rh.transform.gameObject) {
						DeepStalkerTag c = rh.transform.gameObject.GetComponent<DeepStalkerTag>();
						if (c && !c.currentlyHasPlatinum() && !c.gameObject.GetComponent<WaterParkCreature>()) {
							float chance = Mathf.Clamp01(1F-Vector3.Distance(rh.transform.position, p.transform.position)/90F);
							if (UnityEngine.Random.Range(0F, 1F) <= chance)
								c.triggerPtAggro(Player.main.gameObject);
						}
					}
				}
	    	}
	    }
    
	    public static float getReachDistance() {
	    	return Player.main.GetVehicle() == null && VoidSpikesBiome.instance.isInBiome(Player.main.gameObject.transform.position) ? 3.5F : 2;
	    }
	    
	    public static bool checkTargetingSkip(bool orig, Transform obj) {
	    	if (!obj || !obj.gameObject)
	    		return orig;
	    	PrefabIdentifier id = obj.gameObject.FindAncestor<PrefabIdentifier>();
	    	if (!id)
	    		return orig;
	    	//SNUtil.log("Checking targeting skip of "+id+" > "+id.ClassId);
	    	if (id.ClassId == "b250309e-5ad0-43ca-9297-f79e22915db6" && Vector3.Distance(Player.main.transform.position, lrpowerSealSetpieceCenter) <= 8) { //to allow to hit the things inside the mouth
	    		//SNUtil.writeToChat("Is lr setpiece");
	    		return true;
	    	}
	    	if (VoidSpike.isSpike(id.ClassId) && VoidSpikesBiome.instance.isInBiome(obj.position)) {
	    		//SNUtil.writeToChat("Is void spike");
	    		return true;
	    	}
	    	return orig;
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
	    	if (UnityEngine.Random.Range(0F, 1F) < 0.88)
	    		return;
	    	int near = 0;
			foreach (Collider c in Physics.OverlapSphere(chunk.gameObject.transform.position, 4F)) {
				if (!c || !c.gameObject) {
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
	    
	    public static void getWaterTemperature(DIHooks.WaterTemperatureCalculation calc) {
	    	if (EnvironmentalDamageSystem.instance.TEMPERATURE_OVERRIDE >= 0) {
	    		calc.setValue(EnvironmentalDamageSystem.instance.TEMPERATURE_OVERRIDE);
	    		calc.lockValue();
	    		return;
	    	}
			//SNUtil.writeToChat("C2C: Checking water temp @ "+calc.position+" def="+calc.originalValue);
	    	if (Vector3.Distance(calc.position, mountainBaseGeoCenter) <= 20) {
	    		calc.setValue(Mathf.Min(calc.getTemperature(), 45));
	    	}
	    	string biome = EnvironmentalDamageSystem.instance.getBiome(calc.position);
	    	float poison = EnvironmentalDamageSystem.instance.getLRPoison(biome);
	    	if (poison > 0)
	    		calc.setValue(Mathf.Max(4, calc.getTemperature()-poison*1.75F)); //make LR cold, down to 4C (max water density point)
	    	if (biome == null || biome.ToLowerInvariant().Contains("void") && calc.position.y <= -50)
	    		calc.setValue(Mathf.Max(4, calc.getTemperature()+(calc.position.y+50)/20F)); //drop 1C per 20m below 50m, down to 4C around 550m
	    	double dist = VoidSpikesBiome.instance.getDistanceToBiome(calc.position, true);
	    	if (dist <= 500)
	    		calc.setValue((float)MathUtil.linterpolate(dist, 200, 500, VoidSpikesBiome.waterTemperature, calc.getTemperature(), true));
	    	if (VoidSpikesBiome.instance.isInBiome(calc.position)) {
	    		calc.setValue(VoidSpikesBiome.waterTemperature);
	    	}
	    	dist = UnderwaterIslandsFloorBiome.instance.getDistanceToBiome(calc.position);
	    	if (dist <= 150)
	    		calc.setValue((float)MathUtil.linterpolate(dist, 0, 150, UnderwaterIslandsFloorBiome.waterTemperature, calc.getTemperature(), true));
	    	if (UnderwaterIslandsFloorBiome.instance.isInBiome(calc.position))
	    		calc.setValue(calc.getTemperature()+UnderwaterIslandsFloorBiome.instance.getTemperatureBoost(calc.getTemperature(), calc.position));
	    	calc.setValue(Mathf.Max(calc.getTemperature(), EnvironmentalDamageSystem.instance.getWaterTemperature(calc.position)));
			foreach (HeatSinkTag lb in UnityEngine.Object.FindObjectsOfType<HeatSinkTag>()) {
				if (lb) {
					dist = Vector3.Distance(lb.transform.position, calc.position);
					if (dist <= EjectedHeatSink.HEAT_RADIUS) {
						float f = 1F-(float)(dist/EjectedHeatSink.HEAT_RADIUS);
						//SNUtil.writeToChat("Found heat sink "+lb.transform.position+" at dist "+dist+" > "+f+" > "+(f*lb.getTemperature()));
						calc.setValue(Mathf.Max(calc.getTemperature(), f*lb.getTemperature()));
					}
				}
			}/* Too expensive
	    	Geyser g = WorldUtil.getClosest<Geyser>(calc.position);
	    	if (g && g.erupting && calc.position.y > g.transform.position.y) {
	    		calc.setValue(Mathf.Max(calc.getTemperature(), 800-10*Vector3.Distance(g.transform.position, calc.position)));
	    	}
	    	calc.setValue(C2CMoth.getOverrideTemperature(calc.getTemperature()));*/
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
	    	PrefabIdentifier pi = go.FindAncestor<PrefabIdentifier>();
			if (pi && pi.ClassId == VanillaCreatures.SEA_TREADER.prefab) {
				//go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
				go.EnsureComponent<C2CTreader>();
	    	}
			else if (pi && pi.ClassId == "61ac1241-e990-4646-a618-bddb6960325b") {
	    		if (Vector3.Distance(go.transform.position, Player.main.transform.position) <= 40 && go.transform.position.y < -200) {
					PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.TreaderPooPrompt).key);
		    	}
	    	}
	    	else if (pi && pi.ClassId == "58247109-68b9-411f-b90f-63461df9753a" && Vector3.Distance(deepDegasiTablet, go.transform.position) <= 0.2) {
	    		GameObject go2 = ObjectUtil.createWorldObject(C2CItems.brokenOrangeTablet.ClassID);
	    		go2.transform.position = go.transform.position;
	    		go2.transform.rotation = go.transform.rotation;
	    		UnityEngine.Object.Destroy(go);
	    		return;
	    	}
	    	else if (pi && pi.ClassId == "1c34945a-656d-4f70-bf86-8bc101a27eee") {
	    		go.EnsureComponent<C2CMoth>();
	    	}
	    	else if (pi && pi.ClassId == "ba3fb98d-e408-47eb-aa6c-12e14516446b") { //prawn
	    		TemperatureDamage td = go.EnsureComponent<TemperatureDamage>();
	    		td.minDamageTemperature = 350;
	    		td.baseDamagePerSecond = Mathf.Max(10, td.baseDamagePerSecond)*0.5F;
	    		td.onlyLavaDamage = false;
	    		td.InvokeRepeating("UpdateDamage", 1f, 1f);
	    	}
	    	/*
	    	else if (pi && pi.ClassId == auroraStorageModule.prefabName && Vector3.Distance(auroraStorageModule.position, go.transform.position) <= 0.2) {
	    		go.transform.position = auroraCyclopsModule.position;
	    		go.transform.rotation = auroraCyclopsModule.rotation;
	    	}
	    	else if (pi && pi.ClassId == auroraCyclopsModule.prefabName && Vector3.Distance(auroraCyclopsModule.position, go.transform.position) <= 0.2) {
	    		go.transform.position = auroraStorageModule.position;
	    		go.transform.rotation = auroraStorageModule.rotation;
	    	}*/
	    }/*
	    
	    public static void onPingAdd(uGUI_PingEntry e, PingType type, string name, string text) {
	    	SNUtil.log("Ping ID type "+type+" = "+name+"|"+text+" > "+e.label.text);
	    }*/
	    
	    public static void updateSeamothModules(SeaMoth sm, int slotID, TechType tt, bool added) {
	    	if (added && tt == C2CItems.heatSinkModule.TechType) {
	    		sm.torpedoSilos[slotID].SetActive(true);
	    	}
	    }
	    
	    public static void useSeamothModule(SeaMoth sm, TechType tt, int slotID) {
			
	    }
	    
	    public static float getVehicleTemperature(Vehicle v) {
	    	if (v is SeaMoth) {
	    		C2CMoth cm = v.GetComponent<C2CMoth>();
	    		if (cm)
	    			return cm.getTemperature();
	    	}
	    	return WaterTemperatureSimulation.main.GetTemperature(v.transform.position);
	    }
    
	    public static bool isSpawnableVoid(string biome) {
	    	return VoidSpikeLeviathanSystem.instance.isSpawnableVoid(biome);
	    }
	    
	    public static GameObject getVoidLeviathan(VoidGhostLeviathansSpawner spawner, Vector3 pos) {
	    	return VoidSpikeLeviathanSystem.instance.getVoidLeviathan(spawner, pos);
	    }
	    
	    public static void tickVoidLeviathan(GhostLeviatanVoid gv) {
	    	VoidSpikeLeviathanSystem.instance.tickVoidLeviathan(gv);
	    }
	    
	    public static void pingSeamothSonar(SeaMoth sm) {
	    	VoidSpikeLeviathanSystem.instance.tagSeamothSonar(sm);
	    }
	    
	    public static void pulseSeamothDefence(SeaMoth sm) {
	    	VoidSpikeLeviathanSystem.instance.tagSeamothSonar(sm);
	    }
	    
	    public static void onBaseSonarPinged(GameObject go) {
	    	if (VoidSpikesBiome.instance.isInBiome(go.transform.position)) {
	    		Player ep = Player.main;
	    		Vehicle v = ep.GetVehicle();
	    		if (v && v is SeaMoth && VoidSpikesBiome.instance.isInBiome(ep.transform.position))
	    			VoidSpikeLeviathanSystem.instance.tagSeamothSonar((SeaMoth)v);
	    	}
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
	   
		public static string getO2Tooltip(Oxygen ox) {
	   		if (ox.GetComponent<Pickupable>().GetTechType() == C2CItems.liquidTank.TechType) {
	   			return ox.GetSecondsLeft()+"s fluid stored in supply tank";
	   		}
	   		return LanguageCache.GetOxygenText(ox.GetSecondsLeft());
		}
	   
		public static string getBatteryTooltip(Battery ox) {
	   		if (ox.GetComponent<Pickupable>().GetTechType() == C2CItems.liquidTank.TechType)
	   			return Mathf.RoundToInt(ox.charge)+"s fluid stored in primary tank";
	   		return Language.main.GetFormat<float, int, float>("BatteryCharge", ox.charge/ox.capacity, Mathf.RoundToInt(ox.charge), ox.capacity);
		}
	   
	   public static void onClickedVehicleUpgrades(VehicleUpgradeConsoleInput v) {
			if (v.docked || SeaToSeaMod.anywhereSeamothModuleCheatActive)
				v.OpenPDA();
	   }
	   
		public static void onHoverVehicleUpgrades(VehicleUpgradeConsoleInput v) {
			HandReticle main = HandReticle.main;
		   	if (!v.docked && !SeaToSeaMod.anywhereSeamothModuleCheatActive) {
				main.SetInteractText("DockToChangeVehicleUpgrades"); //locale key
				main.SetIcon(HandReticle.IconType.HandDeny, 1f);
		   	}
			else if (v.equipment != null) {
				main.SetInteractText(v.interactText);
				main.SetIcon(HandReticle.IconType.Hand, 1f);
			}
		}
	    
	    public static bool isObjectKnifeable(LiveMixin lv) {
	    	if (!lv)
	    		return true;
	    	AlkaliPlantTag a = lv.GetComponent<AlkaliPlantTag>();
	    	if (a) {
	    		return a.isHarvestable();
	    	}
	    	return !lv.weldable && lv.knifeable && !lv.GetComponent<EscapePod>();
	    }
	    
	    public static GameObject getStalkerShinyTarget(GameObject def, CollectShiny cc) {
	    	if (cc.shinyTarget && cc.GetComponent<DeepStalkerTag>()) {
	    		bool hasPlat = cc.shinyTarget.GetComponent<PlatinumTag>();
	    		bool lookingAtPlat = def.GetComponent<PlatinumTag>();
	    		if (hasPlat == lookingAtPlat)
	    			return def;
	    		else if (hasPlat)
	    			return cc.shinyTarget;
	    		else
	    			return def;
	    	}
	    	return def;
	    }
	    
	    public static void onShinyTargetIsCurrentlyHeldByStalker(CollectShiny cc) {
	    	if (cc.shinyTarget && cc.shinyTarget.GetComponent<PlatinumTag>()) {
	    		DeepStalkerTag ds = cc.GetComponent<DeepStalkerTag>();
	    		ds.tryStealFrom(cc.shinyTarget.GetComponentInParent<Stalker>());
	    	}
	    	else {
				cc.targetPickedUp = false;
				cc.shinyTarget = null;
	    	}
	    }
	    
	    public static bool stalkerTryDropTooth(Stalker s) {
	    	if (s.GetComponent<DeepStalkerTag>() && UnityEngine.Random.Range(0F, 1F) <= 0.8)
	    		return false;
	    	return s.LoseTooth();
	    }
	    
	    public static bool tryEat(Survival s, GameObject go) {
	    	if (LiquidBreathingSystem.instance.hasLiquidBreathing()) {
	    		SoundManager.playSoundAt(SoundManager.buildSound("event:/interface/select"), Player.main.transform.position, false, -1, 1);
	    		return false;
	    	}
	    	return s.Eat(go);
	    }
	}
}

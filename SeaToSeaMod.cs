using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{
  [QModCore]
  public static class SeaToSeaMod
  {
    public const string MOD_KEY = "ReikaKalseki.SeaToSea";
    
    public static readonly float ENVIRO_RATE_SCALAR = 4;
    
    public static readonly Config<C2CConfig.ConfigEntries> config = new Config<C2CConfig.ConfigEntries>();
    public static readonly XMLLocale locale = new XMLLocale("XML/items.xml");
    public static readonly XMLLocale pdas = new XMLLocale("XML/pda.xml");
    public static readonly XMLLocale signals = new XMLLocale("XML/signals.xml");
    
    public static SeamothVoidStealthModule voidStealth;
    public static SeamothDepthModule depth1300;
    public static CustomEquipable sealSuit;
    public static CustomEquipable rebreatherV2;
    public static CustomBattery t2Battery;
    
    public static AlkaliPlant alkali;
    
    public static Bioprocessor processor;

    [QModPatch]
    public static void Load()
    {
        config.load();
        
        Harmony harmony = new Harmony(MOD_KEY);
        Harmony.DEBUG = true;
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        SBUtil.log("Ran mod register, started harmony");
        try {
        	harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
        }
        catch (Exception e) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(e.Message);
			FileLog.Log(e.StackTrace);
			FileLog.Log(e.ToString());
        }
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(WorldGenerator).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PlacedObject).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CustomPrefab).TypeHandle);
        
        locale.load();
        pdas.load();
        signals.load();
        
        processor = new Bioprocessor();
        processor.Patch();
        SBUtil.log("Registered custom machine "+processor);
        
        addFlora();
        addItemsAndRecipes();
                 
        WorldgenDatabase.instance.load();
        DataboxTypingMap.instance.load();
        
        addCommands();
        addPDAEntries();
        addOreGen();
        /*
        GenUtil.registerWorldgen("00037e80-3037-48cf-b769-dc97c761e5f6", new Vector3(622.7F, -250.0F, -1122F), new Vector3(0, 32, 0)); //lifepod 13 (khasar)
        spawnDatabox(TechType.SwimChargeFins, new Vector3(622.7F, -249.3F, -1122F));
        */
       
		VoidSpikesBiome.instance.register();
		//AvoliteSpawner.instance.register();
       
        /*
        for (int i = 0; i < 12; i++) {
        	double r = UnityEngine.Random.Range(1.5F, 12);
        	double ang = UnityEngine.Random.Range(0, 360F);
        	double cos = Math.Cos(ang*Math.PI/180D);
        	double sin = Math.Sin(ang*Math.PI/180D);
        	double rx = r*cos;
        	double rz = r*sin;
        	bool big = UnityEngine.Random.Range(0, 1F) < 0.2;
        	Vector3 pos2 = new Vector3((float)(pos.x+rx), pos.y, (float)(pos.z+rz));
        	GenUtil.registerWorldgen(big ? VanillaResources.LARGE_KYANITE.prefab : VanillaResources.KYANITE.prefab, pos2);
        }*/
        
        //GenUtil.registerWorldgen(VanillaResources.LARGE_DIAMOND.prefab, new Vector3(-1496, -325, -714), new Vector3(120, 60, 45));
    }
    
    private static void addFlora() {
		alkali = new AlkaliPlant();
		alkali.Patch();	
		alkali.addPDAEntry(locale.getEntry(alkali.ClassID).pda, 3);
		SBUtil.log(" > "+alkali);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Mountains_IslandCaveFloor, 1, 1F);
		//GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Mountains_CaveFloor, 1, 0.5F);
		//GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Dunes_CaveFloor, 1, 0.5F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.KooshZone_CaveFloor, 1, 2F);
    }
    
    private static void addOreGen() {
    	BasicCustomOre vent = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL);
    	vent.registerWorldgen(BiomeType.Dunes_ThermalVent, 1, 3F);
    	vent.registerWorldgen(BiomeType.Mountains_ThermalVent, 1, 1.2F);
    	//vent.registerWorldgen(BiomeType.JellyshroomCaves_Geyser, 1, 0.5F);
    	//vent.registerWorldgen(BiomeType.KooshZone_Geyser, 1, 1F);
    	//vent.registerWorldgen(BiomeType.GrandReef_ThermalVent, 1, 3F);
    	//vent.registerWorldgen(BiomeType.DeepGrandReef_ThermalVent, 1, 4F);
    	
    	BasicCustomOre irid = CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor, 1, 1.5F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor_Far, 1, 0.75F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Wall, 1, 0.25F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Chamber_Ceiling, 1, 2F);
    	
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.UnderwaterIslands_Geyser, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.DrillableMagnetite), BiomeType.UnderwaterIslands_Geyser, 0.2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Lithium), BiomeType.UnderwaterIslands_Geyser, 1.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Quartz), BiomeType.UnderwaterIslands_Geyser, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Diamond), BiomeType.UnderwaterIslands_Geyser, 1F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Quartz), BiomeType.UnderwaterIslands_ValleyFloor, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Lithium), BiomeType.UnderwaterIslands_ValleyFloor, 1F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.DrillableQuartz), BiomeType.UnderwaterIslands_ValleyFloor, 0.2F, 1);
    	vent.registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 0.5F);
    	//CustomMaterials.getItem(CustomMaterials.Materials.).registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 8F);
    	/*
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Dunes_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Mountains_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.GrandReef_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.DeepGrandReef_ThermalVent, 2F, 1);*/
    }
    
    private static void addCommands() {
        BuildingHandler.instance.addCommand<string>("pfb", BuildingHandler.instance.spawnPrefabAtLook);
        //BuildingHandler.instance.addCommand<string>("btt", BuildingHandler.instance.spawnTechTypeAtLook);
        BuildingHandler.instance.addCommand<bool>("bden", BuildingHandler.instance.setEnabled);  
        BuildingHandler.instance.addCommand("bdsa", BuildingHandler.instance.selectAll);
        BuildingHandler.instance.addCommand("bdslp", BuildingHandler.instance.selectLastPlaced);
        BuildingHandler.instance.addCommand<string>("bdexs", BuildingHandler.instance.saveSelection);
        BuildingHandler.instance.addCommand<string>("bdexa", BuildingHandler.instance.saveAll);
        BuildingHandler.instance.addCommand<string>("bdld", BuildingHandler.instance.loadFile);
        BuildingHandler.instance.addCommand("bdinfo", BuildingHandler.instance.selectedInfo);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, bool>>("sound", SBUtil.playSound);
       // ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("voidsig", VoidSpikesBiome.instance.activateSignal);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, string, string>>("exec", DebugExec.run);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("execTemp", DebugExec.tempCode);
    }
    
    private static void addItemsAndRecipes() {
        BasicCraftingItem comb = CraftingItems.getItem(CraftingItems.Items.HoneycombComposite);
        comb.craftingTime = 12;
        comb.addIngredient(TechType.AramidFibers, 6).addIngredient(TechType.PlasteelIngot, 1);
        
        BasicCraftingItem lens = CraftingItems.getItem(CraftingItems.Items.CrystalLens);
        lens.craftingTime = 20;
        lens.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL), 45).addIngredient(TechType.Diamond, 9).addIngredient(TechType.Magnetite, 24);
        
        BasicCraftingItem sealedFabric = CraftingItems.getItem(CraftingItems.Items.SealFabric);
        sealedFabric.craftingTime = 4;
        sealedFabric.numberCrafted = 2;
        sealedFabric.addIngredient(CraftingItems.getItem(CraftingItems.Items.Sealant), 5).addIngredient(TechType.AramidFibers, 3).addIngredient(TechType.StalkerTooth, 1).addIngredient(TechType.Silicone, 2);
        
        BasicCraftingItem armor = CraftingItems.getItem(CraftingItems.Items.HullPlating);
        armor.craftingTime = 9;
        armor.addIngredient(TechType.PlasteelIngot, 2).addIngredient(TechType.Lead, 5).addIngredient(comb, 1);
        
        CraftingItems.addAll();
        
        voidStealth = new SeamothVoidStealthModule();
        voidStealth.addIngredient(lens, 1).addIngredient(comb, 2).addIngredient(TechType.Aerogel, 12);
        voidStealth.Patch();
        
        depth1300 = new SeamothDepthModule("SMDepth4", "Seamoth Depth Module MK4", "Increases crush depth to 1300m.", 1300);
        //depth1300.addIngredient(lens, 1).addIngredient(comb, 2).addIngredient(TechType.Aerogel, 12);
        depth1300.Patch();
        /*
        CraftData.itemSizes[TechType.AcidMushroom] = new Vector2int(1, 2);
        CraftData.itemSizes[TechType.HydrochloricAcid] = new Vector2int(2, 2);
        RecipeUtil.modifyIngredients(TechType.HydrochloricAcid, i => i.amount = 12);
        */
		RecipeUtil.removeRecipe(TechType.HydrochloricAcid);
		RecipeUtil.removeRecipe(TechType.Benzene);
		
        sealSuit = new SealedSuit();
        sealSuit.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 9).addIngredient(CraftingItems.getItem(CraftingItems.Items.SealFabric), 6);
        sealSuit.Patch();
		
		t2Battery = new CustomBattery(locale.getEntry("t2battery"), 500);
        t2Battery.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL), 2).addIngredient(TechType.Polyaniline, 1).addIngredient(TechType.Lithium, 2).addIngredient(TechType.Magnetite, 5);
		t2Battery.Patch();
		
        rebreatherV2 = new RebreatherV2();
        rebreatherV2.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 6).addIngredient(TechType.Benzene, 12).addIngredient(TechType.Silicone, 3).addIngredient(TechType.Rebreather, 1).addIngredient(t2Battery, 1);
        rebreatherV2.Patch();
        
        RecipeUtil.addIngredient(TechType.StasisRifle, CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 4);
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
    
    public static void addPDAEntries() {
    	foreach (XMLLocale.LocaleEntry e in pdas.getEntries()) {
			PDAManager.PDAPage page = PDAManager.createPage(e);
			if (e.hasField("audio"))
				page.setVoiceover(e.getField<string>("audio"));
			if (e.hasField("header"))
				page.setHeaderImage(TextureManager.getTexture(e.getField<string>("header")));
			page.register();
    	}
    }
    
    public static void addSignals(SignalDatabase db) {/*
    	foreach (XMLLocale.LocaleEntry e in signals.getEntries()) {
    		string id = e.getField<string>("id", null);
    		if (string.IsNullOrEmpty(id))
    			throw new Exception("Missing id for signal '"+e.dump()+"'!");
    		Int3 pos = Int3.zero;
    		if (e.hasField("location")) {
    			pos = e.getField<Int3>("location", pos);
    		}
    		if (id == "voidpod") {
    			pos = VoidSpikesBiome.signalLocation.roundToInt3();
    		}
    		if (string.IsNullOrEmpty(e.name) || string.IsNullOrEmpty(e.desc)) {
    			throw new Exception("Missing data for signal '"+id+"'!");
    		}    	
    		if (pos == Int3.zero)
    			throw new Exception("Missing location for signal '"+id+"'!");
    		Vector3 vec = pos.ToVector3();
    		SignalInfo info = new SignalInfo {
    			biome = SBUtil.getBiome(vec),
    			batch = SBUtil.getBatch(vec),
    			position = pos,
    			description = e.desc
    		};
    		SBUtil.log("Injected signal "+id+" @ "+pos+": "+info);
    		db.entries.Add(info);
    	}*/
    }
    
    private static bool worldLoaded = false;
    
    public static void onWorldLoaded() {
    	worldLoaded = true;
    	SBUtil.log("Intercepted world load");
        
    	VoidSpikesBiome.instance.onWorldStart();
    }
    /*
    public static void onEntitySpawn(LargeWorldEntity p) {
    	TechTag tag = p.GetComponent<TechTag>();
    	if (tag != null && tag.type == TechType.PrecursorKey_Purple) {
    		GameObject repl = SBUtil.createWorldObject(CraftData.GetClassIdForTechType(TechType.PrecursorKey_PurpleFragment));
    		repl.transform.position = p.gameObject.transform.position;
    		repl.transform.rotation = p.gameObject.transform.rotation;
    		UnityEngine.Object.Destroy(p.gameObject);
    	}
    }
    */
    public static void tickPlayer(Player ep) {
    	/*
    	if (ep.GetVehicle() == null && ep.gameObject.transform.position.y < -500 && !ep.CanBreathe() && Inventory.main.equipment.GetCount(TechType.Rebreather) == 0) {
    		
    	}*//*
    	Vehicle v = ep.GetVehicle();
    	if (v == null && ep.currentSub == null) {
    		float y = -ep.gameObject.transform.position.y;
    		float ymin = 500;
    		if (y > ymin) {
    			float ymax = 600;
    			float f = y >= ymax ? 1 : (y-ymin)/(ymax-ymin);
    			LiveMixin live = ep.gameObject.GetComponentInParent<LiveMixin>();
    			if (live != null && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0) {
    				ep.GetComponentInParent<LiveMixin>().TakeDamage(3*f, ep.transform.position, DamageType.Pressure, ep.gameObject); //TODO make use time elapsed
    			}
    		}
    	}
    	else if (v != null || (ep.currentSub != null && ep.currentSub.isCyclops)) {
    		string biome = ep.GetBiomeString();
    		float amt = -1;
    		switch(biome) {
    			
    		}
    		if (amt > 0) {
    			if (v != null)
    				v.ConsumeEnergy(amt);
    			else
    				ep.currentSub.power?;
    		}
    	}*/
    }
   
	public static void doEnvironmentalDamage(TemperatureDamage dmg) { //TODO rebalance
   		//SBUtil.writeToChat("Doing enviro damage on "+dmg+" in "+dmg.gameObject+" = "+dmg.player);
   		if (dmg.player && (dmg.player.IsInsideWalkable() || !dmg.player.IsSwimming()))
   			return;
		float temperature = dmg.GetTemperature();
		float f = 1;
		float f0 = 1;
    	if (dmg.player) {
    		f0 = Inventory.main.equipment.GetCount(TechType.ReinforcedDiveSuit) == 0 ? 2.5F : 0.4F;
    		string biome = dmg.player.GetBiomeString();
    		//SBUtil.writeToChat(biome+" HH# "+dmg.gameObject);
    		switch(biome) {
    			case "ILZCorridor":
    				temperature = 90;
    				f = 8;
    				break;
    			case "ILZChamber":
    				temperature = 120;
    				f = 10;
    				break;
    			case "LavaPit":
    				temperature = 140;
    				f = 12;
    				break;
    			case "LavaFalls":
    				temperature = 160;
    				f = 15;
    				break;
    			case "LavaLakes":
    				temperature = 240;
    				f = 18;
    				break;
    			case "ilzLava":
    				temperature = 1200;
    				f = 24;
    				break;
    			default:
    				break;
    		}
		}
		if (temperature >= dmg.minDamageTemperature) {
			float num = temperature / dmg.minDamageTemperature;
			num *= dmg.baseDamagePerSecond;
			dmg.liveMixin.TakeDamage(num*f*f0/ENVIRO_RATE_SCALAR, dmg.transform.position, DamageType.Heat, null);
		}
    	if (dmg.player) {
	    	float y = -dmg.player.gameObject.transform.position.y;
	    	float ymin = 500;
    		if (y > ymin && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0) {
    			float ymax = 600;
    			float f2 = y >= ymax ? 1 : (y-ymin)/(ymax-ymin);
    			dmg.liveMixin.TakeDamage(30*f2/ENVIRO_RATE_SCALAR, dmg.transform.position, DamageType.Pressure, null);
    		}
	    	//if (Inventory.main.equipment.GetCount(sealSuit.TechType) == 0) {
	    		string biome = dmg.player.GetBiomeString();
	    		//SBUtil.writeToChat(biome+" # "+dmg.gameObject);
	    		float amt = -1;
	    		switch(biome) {
	    			case "LostRiver_BonesField_Corridor":
	    			case "LostRiver_GhostTree":
	    			case "LostRiver_Corridor":
	    				amt = 18;
	    				break;
	    			case "LostRiver_Canyon":
	    				amt = 24;
	    				break;
	    			case "LostRiver_BonesField":
	    			case "LostRiver_Junction":
	    			//case "LostRiver_TreeCove":
	    			case "LostRiver_GhostTree_Lower":
	    				amt = 40;
	    				break;
	    			default:
	    				break;
	    		}
	    		if (amt > 0) {
	    			dmg.liveMixin.TakeDamage(amt/ENVIRO_RATE_SCALAR, dmg.transform.position, DamageType.Poison, null);
	    		}
	    	//}
    	}
		else {
    		string biome = Player.main.GetBiomeString();
    		Vehicle v = dmg.gameObject.GetComponentInParent<Vehicle>();
    		if (!v.docked) {
	    		//SBUtil.writeToChat(biome+" # "+dmg.gameObject);
	    		float amt = -1;
	    		switch(biome) {
	    			case "LostRiver_BonesField_Corridor":
	    			case "LostRiver_BonesField":
	    			case "LostRiver_Junction":
	    			case "LostRiver_TreeCove":
	    			case "LostRiver_Corridor":
	    			case "LostRiver_GhostTree_Lower":
	    			case "LostRiver_GhostTree":
	    				amt = 2;
	    				break;
	    			case "LostRiver_Canyon":
	    				amt = 5;
	    				break;
	    			default:
	    				break;
	    		}
	    		if (v.playerSits)
	    			amt *= 2;
	    		if (amt > 0) { //TODO trigger PDA prompt and entry, like void
	    			v.ConsumeEnergy(amt/ENVIRO_RATE_SCALAR);
	    		}
    		}
		}
 	}
   
	public static float CalculateDamage(float damage, DamageType type, GameObject target, GameObject dealer) {
   		Player p = target.GetComponentInParent<Player>();
   		if (p != null && Inventory.main.equipment.GetCount(sealSuit.TechType) != 0) {
   			if (type == DamageType.Poison || type == DamageType.Acid || type == DamageType.Electrical) {
   				damage *= 0.2F;
   				damage -= 10;
   				if (damage < 0)
   					damage = 0;
   			}
   		}
   		return damage;
	}
    /*
    public static float getPlayerO2Use(Player ep, float breathingInterval, int depthClass) {
		if (!GameModeUtils.RequiresOxygen())
			return 0;
		float num = 1;
		if (ep.mode != Player.Mode.Piloting && ep.mode != Player.Mode.LockedPiloting) {
			bool hasRebreatherV2 = Inventory.main.equipment.GetCount(rebreatherV2.TechType) != 0;
			bool hasRebreather = hasRebreatherV2 || Inventory.main.equipment.GetCount(TechType.Rebreather) != 0;
			if (!hasRebreather) {
				if (depthClass == 2) {
					num = 1.5F;
				}
				else if (depthClass == 3) {
					num = 2;
				}
			}			
			if (depthClass >= 3 && !hasRebreatherV2 && ep.gameObject.transform.position.y < -500) {
				num = 30;
			}
		}
		return breathingInterval * num;
    }*/
    
    public static void onItemPickedUp(Pickupable p) {
    	if (p.GetTechType() == CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType) {
			if (Inventory.main.equipment.GetCount(SeaToSeaMod.sealSuit.TechType) == 0 || Inventory.main.equipment.GetCount(TechType.SwimChargeFins) != 0) {
				Player.main.gameObject.GetComponentInParent<LiveMixin>().TakeDamage(25, Player.main.gameObject.transform.position, DamageType.Electrical, Player.main.gameObject);
			}
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
    
    public static void onDataboxActivate(BlueprintHandTarget c) {
    	//SBUtil.log("original databox unlock being reprogrammed on 'activate' from: "+c.unlockTechType);
    	//SBUtil.log(c.gameObject.ToString());
    	//SBUtil.log(c.gameObject.transform.ToString());
    	//SBUtil.log(c.gameObject.transform.position.ToString());
    	//SBUtil.log(c.gameObject.transform.eulerAngles.ToString());
    	
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
    /*
    public static bool onDataboxUsed(TechType recipe, bool verb, BlueprintHandTarget c) {
    	bool flag = KnownTech.Add(recipe, verb);
    	SBUtil.log("Used databox: "+recipe);
    	SBUtil.writeToChat(c.gameObject.ToString());
    	SBUtil.writeToChat(c.gameObject.transform.position.ToString());
    	SBUtil.writeToChat(c.gameObject.transform.eulerAngles.ToString());
    	return flag;
    }*/
    
    public static GameObject interceptScannerTarget(GameObject original, ref PDAScanner.ScanTarget tgt) { //the GO is the collider, NOT the parent
    	/*
    	if (original != null) {
    		GameObject root = original.transform.parent.gameObject;
    		ResourceTracker res = root.GetComponent<ResourceTracker>();
    		if (res != null && Enum.GetName(typeof(TechType), res.techType).Contains("Fragment")) { //FIXME "== Fragment" does not catch seamoth ("SeamothFragment") and deco fragments (direct name, eg BarTable or StarshipChair [those also have null res])
    			//SBUtil.dumpObjectData(original);
    			TechTag tag = root.GetComponent<TechTag>();
    			if (tag != null) {
    				SBUtil.log("frag: "+tag.type);
			    	RaycastHit[] hit = UnityEngine.Physics.SphereCastAll(root.transform.position, 32, new Vector3(1, 0, 0), 32);
			    	if (hit.Length > 0) {
			    		SBUtil.log("found "+hit.Length);
			    		foreach (RaycastHit r in hit) {
			    			GameObject go = r.transform.gameObject;
			    			if (go != null && go.GetComponent<LargeWorldEntity>() != null) {
			    				//SBUtil.dumpObjectData(go);
			    				if (go.GetComponent<ResourceTracker>() != null && go.GetComponent<Pickupable>() != null && go.GetComponent<ResourceTracker>().techType == TechType.Fragment && go.GetComponent<Pickupable>().GetTechType() != tag.type) {
				    				SBUtil.log("becomes: "+go);
				    				//go.transform.position.y += 2;
				    				return go;
				    			}
			    			}
			    		}
			    	}
    			}
    		}
    	}*/
    	return original;
    }
    
    public static TechType onFragmentScanned(TechType original) {
    	PDAScanner.ScanTarget tgt = PDAScanner.scanTarget;
    	SBUtil.log("Scanned fragment: "+original);
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
	    			if (ii != null && ii.item.GetTechType() != TechType.None && ii.item.GetTechType() == voidStealth.TechType) {
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
    
    public static void updateCrushDamage(float val) {
    	SBUtil.log(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name+": update crush to "+val+" from trace "+System.Environment.StackTrace);
    	SBUtil.writeToChat("update crush to "+val);
    }
    
    public static void updateSeamothModules(SeaMoth sm, int slotID, TechType techType, bool added) {
		for (int i = 0; i < sm.slotIDs.Length; i++) {
			string slot = sm.slotIDs[i];
			TechType techTypeInSlot = sm.modules.GetTechTypeInSlot(slot);
    		SBUtil.writeToChat(i+": "+techTypeInSlot);
			if (techTypeInSlot == depth1300.TechType) {
				sm.crushDamage.SetExtraCrushDepth(depth1300.depthBonus);
			}
		}
    	/*
		if (slotID >= 0 && slotID < sm.storageInputs.Length) {
			sm.storageInputs[slotID].SetEnabled(added && techType == TechType.VehicleStorageModule);
			GameObject gameObject = sm.torpedoSilos[slotID];
			if (gameObject != null)
			{
				gameObject.SetActive(added && techType == TechType.SeamothTorpedoModule);
			}
		}
    	switch (techType) {
    		case TechType.SeamothSolarCharge:
    			break;
    		case TechType.SeamothReinforcementModule:
    			break;
    		case TechType.VehicleHullModule1:
    			break;
    		case TechType.VehicleHullModule2:
    			break;
    		case TechType.VehicleHullModule3:
    			break;
    	}
		int count = sm.modules.GetCount(techType);
		if (techType != TechType.SeamothReinforcementModule)
		{
			if (techType != TechType.SeamothSolarCharge)
			{
				if (techType - TechType.VehicleHullModule1 <= 2)
				{
					goto IL_7D;
				}
				sm.OnUpgradeModuleChange(slotID, techType, added);
			}
			else
			{
				sm.CancelInvoke("UpdateSolarRecharge");
				if (count > 0)
				{
					sm.InvokeRepeating("UpdateSolarRecharge", 1f, 1f);
					return;
				}
			}
			return;
		}
		IL_7D:
		Dictionary<TechType, float> dictionary = new Dictionary<TechType, float>
		{
			{
				TechType.SeamothReinforcementModule,
				800f
			},
			{
				TechType.VehicleHullModule1,
				100f
			},
			{
				TechType.VehicleHullModule2,
				300f
			},
			{
				TechType.VehicleHullModule3,
				700f
			}
		};
		float num = 0f;
		for (int i = 0; i < sm.slotIDs.Length; i++)
		{
			string slot = sm.slotIDs[i];
			TechType techTypeInSlot = sm.modules.GetTechTypeInSlot(slot);
			if (dictionary.ContainsKey(techTypeInSlot))
			{
				float num2 = dictionary[techTypeInSlot];
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		sm.crushDamage.SetExtraCrushDepth(num);*/
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

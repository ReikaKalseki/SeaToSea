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
  public class SeaToSeaMod
  {
    public const string MOD_KEY = "ReikaKalseki.SeaToSea";
    
    public static readonly Config<C2CConfig.ConfigEntries> config = new Config<C2CConfig.ConfigEntries>();
    public static readonly XMLLocale locale = new XMLLocale("XML/items.xml");
    public static readonly XMLLocale signals = new XMLLocale("XML/signals.xml");
    
    private static SeamothVoidStealthModule voidStealth;
    private static SeamothDepthModule depth1300;

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
        signals.load();
        
        addItemsAndRecipes();
                 
        WorldgenDatabase.instance.load();
        DataboxTypingMap.instance.load();
        
        addCommands();
        
        addOreGen();
        /*
        GenUtil.registerWorldgen("00037e80-3037-48cf-b769-dc97c761e5f6", new Vector3(622.7F, -250.0F, -1122F), new Vector3(0, 32, 0)); //lifepod 13 (khasar)
        spawnDatabox(TechType.SwimChargeFins, new Vector3(622.7F, -249.3F, -1122F));
        */
       
		VoidSpikesBiome.instance.register();
       
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
    
    private static void addOreGen() {
    	BasicCustomOre mountainCaveResource = CustomMaterials.getItem(CustomMaterials.Materials.MOUNTAIN_CRYSTAL);
    	mountainCaveResource.registerWorldgen(BiomeType.Mountains_CaveFloor, 1, 0.1F);
    	mountainCaveResource.registerWorldgen(BiomeType.Mountains_CaveWall, 1, 0.1F);
    	mountainCaveResource.registerWorldgen(BiomeType.Mountains_CaveCeiling, 1, 0.1F);
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
    }
    
    private static void addItemsAndRecipes() {
        BasicCraftingItem comb = CraftingItems.getItem(CraftingItems.Items.HoneycombComposite);
        comb.craftingTime = 12;
        comb.addIngredient(TechType.AramidFibers, 6).addIngredient(TechType.PlasteelIngot, 1);
        
        BasicCraftingItem lens = CraftingItems.getItem(CraftingItems.Items.CrystalLens);
        lens.craftingTime = 20;
        lens.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.MOUNTAIN_CRYSTAL).TechType, 30).addIngredient(TechType.Diamond, 3).addIngredient(TechType.Magnetite, 1);
        
        CraftingItems.addAll();
        
        voidStealth = new SeamothVoidStealthModule();
        voidStealth.addIngredient(lens, 1).addIngredient(comb, 2).addIngredient(TechType.Aerogel, 12);
        voidStealth.Patch();
        
        depth1300 = new SeamothDepthModule("SMDepth4", "Seamoth Depth Module MK4", "Increases crush depth to 1300m.", 1300);
        //depth1300.addIngredient(lens, 1).addIngredient(comb, 2).addIngredient(TechType.Aerogel, 12);
        depth1300.Patch();
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
    
    public static void addSignals(SignalDatabase db) {
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
    		db.entries.Add(info);
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
    	GameObject prefab;
		if (UWE.PrefabDatabase.TryGetPrefab(VanillaResources.KYANITE.prefab, out prefab)) { //TODO switch this over
			if (prefab != null) {
    			//res.prefabList.Insert(0, pfb);
    			//SBUtil.writeToChat("Added "+pfb.prefab+" to "+chunk+" @ "+chunk.transform.position);
    			GameObject placed = UnityEngine.Object.Instantiate(prefab, owner.transform.position, owner.transform.rotation);
    			UnityEngine.Object.Destroy(owner);
			}
			else {
				SBUtil.writeToChat("Prefab found and placed succeeeded but resulted in null?!");
			}
		}
		else {
			SBUtil.writeToChat("Prefab found but was null?!");
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
    	int maxd = far ? 200 : 900; //TODO rebalance
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

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
    
    private static SeamothVoidStealthModule voidStealth;
    private static BasicCraftingItem honeycombComposite;
    private static BasicCraftingItem crystalLens;

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
        
        TechType mountainCaveResource = TechType.MercuryOre; //TODO replace mercury with unique new mountain cave resource
        
        honeycombComposite = new BasicCraftingItem("HoneycombComposite", "Honeycomb Composite Plating", "A lightweight and low-conductivity panel.");
        honeycombComposite.isAdvanced = true;
        honeycombComposite.unlockRequirement = TechType.Fabricator;
        honeycombComposite.addIngredient(TechType.AramidFibers, 6).addIngredient(TechType.PlasteelIngot, 1);
        honeycombComposite.Patch();
        crystalLens = new BasicCraftingItem("CrystalLens", "Refractive Lens", "A lens with the ability to refract several kinds of matter.");
        crystalLens.isAdvanced = true;
        crystalLens.unlockRequirement = TechType.Fabricator;
        crystalLens.addIngredient(mountainCaveResource, 30).addIngredient(TechType.Diamond, 3).addIngredient(TechType.Magnetite, 1);
        crystalLens.Patch();
        
        voidStealth = new SeamothVoidStealthModule();
        voidStealth.addIngredient(crystalLens.getTechType(), 1).addIngredient(honeycombComposite.getTechType(), 2).addIngredient(TechType.Aerogel, 12);
        voidStealth.Patch();
        
        WorldgenDatabase.instance.load();
        DataboxTypingMap.instance.load();
        
        //CommandHandler.instance.registerCommand("buildprefab", BuildingHandler.instance.spawnPrefabAtLook);
        //DevConsole.RegisterConsoleCommand(new test(), "makepfb");
        BuildingHandler.instance.addCommand<string>("pfb", BuildingHandler.instance.spawnPrefabAtLook);
        //BuildingHandler.instance.addCommand<string>("btt", BuildingHandler.instance.spawnTechTypeAtLook);
        BuildingHandler.instance.addCommand<bool>("bden", BuildingHandler.instance.setEnabled);  
        BuildingHandler.instance.addCommand("bdsa", BuildingHandler.instance.selectAll);
        BuildingHandler.instance.addCommand<string>("bdexs", BuildingHandler.instance.saveSelection);
        BuildingHandler.instance.addCommand<string>("bdexa", BuildingHandler.instance.saveAll);
        BuildingHandler.instance.addCommand<string>("bdld", BuildingHandler.instance.loadFile);
        
        string mountainCaveID = CraftData.GetClassIdForTechType(mountainCaveResource);
        string mountainCavePath;
        UWE.PrefabDatabase.TryGetPrefabFilename(mountainCaveID, out mountainCavePath);
        List<LootDistributionData.BiomeData> li = new List<LootDistributionData.BiomeData>();
        li.Add(new LootDistributionData.BiomeData{biome = BiomeType.Mountains_CaveFloor, count = 1, probability = 0.1F});
        li.Add(new LootDistributionData.BiomeData{biome = BiomeType.Mountains_CaveWall, count = 1, probability = 0.1F});
        li.Add(new LootDistributionData.BiomeData{biome = BiomeType.Mountains_CaveCeiling, count = 1, probability = 0.1F});
        UWE.WorldEntityInfo info = null;//new UWE.WorldEntityInfo();
        UWE.WorldEntityDatabase.TryGetInfo(mountainCaveID, out info);
       	WorldEntityDatabaseHandler.AddCustomInfo(mountainCaveID, info);
        LootDistributionHandler.AddLootDistributionData(mountainCaveID, mountainCavePath, li, info);
        /*
        GenUtil.registerWorldgen("00037e80-3037-48cf-b769-dc97c761e5f6", new Vector3(622.7F, -250.0F, -1122F), new Vector3(0, 32, 0)); //lifepod 13 (khasar)
        spawnDatabox(TechType.SwimChargeFins, new Vector3(622.7F, -249.3F, -1122F));
        */
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
    			float s = KeyCodeUtils.GetKeyHeld(KeyCode.C) ? 0.15F : (KeyCodeUtils.GetKeyHeld(KeyCode.X) ? 0.02F : 0.05F);
	    		BuildingHandler.instance.manipulateSelected(s);
	    	}
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
    		c.unlockTechType = over;
    	}
    }
    
    public static void onCrateActivate(SupplyCrate c) {
    	//SBUtil.log("original databox unlock being reprogrammed on 'activate' from: "+c.unlockTechType);
    	//SBUtil.log(c.gameObject.ToString());
    	//SBUtil.log(c.gameObject.transform.ToString());
    	//SBUtil.log(c.gameObject.transform.position.ToString());
    	//SBUtil.log(c.gameObject.transform.eulerAngles.ToString());
    	
    	TechType over = CrateFillMap.instance.getOverride(c);
    	if (over != TechType.None) {
    		SBUtil.log("Crate @ "+c.gameObject.transform.ToString()+", previously "+c.itemInside+", found an override to "+over);
    		setCrateItem(c, over);
    	}
    }
    
    public static void setCrateItem(SupplyCrate c, TechType item) {
    	/*
	   	if (c.itemInside == null) {
    		c.itemInside = c.transform.gameObject.AddComponent<Pickupable>();
    	}
		//TODO fix crate item set
		*//*
		typeof(SupplyCrate).GetMethod("FindInsideItemAfterStart", unchecked((System.Reflection.BindingFlags)0x7fffffff)).Invoke(c, new object[0]);
		SBUtil.log("T"+c.transform);
		SBUtil.dumpObjectData(c.transform);
		SBUtil.log("P"+c.transform.GetComponentInChildren<Pickupable>());
		Pickupable p = c.transform.GetComponentInChildren<Pickupable>();
		p.SetTechTypeOverride(item);
		GameObject use = CraftData.GetPrefabForTechType(item);//Utils.CreateGenericLoot(item);*/
		//p.
    }
    
    public static bool onDataboxUsed(TechType recipe, bool verb, BlueprintHandTarget c) {
    	bool flag = KnownTech.Add(recipe, verb);
    	SBUtil.log("Used databox: "+recipe);
    	SBUtil.writeToChat(c.gameObject.ToString());
    	SBUtil.writeToChat(c.gameObject.transform.position.ToString());
    	SBUtil.writeToChat(c.gameObject.transform.eulerAngles.ToString());
    	return flag;
    }
    
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
	    			if (ii != null && ii.item.GetTechType() != TechType.None && ii.item.GetTechType() == voidStealth.getTechType()) {
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
    
    public static void updateSeamothModules(SeaMoth sm, int slotID, TechType techType, bool added) {
    	
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

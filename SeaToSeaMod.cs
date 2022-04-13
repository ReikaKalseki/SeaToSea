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
        
        //CommandHandler.instance.registerCommand("buildprefab", BuildingHandler.instance.spawnPrefabAtLook);
        //DevConsole.RegisterConsoleCommand(new test(), "makepfb");
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string>>("pfb", BuildingHandler.instance.spawnPrefabAtLook);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("builder", BuildingHandler.instance.setEnabled);
        
        
        GenUtil.registerWorldgen("00037e80-3037-48cf-b769-dc97c761e5f6", new Vector3(622.7F, -250.0F, -1122F), new Vector3(0, 32, 0)); //lifepod 13 (khasar)
        spawnDatabox(TechType.SwimChargeFins, new Vector3(622.7F, -249.3F, -1122F));
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
    
    private static void spawnDatabox(TechType tech, Vector3 pos) {
    	spawnDatabox(tech, pos, Vector3.zero);
    }
    
    private static void spawnDatabox(TechType tech, Vector3 pos, double rotY) {
    	spawnDatabox(tech, pos, new Vector3(0, (float)rotY, 0));
    }
    
    private static void spawnDatabox(TechType tech, Vector3 pos, Vector3 rot) {
        GenUtil.spawnDatabox(pos, rot);
    	DataboxTypingMap.instance.addValue(pos, tech);
    }
    
    public static void onTick(DayNightCycle cyc) {
    	if (BuildingHandler.instance.isEnabled) {
	    	if (GameInput.GetButtonDown(GameInput.Button.LeftHand)) {
	    		BuildingHandler.instance.handleClick(KeyCodeUtils.GetKeyHeld(KeyCode.LeftControl));
	    	}
	    	
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Delete)) {
	    		BuildingHandler.instance.deleteSelected();
	    	}
	    	
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftAlt)) {
	    		float s = KeyCodeUtils.GetKeyHeld(KeyCode.C) ? 0.25F : 0.05F;
	    		if (KeyCodeUtils.GetKeyHeld(KeyCode.UpArrow))
	    			BuildingHandler.instance.moveSelected(new Vector3(0, 0, s));
	    		if (KeyCodeUtils.GetKeyHeld(KeyCode.DownArrow))
	    			BuildingHandler.instance.moveSelected(new Vector3(0, 0, -s));
	    		if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftArrow))
	    			BuildingHandler.instance.moveSelected(new Vector3(-s, 0, 0));
	    		if (KeyCodeUtils.GetKeyHeld(KeyCode.RightArrow))
	    			BuildingHandler.instance.moveSelected(new Vector3(s, 0, 0));
	    		if (KeyCodeUtils.GetKeyHeld(KeyCode.R))
	    			BuildingHandler.instance.rotateSelectedYaw(1);
	    		if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftBracket))
	    			BuildingHandler.instance.rotateSelected(-1, 0, 0);
	    		if (KeyCodeUtils.GetKeyHeld(KeyCode.RightBracket))
	    			BuildingHandler.instance.rotateSelected(1, 0, 0);
	    		if (KeyCodeUtils.GetKeyHeld(KeyCode.Comma))
	    			BuildingHandler.instance.rotateSelected(0, 0, -1);
	    		if (KeyCodeUtils.GetKeyHeld(KeyCode.Period))
	    			BuildingHandler.instance.rotateSelected(0, 0, 1);
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

  }
}

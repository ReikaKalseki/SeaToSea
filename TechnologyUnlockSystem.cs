using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class TechnologyUnlockSystem {
		
		public static readonly TechnologyUnlockSystem instance = new TechnologyUnlockSystem();
	    
	    private readonly Dictionary<TechType, List<TechType>> directUnlocks = new Dictionary<TechType, List<TechType>>() {/*
	    	{TechType.LimestoneChunk, new TechType[]{SeaToSeaMod.getIngot(TechType.Copper)}},
	    	{TechType.Lithium, new TechType[]{SeaToSeaMod.getIngot(TechType.Lithium)}},
	    	{TechType.Magnetite, new TechType[]{SeaToSeaMod.getIngot(TechType.Magnetite)}},
	    	{TechType.Nickel, new TechType[]{SeaToSeaMod.getIngot(TechType.Nickel)}},
	    	{TechType.ShaleChunk, new TechType[]{SeaToSeaMod.getIngot(TechType.Lithium)}},
	    	{TechType.SandstoneChunk, new TechType[]{SeaToSeaMod.getIngot(TechType.Silver), SeaToSeaMod.getIngot(TechType.Gold), SeaToSeaMod.getIngot(TechType.Lead)}},*/
	    };
	    
	    private FMODAsset unlockSound;
		
		private TechnologyUnlockSystem() {
	    	foreach (TechType tt in SeaToSeaMod.getIngots()) {
	    		TechType[] ingot = SeaToSeaMod.getIngot(tt);
	    		addDirectUnlock(tt, ingot[0]);
	    		addDirectUnlock(tt, ingot[1]);
	    	}
	    	addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType);
	    	addDirectUnlock(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType);
	    	
	    	addDirectUnlock(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, TechType.BaseReinforcement);
	    	
	    	addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, CraftingItems.getItem(CraftingItems.Items.CrystalLens).TechType);
	    	addDirectUnlock(TechType.Glass, CraftingItems.getItem(CraftingItems.Items.BaseGlass).TechType);
		}
	    
	    public void addDirectUnlock(TechType from, TechType to) {
	    	List<TechType> li = directUnlocks.ContainsKey(from) ? directUnlocks[from] : new List<TechType>();
	    	li.Add(to);
	    	directUnlocks[from] = li;
	    }
	    
	    private FMODAsset getUnlockSound() {
	    	if (unlockSound == null) {
	    		foreach (KnownTech.AnalysisTech kt in KnownTech.analysisTech) {
	    			if (kt.unlockMessage == "NotificationBlueprintUnlocked") {
	    				unlockSound = kt.unlockSound;
	    				break;
	    			}
	    		}
	    	}
	    	return unlockSound;
	    }
		
		public void onLogin() {
	    	foreach (TechType kvp in directUnlocks.Keys) {
	    		if (PDAScanner.complete.Contains(kvp)) {
	    			triggerDirectUnlock(kvp);
	    		}
	    	}
		}
	   
		public void triggerDirectUnlock(TechType tt) {
	   		if (!directUnlocks.ContainsKey(tt))
	   			return;
	   		bool any = false;
		   	foreach (TechType unlock in directUnlocks[tt]) {
		   		if (!KnownTech.Contains(unlock)) {
		        	KnownTech.Add(unlock);
		        	any = true;
		    	}
		   	}
	   		if (any) {
		   		SNUtil.log("Triggering direct unlock via "+tt+" of "+directUnlocks[tt].Count+":["+string.Join(", ", directUnlocks[tt].Select<TechType, string>(tc => ""+tc))+"]");
		   		KnownTech.AnalysisTech at = new KnownTech.AnalysisTech();
		   		at.techType = tt;
		   		at.unlockMessage = "NotificationBlueprintUnlocked";
		   		at.unlockSound = getUnlockSound();
		   		uGUI_PopupNotification.main.OnAnalyze(at, true);
	   		}
		}
	}
	
}

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
		
		private TechnologyUnlockSystem() {
	    	foreach (SeaToSeaMod.IngotDefinition tt in SeaToSeaMod.getIngots()) {
	    		addDirectUnlock(tt.material, tt.ingot);
	    		addDirectUnlock(tt.material, tt.unpackingRecipe.TechType);
	    	}
	    	SeaToSeaMod.IngotDefinition qi = SeaToSeaMod.getIngot(TechType.Quartz);
	    	addDirectUnlock(qi.material, SeaToSeaMod.quartzIngotToGlass.TechType);
	    	
	    	addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType);
	    	addDirectUnlock(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType);
	    	
	    	addDirectUnlock(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, TechType.BaseReinforcement);
	    	
	    	addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, CraftingItems.getItem(CraftingItems.Items.CrystalLens).TechType);
	    	addDirectUnlock(TechType.Glass, CraftingItems.getItem(CraftingItems.Items.BaseGlass).TechType);
	    	
	    	addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType, SeaToSeaMod.getAlternateEnzyme().TechType);
	    	
	    	addDirectUnlock(SeaToSeaMod.alkali.TechType, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType);
	    	//addDirectUnlock(SeaToSeaMod.alkali.TechType, CraftingItems.getItem(CraftingItems.Items.SealFabric).TechType);
	    	
	    	addDirectUnlock(SeaToSeaMod.kelp.TechType, CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType);
	    	
	    	addDirectUnlock(TechType.HeatBlade, TechType.HeatBlade);
	    	
	    	addDirectUnlock(SeaToSeaMod.powerSeal.TechType, SeaToSeaMod.powerSeal.TechType);
	    	
	    	//addDirectUnlock(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, SeaToSeaMod.powerSeal.TechType);
	    	//addDirectUnlock(TechType.PrecursorIonPowerCell, SeaToSeaMod.powerSeal.TechType);
	    	
	    	//addDirectUnlock(PDAManager.getPage("lostrivershortcircuit"), SeaToSeaMod.powerSeal.TechType);
		}
	    
	    public void addDirectUnlock(TechType from, TechType to) {
	    	List<TechType> li = directUnlocks.ContainsKey(from) ? directUnlocks[from] : new List<TechType>();
	    	li.Add(to);
	    	directUnlocks[from] = li;
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
		   		SNUtil.triggerTechPopup(tt);
	   		}
		}
	}
	
}

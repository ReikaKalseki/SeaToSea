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
	
	public class C2CUnlocks {
		
		public static readonly C2CUnlocks instance = new C2CUnlocks();
		
		private C2CUnlocks() {
	    	foreach (SeaToSeaMod.IngotDefinition tt in SeaToSeaMod.getIngots()) {
	    		TechnologyUnlockSystem.instance.addDirectUnlock(tt.material, tt.ingot);
	    		TechnologyUnlockSystem.instance.addDirectUnlock(tt.material, tt.unpackingRecipe.TechType);
	    	}
	    	SeaToSeaMod.IngotDefinition qi = SeaToSeaMod.getIngot(TechType.Quartz);
	    	TechnologyUnlockSystem.instance.addDirectUnlock(qi.material, SeaToSeaMod.quartzIngotToGlass.TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType);
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, TechType.BaseReinforcement);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, CraftingItems.getItem(CraftingItems.Items.CrystalLens).TechType);
	    	TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Glass, CraftingItems.getItem(CraftingItems.Items.BaseGlass).TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType, SeaToSeaMod.getAlternateEnzyme().TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(SeaToSeaMod.alkali.TechType, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType);
	    	//addDirectUnlock(SeaToSeaMod.alkali.TechType, CraftingItems.getItem(CraftingItems.Items.SealFabric).TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(SeaToSeaMod.kelp.TechType, CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(TechType.HeatBlade, TechType.HeatBlade);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(SeaToSeaMod.powerSeal.TechType, SeaToSeaMod.powerSeal.TechType);
	    	
	    	//addDirectUnlock(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, SeaToSeaMod.powerSeal.TechType);
	    	//addDirectUnlock(TechType.PrecursorIonPowerCell, SeaToSeaMod.powerSeal.TechType);
	    	
	    	//addDirectUnlock(PDAManager.getPage("lostrivershortcircuit"), SeaToSeaMod.powerSeal.TechType);
		}
	}
	
}

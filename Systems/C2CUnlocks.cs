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
	    	foreach (C2CItems.IngotDefinition tt in C2CItems.getIngots()) {
	    		TechnologyUnlockSystem.instance.addDirectUnlock(tt.material, tt.ingot);
	    		TechnologyUnlockSystem.instance.addDirectUnlock(tt.material, tt.unpackingRecipe.TechType);
	    	}
	    	C2CItems.IngotDefinition qi = C2CItems.getIngot(TechType.Quartz);
	    	TechnologyUnlockSystem.instance.addDirectUnlock(qi.material, C2CRecipes.getQuartzIngotToGlass().TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType);
	    	//TechnologyUnlockSystem.instance.addDirectUnlock(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, TechType.BaseReinforcement);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, CraftingItems.getItem(CraftingItems.Items.CrystalLens).TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType, C2CRecipes.getAlternateEnzyme().TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, Bioprocessor.getRecipeReferenceItem(TechType.SeaTreaderPoop));
	    	
	    	//TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType, Bioprocessor.getRecipeReferenceItem(C2CItems.kelp.seed.TechType));
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(C2CItems.alkali.TechType, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType);
	    	TechnologyUnlockSystem.instance.addDirectUnlock(C2CItems.alkali.seed.TechType, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(C2CItems.kelp.TechType, CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType);
	    	TechnologyUnlockSystem.instance.addDirectUnlock(C2CItems.kelp.seed.TechType, CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(C2CItems.mountainGlow.TechType, C2CRecipes.getAlternateFiber().TechType);
	    	TechnologyUnlockSystem.instance.addDirectUnlock(C2CItems.mountainGlow.seed.TechType, C2CRecipes.getAlternateFiber().TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(TechType.SeaTreaderPoop, CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(TechType.HeatBlade, TechType.HeatBlade);
	    	TechnologyUnlockSystem.instance.addDirectUnlock(TechType.AdvancedWiringKit, TechType.AdvancedWiringKit);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.BrokenT2Battery).TechType, C2CRecipes.getT2BatteryRepair().TechType);
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.BrokenT2Battery).TechType, C2CItems.t2Battery.TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(C2CItems.powerSeal.TechType, C2CItems.powerSeal.TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(TechType.BlueAmoeba, C2CRecipes.getAlternateBacteria().TechType);
	    	
	    	TechnologyUnlockSystem.instance.addDirectUnlock(CraftingItems.getItem(CraftingItems.Items.SulfurAcid).TechType, C2CRecipes.getAltTraceMetal().TechType);
		}
	}
	
}

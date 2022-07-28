using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class LiquidBreathingSystem {
		
		public static readonly LiquidBreathingSystem instance = new LiquidBreathingSystem();
	    
	    private Texture2D baseO2BarTexture;
	    private Color baseO2BarColor;
	    private Texture2D baseO2BubbleTexture;
	    private float baseOverlayAlpha2;
	    private float baseOverlayAlpha1;
	    
	    private float lastRechargeRebreatherTime = -1;
	    private float rechargingTintStrength = 0;
	    
	    private float forceAllowO2 = 0;
		
		private LiquidBreathingSystem() {
			
		}
	    
	    public void onEquip() {
	    	
	    }
	    
	    public void onUnequip() {
	    	Player.main.oxygenMgr.RemoveOxygen(Player.main.oxygenMgr.GetOxygenAvailable()/*-1*/);
	    }
	    
	    public void recharge(Player p, float amt) {
			forceAllowO2 += amt;
			if (amt > 0)
				p.oxygenMgr.AddOxygen(amt);
			onAddO2();
			//SNUtil.writeToChat("Added "+add);
	    }
	    
	    private void onAddO2() {
			lastRechargeRebreatherTime = DayNightCycle.main.timePassedAsFloat;	    		
	    }
	    
	    public float addO2ToPlayer(OxygenManager mgr, float f) {
	    	if (forceAllowO2 > 0) {
	    		float ret = Math.Min(f, forceAllowO2);
				forceAllowO2 = 0;
				onAddO2();
	    		return ret;
	    	}
	    	if (hasLiquidBreathing() && !isLiquidBreathingRechargeable(Player.main)) {
	    		f = 0;
	    	}
	    	return f;
	    }
	    
	    public bool isLiquidBreathingRechargeable(Player p) {
	    	if (p.currentEscapePod == EscapePod.main && Story.StoryGoalManager.main.IsGoalComplete(EscapePod.main.fixPanelGoal.key)) {
				onAddO2();
	    		return true;
	    	}/*
	    	SubRoot sub = p.currentSub;
	    	if (sub != null && sub.powerRelay.IsPowered()) {
	    		RebreatherRechargerSeaBaseLogic fill = sub.gameObject.GetComponent<RebreatherRechargerSeaBaseLogic>();
	    		if (fill != null && fill.consume()) {
	    			return true;
	    		}
	    	}*/
	    	return false;
	    }
	    
	    public bool hasLiquidBreathing() {
	    	return Inventory.main.equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType) != 0;
	    }
	    
	    public void checkLiquidBreathingSupport(OxygenArea a) {
	    	OxygenAreaWithLiquidSupport oxy = a.gameObject.GetComponent<OxygenAreaWithLiquidSupport>();
	    	SNUtil.writeToChat("Check pipe: "+oxy+" > "+(oxy != null ? oxy.supplier+"" : "null"));
	    	if (oxy != null && oxy.supplier != null && DayNightCycle.main.timePassedAsFloat-oxy.lastVerify < 5) {
	    		float need = Math.Min(a.oxygenPerSecond*Time.deltaTime, Player.main.GetOxygenCapacity()-Player.main.GetOxygenAvailable());
	    		if (need > 0) {
		    		float has = oxy.supplier.consume(need);
		    		onAddO2();
		    		SNUtil.writeToChat("Found and use "+has+" of "+oxy.supplier.getFuel());
		    		forceAllowO2 = has;
	    		}
	    	}
	    }
		
		public void updateOxygenGUI(uGUI_OxygenBar gui) {
			uGUI_CircularBar bar = gui.bar;
	    	if (baseO2BarTexture == null) {
	    		baseO2BarTexture = bar.texture;
	    		baseO2BarColor = bar.borderColor;
	    		baseO2BubbleTexture = bar.overlay;
	    		baseOverlayAlpha1 = gui.overlay1Alpha;
	    		baseOverlayAlpha2 = gui.overlay2Alpha;
	    		//RenderUtil.dumpTexture("o2bar_core", baseO2BarTexture);
	    		//RenderUtil.dumpTexture("o2bar_bubble", baseO2BubbleTexture);
	    	}	    	
	    	
	    	bool pink = Inventory.main.equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType) != 0;
	    	
	    	bar.edgeWidth = pink ? 0.25F : 0.2F;
	    	bar.borderWidth = pink ? 0.1F : 0.2F;
	    	bar.borderColor = pink ? new Color(1, 0.6F, 0.82F) : baseO2BarColor;
	    	bar.texture = pink ? TextureManager.getTexture("Textures/o2bar_liquid") : baseO2BarTexture;
	    	bar.overlay = pink ? TextureManager.getTexture("Textures/o2bar_liquid_bubble") : baseO2BubbleTexture;
	    	bar.overlay1Alpha = pink ? Math.Min(1, baseOverlayAlpha1*2) : baseOverlayAlpha1;
	    	bar.overlay2Alpha = pink ? Math.Min(1, baseOverlayAlpha2*2) : baseOverlayAlpha2;
	    	bar.color = Color.white;
	    	
	    	float time = DayNightCycle.main.timePassedAsFloat;
	    	if (time-lastRechargeRebreatherTime <= 0.5) {
	    		rechargingTintStrength = Math.Min(1, rechargingTintStrength*1.01F+0.025F);
	    	}
	    	else {
	    		rechargingTintStrength = Math.Max(0, rechargingTintStrength*0.992F-0.0125F);
	    	}
	    	if (pink && rechargingTintStrength > 0) {
	    		float f = 1-0.33F*(0.5F+rechargingTintStrength*0.5F);
	    		bar.color = new Color(f, f, 1);
	    	}
		}
	    
	    public void applyToBasePipes(RebreatherRechargerLogic machine, Transform seabase) {
	    	foreach (Transform child in seabase) {
	    		IPipeConnection root = child.gameObject.GetComponent<IPipeConnection>();
	    		if (root != null) {
					for (int i = 0; i < OxygenPipe.pipes.Count; i++) {
						OxygenPipe p = OxygenPipe.pipes[i];
						if (p && p.oxygenProvider != null && p.GetRoot() == root && p.oxygenProvider.activeInHierarchy) {
							OxygenAreaWithLiquidSupport oxy = p.oxygenProvider.EnsureComponent<OxygenAreaWithLiquidSupport>();
							oxy.supplier = machine;
							oxy.lastVerify = DayNightCycle.main.timePassedAsFloat;
							//SNUtil.writeToChat("Enable oxy area @ "+oxy.lastVerify);
						}
					}
	    		}
	    	}
	    }
	    
	    class OxygenAreaWithLiquidSupport : MonoBehaviour {
	    	
	    	internal RebreatherRechargerLogic supplier;
	    	internal float lastVerify;
	    	
	    }
		
	}
	
}

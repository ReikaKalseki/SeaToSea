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
	
	public class LiquidBreathingSystem {
		
		public static readonly LiquidBreathingSystem instance = new LiquidBreathingSystem();
		
		internal static readonly float ITEM_VALUE = 30*60; //seconds
		internal static readonly float TANK_CHARGE = 10*60; //how much time you can spend (total) of liquid before returning to a base with a charger
		internal static readonly float TANK_CAPACITY = 2.5F*60; //per "air tank" before you need to go back to a powered air-filled space
		
		private static readonly string customHUDText = "CF<size=30>X</size>•O<size=30>Y</size>";
	    
	    private Texture2D baseO2BarTexture;
	    private Color baseO2BarColor;
	    private Texture2D baseO2BubbleTexture;
	    private float baseOverlayAlpha2;
	    private float baseOverlayAlpha1;
	    private string baseLabel;
	    
	    private float lastRechargeRebreatherTime = -1;
	    private float rechargingTintStrength = 0;
	    
	    private float forceAllowO2 = 0;
		
		private LiquidBreathingSystem() {
			
		}
	    
	    public void onEquip() {
	    	
	    }
	    
	    public void onUnequip() {
	    	Player.main.oxygenMgr.RemoveOxygen(Player.main.oxygenMgr.GetOxygenAvailable()/*-1*/);
	    	SNUtil.playSoundAt(SNUtil.getSound("event:/player/Puke"), Player.main.lastPosition, false, 12);
	    }
	    
	    public void refreshGui() {
	    	lastRechargeRebreatherTime = DayNightCycle.main.timePassedAsFloat;
	    }
	    /*
	    public void refillPlayerO2Bar(Player p, float amt) {
			forceAllowO2 += amt;
			if (amt > 0)
				p.oxygenMgr.AddOxygen(amt);
			onAddO2ToBar();
			//SNUtil.writeToChat("Added "+add);
	    }*/
	    
	    private void onAddO2ToBar() {
				    		
	    }
	    
	    public float getFuelLevel() {
	    	Battery b = getTankBattery();
	    	return b ? b.charge : 0;
	    }
	    
	    public float getAvailableFuelSpace() {
	    	Battery b = getTankBattery();
	    	return b ? b.capacity-b.charge : 0;
	    }
	    
	    private Battery getTankBattery() {
	    	InventoryItem tank = Inventory.main.equipment.GetItemInSlot("Tank");
	    	if (tank.item.GetTechType() != SeaToSeaMod.liquidTank.TechType)
	    		return null;
	    	Battery b = tank.item.gameObject.GetComponent<Battery>();
	    	return b;
	    }
	    
	    public float rechargePlayerLiquidBreathingFuel(float amt) {
	    	Battery b = getTankBattery();
	    	if (!b)
	    		return 0;
	    	float add = Mathf.Min(amt, b.capacity-b.charge);
	    	if (add > 0) {
	    		b.charge += add;
	    		refreshGui();
	    	}
	    	return add;
	    }
	    
	    public bool isInPoweredArea(Player p) {
	    	if (p == null)
	    		return false;
	    	if (p.currentEscapePod == EscapePod.main && Story.StoryGoalManager.main && Story.StoryGoalManager.main.IsGoalComplete(EscapePod.main.fixPanelGoal.key))
	    		return true;
	    	Vehicle v = p.GetVehicle();
	    	if (v && v.IsPowered())
	    		return true;
	    	SubRoot sub = p.currentSub;
	    	if (sub && sub.powerRelay && sub.powerRelay.IsPowered())
	    		return true;
	    	return false;
	    }
	    
	    public bool tryFillPlayerO2Bar(Player p, ref float amt, bool force = false) {
	    	if (hasTankButNoMask()) {
	    		amt = 0;
	    		return false;
	    	}
	    	if (!hasLiquidBreathing())
	    		return true;
	    	if (!force && !isInPoweredArea(p)) {
	    		amt = 0;
	    	    return false;
	    	}
	    	Battery b = getTankBattery();
	    	if (!b) {
	    		amt = 0;
	    		return false;
	    	}
	    	amt = Mathf.Min(amt, b.charge);
	    	if (amt > 0)
	    		b.charge -= amt;
	    	//SNUtil.writeToChat(amt+" > "+b.charge);
	    	return amt > 0;
	    }
	    
	    public bool hasTankButNoMask() {
	    	return Inventory.main.equipment.GetTechTypeInSlot("Head") != SeaToSeaMod.rebreatherV2.TechType && Inventory.main.equipment.GetTechTypeInSlot("Tank") == SeaToSeaMod.liquidTank.TechType;
	    }
	    
	    public bool hasLiquidBreathing() {
	    	return Inventory.main.equipment.GetTechTypeInSlot("Head") == SeaToSeaMod.rebreatherV2.TechType && Inventory.main.equipment.GetTechTypeInSlot("Tank") == SeaToSeaMod.liquidTank.TechType;
	    }
	    
	    public void checkLiquidBreathingSupport(OxygenArea a) {
	    	OxygenAreaWithLiquidSupport oxy = a.gameObject.GetComponent<OxygenAreaWithLiquidSupport>();
	    	//SNUtil.writeToChat("Check pipe: "+oxy+" > "+(oxy != null ? oxy.supplier+"" : "null"));
	    	if (oxy != null && oxy.supplier != null && DayNightCycle.main.timePassedAsFloat-oxy.lastVerify < 5) {
	    		refillFrom(oxy.supplier, Time.deltaTime);
	    	}
	    }
	    
	    public void refillFrom(RebreatherRechargerLogic lgc, float seconds) {
			if (hasLiquidBreathing()) {
				float add = lgc.consume(getAvailableFuelSpace(), seconds);
				float added = rechargePlayerLiquidBreathingFuel(add);
				lgc.refund(add-added); //if somehow added less than space, refund it
			}
	    }
		
		public void updateOxygenGUI(uGUI_OxygenBar gui) {
			uGUI_CircularBar bar = gui.bar;
			Text t = ObjectUtil.getChildObject(gui.gameObject, "OxygenTextLabel").GetComponent<Text>();
			Text tn = ObjectUtil.getChildObject(gui.gameObject, "OxygenTextValue").GetComponent<Text>();
	    	if (baseO2BarTexture == null) {
	    		baseO2BarTexture = bar.texture;
	    		baseO2BarColor = bar.borderColor;
	    		baseO2BubbleTexture = bar.overlay;
	    		baseOverlayAlpha1 = gui.overlay1Alpha;
	    		baseOverlayAlpha2 = gui.overlay2Alpha;
	    		baseLabel = t.text; //O<size=30>2</size>
	    		//RenderUtil.dumpTexture("o2bar_core", baseO2BarTexture);
	    		//RenderUtil.dumpTexture("o2bar_bubble", baseO2BubbleTexture);
	    	}	    	
	    	
			bool pink = hasLiquidBreathing();
	    	
	    	bar.edgeWidth = pink ? 0.25F : 0.2F;
	    	bar.borderWidth = pink ? 0.1F : 0.2F;
	    	bar.borderColor = pink ? new Color(1, 0.6F, 0.82F) : baseO2BarColor;
	    	bar.texture = pink ? TextureManager.getTexture("Textures/HUD/o2bar_liquid") : baseO2BarTexture;
	    	bar.overlay = pink ? TextureManager.getTexture("Textures/HUD/o2bar_liquid_bubble") : baseO2BubbleTexture;
	    	bar.overlay1Alpha = pink ? Math.Min(1, baseOverlayAlpha1*2) : baseOverlayAlpha1;
	    	bar.overlay2Alpha = pink ? Math.Min(1, baseOverlayAlpha2*2) : baseOverlayAlpha2;
	    	t.text = pink ? customHUDText /*"O<size=30>2</size><size=20>(aq)</size>"*/ : baseLabel;
	    	bool pow = isInPoweredArea(Player.main);
	    	tn.color = pink && pow ? Color.gray : Color.white;
	    	if (pink && pow)
	    		tn.text = "-";
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
	    
	    public bool isO2BarFlashingRed() {
	    	return Player.main.GetDepth() >= 400 && EnvironmentalDamageSystem.instance.isPlayerInOcean() && Inventory.main.equipment.GetTechTypeInSlot("Head") != SeaToSeaMod.rebreatherV2.TechType;
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

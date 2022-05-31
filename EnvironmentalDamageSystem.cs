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
	
	public class EnvironmentalDamageSystem {
		
		public static readonly EnvironmentalDamageSystem instance = new EnvironmentalDamageSystem();
    
    	public static readonly float ENVIRO_RATE_SCALAR = 4;
		
		private EnvironmentalDamageSystem() {
			
		}
		
		public void tick(TemperatureDamage dmg) {
	   		//SBUtil.writeToChat("Doing enviro damage on "+dmg+" in "+dmg.gameObject+" = "+dmg.player);
	   		if (dmg.player && (dmg.player.IsInsideWalkable() || !dmg.player.IsSwimming()))
	   			return;
			float temperature = dmg.GetTemperature();
			float f = 1;
			float f0 = 1;
	    	string biome = Player.main.GetBiomeString();
	    	if (dmg.player) {
	    		f0 = Inventory.main.equipment.GetCount(TechType.ReinforcedDiveSuit) == 0 ? 2.5F : 0.4F;
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
		    	float depth = dmg.player.GetDepth();
		    	float ymin = 500;
	    		if (depth > ymin && Inventory.main.equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType) == 0) {
	    			float ymax = 600;
	    			float f2 = depth >= ymax ? 1 : (depth-ymin)/(ymax-ymin);
	    			dmg.liveMixin.TakeDamage(30*0.25F*f2/ENVIRO_RATE_SCALAR, dmg.transform.position, DamageType.Pressure, null);
	    		}
		    	//if (Inventory.main.equipment.GetCount(sealSuit.TechType) == 0) {
		    		//SBUtil.writeToChat(biome+" # "+dmg.gameObject);
		    		float amt = getLRPoison(dmg.player);
		    		if (amt > 0) {
		    			dmg.liveMixin.TakeDamage(amt/ENVIRO_RATE_SCALAR, dmg.transform.position, DamageType.Poison, null);
		    		}
		    	//}
	    	}
		   	float leak = -1;
		   	switch(biome) {
		   		case "LostRiver_BonesField_Corridor":
		   		case "LostRiver_BonesField":
		   		case "LostRiver_Junction":
		   		case "LostRiver_TreeCove":
		   		case "LostRiver_Corridor":
		   		case "LostRiver_GhostTree_Lower":
		   		case "LostRiver_GhostTree":
		   			leak = 1F;
		   			break;
		   		case "LostRiver_Canyon":
		   			leak = 1.75F;
		   			break;
		   		default:
		   			break;
		   	}
		   	if (leak > 0) {
		   		bool used = false;
	    		Vehicle v = dmg.gameObject.GetComponentInParent<Vehicle>();
		    	if (v != null && !v.docked) {
			    	//SBUtil.writeToChat(biome+" # "+dmg.gameObject);
			    	if (v.playerSits)
			    		leak *= 2;
			    	AcidicBrineDamage acid = v.GetComponent<AcidicBrineDamage>();
			    	if (acid != null && acid.numTriggers > 0)
			    		leak *= 8;
			    	int trash;
			    	v.ConsumeEnergy(Math.Min(v.energyInterface.TotalCanProvide(out trash), leak/ENVIRO_RATE_SCALAR));
			    	used = true;
			   	}
	    		foreach (InventoryItem item in Inventory.main.container) {
	    			if (item != null && item.item.GetTechType() != TechType.PrecursorIonPowerCell) {
	    				Battery b = item.item.gameObject.GetComponentInChildren<Battery>();
	    				//SBUtil.writeToChat(item.item.GetTechType()+": "+string.Join(",", (object[])item.item.gameObject.GetComponentsInChildren<MonoBehaviour>()));
	    				if (b != null && b.capacity > 100) {
	    					b.charge = Math.Max(b.charge-leak*0.1F, 0);
	    					//SBUtil.writeToChat("Discharging item "+item.item.GetTechType());
			    			//used = true;
	    				}
	    			}
	    		}
	    		if (used)
		   			PDAManager.getPage("lostrivershortcircuit").unlock();
			}
	 	}
   
		public float getLRPoison(Player p) {
		    switch(p.GetBiomeString()) {
		    	case "LostRiver_BonesField_Corridor":
		    	case "LostRiver_GhostTree":
		    	case "LostRiver_Corridor":
		    		return 8;
		    	case "LostRiver_Canyon":
		    		return 10;
		    	case "LostRiver_BonesField":
		    	case "LostRiver_Junction":
		    	//case "LostRiver_TreeCove":
		    	case "LostRiver_GhostTree_Lower":
		    		return 15;
		    	default:
		    		return 0;
			}
		}
    	
		public float getPlayerO2Rate(Player ep) {
			Player.Mode mode = ep.mode;
			if (mode != Player.Mode.Normal && mode - Player.Mode.Piloting <= 1) {
				return 3f;
			}
			if (Inventory.Get().equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType) > 0) {
				return 3f;
			}
			if (Inventory.Get().equipment.GetCount(TechType.Rebreather) > 0 && ep.GetDepth() < 500) {
				return 3f;
			}
			switch (ep.GetDepthClass()) {
				case Ocean.DepthClass.Safe:
					return 3f;
				case Ocean.DepthClass.Unsafe:
					return 2.25f;
				case Ocean.DepthClass.Crush:
					return 1.5f;
			}
			return 99999f;
		}
	    
	    public float getPlayerO2Use(Player ep, float breathingInterval, int depthClass) {
			if (!GameModeUtils.RequiresOxygen())
				return 0;
			float num = 1;
			if (ep.mode != Player.Mode.Piloting && ep.mode != Player.Mode.LockedPiloting) {
				bool hasRebreatherV2 = Inventory.main.equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType) != 0;
				bool hasRebreather = hasRebreatherV2 || Inventory.main.equipment.GetCount(TechType.Rebreather) != 0;
				if (!hasRebreather) {
					if (depthClass == 2) {
						num = 1.5F;
					}
					else if (depthClass == 3) {
						num = 2;
					}
				}			
				if (depthClass >= 3 && !hasRebreatherV2 && Player.main.GetDepth() >= 500) {
					num = 2.5F+Math.Min(27.5F, (Player.main.GetDepth()-500)/10F);
				}
			}
			return breathingInterval * num;
	    }
	   
		public void tickPlayerEnviroAlerts(RebreatherDepthWarnings warn) {
	   		if (!(warn.alerts[0] is EnviroAlert))
	   			upgradeAlertSystem(warn);
	   		
			if (!Player.main.IsUnderwater()) {
				return;
			}/*
			float depth = Player.main.GetDepth();
			
			bool hasRebreatherV2 = Inventory.main.equipment.GetCount(rebreatherV2.TechType) != 0;
			bool hasRebreatherV1 = Inventory.main.equipment.GetCount(TechType.Rebreather) != 0;
			if (hasRebreatherV2) {
				
			}
			else if (hasRebreatherV1) {
				
			}
			else {*/
				foreach (EnviroAlert ee in warn.alerts) {
	   				//SBUtil.writeToChat(ee+" : "+ee.isActive());
	   				if (!ee.alertCooldown && ee.alert != null && ee.alert.text != null && !ee.wasActiveLastTick && ee.isActive()) {
						ee.alertCooldown = true;
						ee.wasActiveLastTick = true;
						//SBUtil.writeToChat("Firing enviro alert "+ee+" when "+Player.main.GetDepth());
						ee.alert.Play();
						warn.StartCoroutine(warn.ResetAlertCD(ee));
					}
	   				else {
						ee.wasActiveLastTick = false;
	   				}
				}
			//}
			//warn.wasAtDepth = depth;
		}
	   
		private void upgradeAlertSystem(RebreatherDepthWarnings warn) {
	   		List<EnviroAlert> li = new List<EnviroAlert>();
	   		foreach (RebreatherDepthWarnings.DepthAlert a in warn.alerts) {
	   			EnviroAlert e = new EnviroAlert(a);
	   			e.preventiveItem.Add(SeaToSeaMod.rebreatherV2.TechType);
	   			li.Add(e);
	   		}
	   		warn.alerts.Clear();
	   		warn.alerts.AddRange(li);
	   		EnviroAlert ee = new EnviroAlert(warn, 500, "crush pda", "event:/player/story/RadioWarper1");
	   		ee.preventiveItem.Clear();
	   		ee.preventiveItem.Add(SeaToSeaMod.rebreatherV2.TechType);
	   		warn.alerts.Add(ee);
	   		ee = new EnviroAlert(warn, p => getLRPoison(p) > 0, "gas LR", "event:/player/story/Goal_BiomeBloodKelp");
	   		ee.preventiveItem.Clear();
	   		ee.preventiveItem.Add(SeaToSeaMod.sealSuit.TechType);
	   		warn.alerts.Add(ee);
		}
	}
   
	class EnviroAlert : RebreatherDepthWarnings.DepthAlert {
		
   		internal List<TechType> preventiveItem = new List<TechType>(){TechType.Rebreather};
		internal readonly Func<Player, bool> applicability;
   		internal bool wasActiveLastTick = false;
   		
   		internal EnviroAlert(RebreatherDepthWarnings warn, Func<Player, bool> f, string pda, string snd) {
   			alert = warn.gameObject.AddComponent<PDANotification>();
   			alert.text = pda;
   			alert.sound = SBUtil.getSound(snd);
   			applicability = f;
   		}
   	
   		internal EnviroAlert(RebreatherDepthWarnings warn, int depth, string pda, string snd) : this(warn, null, pda, snd) {
   			alertDepth = depth;
	   	}
   	
	   	internal EnviroAlert(RebreatherDepthWarnings.DepthAlert from) {
   			this.alertDepth = from.alertDepth;
   			this.alert = from.alert;
   			this.alertCooldown = from.alertCooldown;
	   	}
   		
   		internal bool isActive() {
   			Player p = Player.main;
   			bool valid = applicability != null ? applicability(p) : p.GetDepth() >= alertDepth;
   			if (!valid)
   				return false;
   			foreach (TechType prevent in preventiveItem) {
   				if (Inventory.main.equipment.GetCount(prevent) != 0)
   					return false;
   			}
   			return true;
   		}
   		
		public override string ToString() {
			return this.alert.text+" @ "+this.alertDepth;
		}
   	
	}
	
}

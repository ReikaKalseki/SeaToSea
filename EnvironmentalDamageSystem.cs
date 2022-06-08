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
    	
    	private readonly Dictionary<string, TemperatureEnvironment> temperatures = new Dictionary<string, TemperatureEnvironment>();
    	private readonly Dictionary<string, float> lrPoisonDamage = new Dictionary<string, float>();
    	private readonly Dictionary<string, float> lrLeakage = new Dictionary<string, float>();
    	
    	private readonly Vector3 lavaCastleCenter = Vector3.zero; //TODO
    	private readonly double lavaCastleRadius = 0; //TODO
		
		private EnvironmentalDamageSystem() {
    		temperatures["ILZCorridor"] = new TemperatureEnvironment(90, 8, 40, 9);
    		temperatures["ILZCorridorDeep"] = new TemperatureEnvironment(120, 9, 20, 12);
    		temperatures["ILZChamber"] = new TemperatureEnvironment(150, 10, 10, 15);
    		temperatures["LavaPit"] = new TemperatureEnvironment(180, 12, 8, 20);
    		temperatures["LavaFalls"] = new TemperatureEnvironment(200, 15, 5, 25);
    		temperatures["LavaLakes"] = new TemperatureEnvironment(250, 18, 2, 40);
    		temperatures["ilzLava"] = new TemperatureEnvironment(1200, 24, 0, 100); //in lava
    		temperatures["LavaCastle"] = new TemperatureEnvironment(360, 18, 4, 20); //TODO detect lava castle
    		temperatures["ILZChamber_Dragon"] = temperatures["ILZChamber"];
    		
    		lrLeakage["LostRiver_BonesField_Corridor"] = 1;
		   	lrLeakage["LostRiver_BonesField"] = 1;
		   	lrLeakage["LostRiver_Junction"] = 1;
		   	lrLeakage["LostRiver_TreeCove"] = 1;
		   	lrLeakage["LostRiver_Corridor"] = 1;
		   	lrLeakage["LostRiver_GhostTree_Lower"] = 1;
		   	lrLeakage["LostRiver_GhostTree"] = 1;
		   	lrLeakage["LostRiver_Canyon"] = 1.75F;
		   	
		   	lrPoisonDamage["LostRiver_BonesField_Corridor"] = 8;
		    lrPoisonDamage["LostRiver_GhostTree"] = 8;
		    lrPoisonDamage["LostRiver_Corridor"] = 8;
		    lrPoisonDamage["LostRiver_Canyon"] = 10;
		    lrPoisonDamage["LostRiver_BonesField"] = 15;
		    lrPoisonDamage["LostRiver_Junction"] = 15;
		    lrPoisonDamage["LostRiver_GhostTree_Lower"] = 15;
		}
		
		public void tickTemperatureDamages(TemperatureDamage dmg) {
	   		//SBUtil.writeToChat("Doing enviro damage on "+dmg+" in "+dmg.gameObject+" = "+dmg.player);
	   		if (dmg.player && (dmg.player.IsInsideWalkable() || !dmg.player.IsSwimming()))
	   			return;
	   		//SBUtil.writeToChat("not skipped");
			float temperature = dmg.GetTemperature();
			float f = 1;
			float f0 = 1;
			string biome = getBiome(dmg.gameObject);//Player.main.GetBiomeString();
	    	if (dmg.player) {
	    		f0 = Inventory.main.equipment.GetCount(TechType.ReinforcedDiveSuit) == 0 ? 2.5F : 0.4F;
	    		TemperatureEnvironment te = temperatures.ContainsKey(biome) ? temperatures[biome] : null;
	    		if (te != null) {
		    		f = te.damageScalar;
		    		temperature = te.temperature;
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
		    		float amt = getLRPoison(biome);
		    		if (amt > 0) {
		    			dmg.liveMixin.TakeDamage(amt/ENVIRO_RATE_SCALAR, dmg.transform.position, DamageType.Poison, null);
		    		}
		    	//}
	    	}
			float leak = getLRPowerLeakage(biome);
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
    	
    	public void tickCyclopsDamage(CrushDamage dmg) {
			if (!dmg.gameObject.activeInHierarchy || !dmg.enabled) {
				return;
			}
			if (dmg.GetCanTakeCrushDamage() && dmg.GetDepth() > dmg.crushDepth) {
				dmg.liveMixin.TakeDamage(dmg.damagePerCrush, dmg.transform.position, DamageType.Pressure, null);
				if (dmg.soundOnDamage) {
					dmg.soundOnDamage.Play();
				}
			}
	    	SubRoot sub = dmg.gameObject.GetComponentInParent<SubRoot>();
	    	if (sub != null && sub.isCyclops) {
	    		TemperatureEnvironment temp = getLavaHeatDamage(dmg.gameObject);
	    		//SBUtil.writeToChat("heat: "+temp);
	    		if (temp != null) {
	    			Equipment modules = sub.upgradeConsole != null ? sub.upgradeConsole.modules : null;
	    			bool immune = false;
	    			if (modules != null) {
		    			foreach (string slot in SubRoot.slotNames) {
							TechType techTypeInSlot = modules.GetTechTypeInSlot(slot);
							if (techTypeInSlot == SeaToSeaMod.cyclopsHeat.TechType)
								immune = true;
						}
	    			}
	    			//SBUtil.writeToChat("immune: "+immune);
	    			if (!immune) {
						dmg.liveMixin.TakeDamage(dmg.damagePerCrush*temp.damageScalar*0.15F, dmg.transform.position, DamageType.Heat, null);
						if (dmg.soundOnDamage) {
							dmg.soundOnDamage.Play();
						}
						if (temp.cyclopsFireChance <= 0 || UnityEngine.Random.Range(0, temp.cyclopsFireChance) == 0) {
							CyclopsRooms key = (CyclopsRooms)UnityEngine.Random.Range(0, Enum.GetNames(typeof(CyclopsRooms)).Length);
							SubFire fire = dmg.gameObject.GetComponentInParent<SubFire>();
							fire.CreateFire(fire.roomFires[key]);
						}
	    			}
	    		}
		    	float leak = getLRPowerLeakage(dmg.gameObject);
		    	//SBUtil.writeToChat("leak "+leak);
			   	if (leak > 0) {
	    			SubControl con = dmg.gameObject.GetComponentInParent<SubControl>();
				    if (con.cyclopsMotorMode.engineOn)
				    	leak *= 1.25F;
				    if (con.appliedThrottle)
				    	leak *= 1.5F;
				    float trash;
				    sub.powerRelay.ConsumeEnergy(leak*2.5F, out trash);
				}
	    	}
    	}
    	
    	public TemperatureEnvironment getLavaHeatDamage(GameObject go) {
    		return getLavaHeatDamage(getBiome(go));//p.GetBiomeString());
    	}
   
		public TemperatureEnvironment getLavaHeatDamage(string biome) {
			return biome != null && temperatures.ContainsKey(biome) ? temperatures[biome] : null;
		}
    	
    	public float getLRPoison(GameObject go) {
    		return getLRPoison(getBiome(go));//p.GetBiomeString());
    	}
   
		public float getLRPoison(string biome) {
			return biome != null && lrPoisonDamage.ContainsKey(biome) ? lrPoisonDamage[biome] : 0;
		}
    	
    	public float getLRPowerLeakage(GameObject go) {
    		return getLRPowerLeakage(getBiome(go));//p.GetBiomeString());
    	}
   
		public float getLRPowerLeakage(string biome) {
    		return biome != null && lrLeakage.ContainsKey(biome) ? lrLeakage[biome] : -1;
		}
    	
    	public string getBiome(GameObject go) {
    		return getBiome(go.transform.position);
    	}
    	
    	public string getBiome(Vector3 pos) {
    		string ret = LargeWorld.main.GetBiome(pos);
    		if (ret == "ILZCorridor" && pos.y < -1175)
    			ret = "ILZCorridorDeep";
    		if (ret == "ILZChamber" && Vector3.Distance(lavaCastleCenter, pos) <= lavaCastleRadius)
    			ret = "LavaCastle";
    		return ret;
    	}
    	
    	public float getWaterTemperature(Vector3 pos) {
    		string biome = getBiome(pos);
    		TemperatureEnvironment temp = getLavaHeatDamage(biome);
    		float ret = temp != null ? temp.temperature : -1000;
    		if (biome == "ILZCorridor" && pos.y <= -1100 && pos.y >= -1175) {
    			ret = (float)MathUtil.linterpolate(-pos.y, 1100, 1175, temperatures["ILZCorridor"].temperature, temperatures["ILZCorridorDeep"].temperature);
    		}
    		return ret;
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
	   		ee = new EnviroAlert(warn, p => getLRPoison(p.gameObject) > 0, "gas LR", "event:/player/story/Goal_BiomeBloodKelp");
	   		ee.preventiveItem.Clear();
	   		ee.preventiveItem.Add(SeaToSeaMod.sealSuit.TechType);
	   		warn.alerts.Add(ee);
		}
	}
	
	public class TemperatureEnvironment {
		
		public readonly float temperature;
		public readonly float damageScalar;
		public readonly float damageScalarCyclops;
		public readonly int cyclopsFireChance;
		
		internal TemperatureEnvironment(float t, float dmg, int cf, float dmgC) {
			temperature = t;
			damageScalar = dmg;
			cyclopsFireChance = cf;
			damageScalarCyclops = dmgC;
		}
		
		public override string ToString()
		{
			return string.Format("[TemperatureEnvironment Temperature={0}, DamageScalar={1}, CyclopsFireChance={2}]", temperature, damageScalar, cyclopsFireChance);
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

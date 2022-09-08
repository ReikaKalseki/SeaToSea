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
    	
    	private readonly static Vector3 lavaCastleCenter = new Vector3(-49, -1242, 118);
    	private readonly static double lavaCastleRadius = Vector3.Distance(new Vector3(-116, -1194, 126), lavaCastleCenter)+32;
    	
    	private readonly static Vector3 auroraPrawnBayCenter = new Vector3(996, 2.5F, -26.5F);//new Vector3(1003, 4, -18);
    	//private readonly static Vector3 auroraTopLeftBack = new Vector3(1010, 13, ?);
    	//private readonly static Vector3 auroraBottomRightFront = new Vector3(?, ?, ?);
    	private readonly static Vector3 auroraFireCeilingTunnel = new Vector3(1047.3F, 1, 2);
    	private readonly static Vector3 auroraPrawnBayDoor = new Vector3(984, 8.5F, -36.2F);
    	
    	internal readonly static float depthFXRippleStart = 450;
    	public readonly static float depthDamageStart = 500;
    	public readonly static float depthDamageMax = 600;
    	
    	private readonly Dictionary<string, TemperatureEnvironment> temperatures = new Dictionary<string, TemperatureEnvironment>();
    	private readonly Dictionary<string, float> lrPoisonDamage = new Dictionary<string, float>();
    	private readonly Dictionary<string, float> lrLeakage = new Dictionary<string, float>();
    	
    	//private DepthRippleFX depthWarningFX1;
    	//private DepthDarkeningFX depthWarningFX2;
		
		private EnvironmentalDamageSystem() {
    		temperatures["ILZCorridor"] = new TemperatureEnvironment(90, 8, 0.5F, 40, 9);
    		temperatures["ILZCorridorDeep"] = new TemperatureEnvironment(120, 9, 0.5F, 20, 12);
    		temperatures["ILZChamber"] = new TemperatureEnvironment(150, 10, 0.5F, 10, 15);
    		temperatures["LavaPit"] = new TemperatureEnvironment(180, 12, 0.5F, 8, 20);
    		temperatures["LavaFalls"] = new TemperatureEnvironment(200, 15, 0.5F, 5, 25);
    		temperatures["LavaLakes"] = new TemperatureEnvironment(250, 18, 0.5F, 2, 40);
    		temperatures["ilzLava"] = new TemperatureEnvironment(1200, 24, 0.5F, 0, 100); //in lava
    		temperatures["LavaCastle"] = new TemperatureEnvironment(360, 18, 0.5F, 4, 20);
    		temperatures["ILZChamber_Dragon"] = temperatures["ILZChamber"];
    		
    		temperatures["AuroraPrawnBay"] = new TemperatureEnvironment(150, 15F, 2.5F, 9999, 0);
    		temperatures["AuroraPrawnBayDoor"] = new TemperatureEnvironment(200, 40F, 2.5F, 9999, 0);
    		temperatures["AuroraFireCeilingTunnel"] = new TemperatureEnvironment(175, 5F, 1.5F, 9999, 0);
    		
    		lrLeakage["LostRiver_BonesField_Corridor"] = 1;
		   	lrLeakage["LostRiver_BonesField"] = 1;
		   	lrLeakage["LostRiver_Junction"] = 1;
		   	lrLeakage["LostRiver_TreeCove"] = 0.9F;
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
    	
    	private bool isPlayerInOcean() {
    		Player ep = Player.main;
    		string biome = getBiome(ep.gameObject);
    		return !ep.IsInsideWalkable() && Player.main.IsUnderwater() && ep.IsSwimming() && !ep.currentWaterPark && !biome.ToLowerInvariant().Contains("prison") && !biome.ToLowerInvariant().Contains("precursor") && !Creature.prisonAquriumBounds.Contains(ep.transform.position);
    	}
		
		public void tickTemperatureDamages(TemperatureDamage dmg) {
    		//depthWarningFX1 = Camera.main.gameObject.EnsureComponent<DepthRippleFX>();
    		//depthWarningFX2 = Camera.main.gameObject.EnsureComponent<DepthDarkeningFX>();
	   		//SBUtil.writeToChat("Doing enviro damage on "+dmg+" in "+dmg.gameObject+" = "+dmg.player);
			string biome = getBiome(dmg.gameObject);//Player.main.GetBiomeString();
			bool aurora = biome == "AuroraPrawnBay" || biome == "AuroraPrawnBayDoor" || biome == "AuroraFireCeilingTunnel";
			bool diveSuit = dmg.player && dmg.player.HasReinforcedGloves() && dmg.player.HasReinforcedSuit();
			if (aurora && !diveSuit) {
	    		PDAMessages.trigger(PDAMessages.Messages.AuroraFireWarn);
			}
			if (dmg.player && !isPlayerInOcean() && !aurora)
	   			return;
	   		//SBUtil.writeToChat("not skipped");
			float temperature = dmg.GetTemperature();
			float f = 1;
			float f0 = 1;
			float fw = 0;
			SNUtil.writeToChat(biome+" for "+dmg.gameObject);
	    	if (dmg.player) {
	    		f0 = !diveSuit ? 2.5F : 0.4F;
	    		TemperatureEnvironment te = temperatures.ContainsKey(biome) ? temperatures[biome] : null;
	    		if (te != null) {
		    		f = te.damageScalar;
		    		temperature = te.temperature;
		    		fw = te.waterScalar;
	    		}
	    		float baseVal = 49;
	    		if (dmg.minDamageTemperature > baseVal && dmg.minDamageTemperature <= 75) { //stop repeating forever
		    		float add = dmg.minDamageTemperature-baseVal; //how much above default it is
		    		dmg.minDamageTemperature = baseVal+add*2;
	    		}
			}
			if (temperature >= dmg.minDamageTemperature) {
				float num = temperature / dmg.minDamageTemperature;
				num *= dmg.baseDamagePerSecond;
				float amt = num*f*f0/ENVIRO_RATE_SCALAR;
				if (aurora && biome == "AuroraPrawnBay" && Vector3.Distance(auroraPrawnBayDoor, dmg.player.gameObject.transform.position) <= 3)
					amt *= 2;
				if (aurora && diveSuit)
					amt = 0;
				//SNUtil.writeToChat(biome+" > "+temperature+" / "+dmg.minDamageTemperature+" > "+amt);
				if (amt > 0) {
					dmg.liveMixin.TakeDamage(amt, dmg.transform.position, DamageType.Heat, null);
					if (dmg.player && !diveSuit) {
						Survival s = Player.main.GetComponent<Survival>();
						s.water = Mathf.Clamp(s.water-amt*fw, 0f, 100f);
					}
				}
			}
	    	if (dmg.player) {
		    	float depth = dmg.player.GetDepth();
		    	bool rb = Inventory.main.equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType) != 0;
		    	//depthWarningFX1.setIntensities(rb ? 0 : depth);
		    	//depthWarningFX2.setIntensities(rb ? 0 : depth);
	    		if (depth > depthDamageStart && !rb) {
	    			float f2 = depth >= depthDamageMax ? 1 : (float)MathUtil.linterpolate(depth, depthDamageStart, depthDamageMax, 0, 1);
	    			dmg.liveMixin.TakeDamage(30*0.25F*f2/ENVIRO_RATE_SCALAR, dmg.transform.position, DamageType.Pressure, null);
	    		}
		    	if (Inventory.main.equipment.GetCount(SeaToSeaMod.sealSuit.TechType) == 0 && Inventory.main.equipment.GetCount(TechType.ReinforcedDiveSuit) == 0) {
		    		//SBUtil.writeToChat(biome+" # "+dmg.gameObject);
		    		float amt = getLRPoison(biome);
		    		if (amt > 0) {
		    			dmg.liveMixin.TakeDamage(amt/ENVIRO_RATE_SCALAR, dmg.transform.position, DamageType.Poison, null);
		    		}
		    	}
	    	}
			float leak = getLRPowerLeakage(biome)*0.9F;
		   	if (leak > 0) {
		   		bool used = false;
	    		Vehicle v = dmg.gameObject.GetComponentInParent<Vehicle>();
		    	if (v != null && !v.docked) {
	    			if (v is SeaMoth || true) {
			    		foreach (int idx in v.slotIndexes.Values) {
			    			InventoryItem ii = v.GetSlotItem(idx);
			    			if (ii != null && ii.item.GetTechType() == SeaToSeaMod.powerSeal.TechType) {
			    				leak *= 0.2F;
			    				break;
			    			}
			    		}
	    			}
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
	    			if (item != null && item.item.GetTechType() != TechType.PrecursorIonPowerCell && item.item.GetTechType() != TechType.PrecursorIonBattery) {
	    				Battery b = item.item.gameObject.GetComponentInChildren<Battery>();
	    				//SBUtil.writeToChat(item.item.GetTechType()+": "+string.Join(",", (object[])item.item.gameObject.GetComponentsInChildren<MonoBehaviour>()));
	    				if (b != null && b.capacity > 100) {
	    					b.charge = Math.Max(b.charge-leak*0.1F, 0);
	    					//SBUtil.writeToChat("Discharging item "+item.item.GetTechType());
			    			//used = true;
	    				}
	    			}
	    		}
	    		if (used) {
		   			PDAManager.getPage("lostrivershortcircuit").unlock();
			   		if (!KnownTech.Contains(SeaToSeaMod.powerSeal.TechType)) {
			        	KnownTech.Add(SeaToSeaMod.powerSeal.TechType);
			    	}
	    		}
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
    		if (string.IsNullOrEmpty(ret))
    			ret = "void";
    		if (Vector3.Distance(auroraPrawnBayCenter, pos) <= 20)
    			ret = "AuroraPrawnBay";
    		if (Vector3.Distance(auroraPrawnBayDoor, pos) <= 3)
    			ret = "AuroraPrawnBayDoor";
    		if (Vector3.Distance(auroraFireCeilingTunnel, pos) <= 9)
    			ret = "AuroraFireCeilingTunnel";
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
				if (depthClass >= 3 && !hasRebreatherV2 && Player.main.GetDepth() >= 400 && isPlayerInOcean()) {
					num = 2.5F+Math.Min(27.5F, (Player.main.GetDepth()-400)/10F);
				}
			}
			return breathingInterval * num;
	    }
	   
		public void tickPlayerEnviroAlerts(RebreatherDepthWarnings warn) {
	   		if (!(warn.alerts[0] is EnviroAlert))
	   			upgradeAlertSystem(warn);
	   		
	   		if (!isPlayerInOcean()/* || Player.main.liveMixin == null || !Player.main.liveMixin.enabled || Player.main.liveMixin.invincible*/) {
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
	   		EnviroAlert ee = new EnviroAlert(warn, 500, SeaToSeaMod.miscLocale.getEntry("deepair"));
	   		ee.preventiveItem.Clear();
	   		ee.preventiveItem.Add(SeaToSeaMod.rebreatherV2.TechType);
	   		warn.alerts.Add(ee);
	   		ee = new EnviroAlert(warn, p => getLRPoison(p.gameObject) > 0, SeaToSeaMod.miscLocale.getEntry("lrpoison"));
	   		ee.preventiveItem.Clear();
	   		ee.preventiveItem.Add(SeaToSeaMod.sealSuit.TechType);
	   		ee.preventiveItem.Add(TechType.ReinforcedDiveSuit);
	   		warn.alerts.Add(ee);
		}
	}
	
	abstract class DepthFX : MonoBehaviour {
		
		protected float strength = 0;
		protected float lastUpdate = 0;
		
		private void OnPreRender() {
			if (DayNightCycle.main.timePassedAsFloat-lastUpdate >= 3) {
				strength *= 0.95F;
			}
	    	enabled = strength > 0.01;
		}
		
		internal void setIntensities(float depth) {
			lastUpdate = DayNightCycle.main.timePassedAsFloat;
			strength = calculateIntensity(depth);
	    	enabled = strength > 0.01;
		}
		
		protected abstract float calculateIntensity(float depth);
	}
	
	class DepthRippleFX : DepthFX {
		
		protected override float calculateIntensity(float depth) {
	    	return (float)MathUtil.linterpolate(depth, EnvironmentalDamageSystem.depthFXRippleStart, EnvironmentalDamageSystem.depthDamageStart, 0, 1, true);
		}
	
		private void OnRenderImage(RenderTexture source, RenderTexture destination) {			
			Material mat = gameObject.GetComponent<MesmerizedScreenFX>().mat;
			mat.SetFloat(ShaderPropertyID._Amount, strength);
			mat.SetColor(ShaderPropertyID._ColorStrength, new Color(0, 0, 0, strength));
			Graphics.Blit(source, destination, mat);
		}
	}
	
	class DepthDarkeningFX : DepthFX {
		
		protected override float calculateIntensity(float depth) {
	    	return (float)MathUtil.linterpolate(depth, EnvironmentalDamageSystem.depthDamageStart, EnvironmentalDamageSystem.depthDamageMax, 0, 1, true);
		}
	
		private void OnRenderImage(RenderTexture source, RenderTexture destination) {/*
			//if (!intermediaryTexture || !intermediaryTexture.IsCreated() || intermediaryTexture.height != source.height || intermediaryTexture.width != source.width)
			//	intermediaryTexture = new RenderTexture(destination ? destination.descriptor : source.descriptor);
			RenderTexture intermediaryTexture = RenderTexture.GetTemporary(destination ? destination.descriptor : source.descriptor);
			Material mat1 = gameObject.GetComponent<MesmerizedScreenFX>().mat;
			Material mat2 = gameObject.GetComponent<CyclopsSmokeScreenFX>().mat;
			mat1.SetFloat(ShaderPropertyID._Amount, rippleStrength);
			//mat1.color = new Color(0, 0, 0, 1);
			mat1.SetColor(ShaderPropertyID._ColorStrength, new Color(0, 0, 0, 1));
			mat2.color = new Color(0, 0, 0, darkening*0.95F);
			Graphics.Blit(source, intermediaryTexture, mat1);
			Graphics.Blit(intermediaryTexture, destination, mat2);
			intermediaryTexture.Release();*/
			//Graphics.Blit(source, destination);
			
			Material mat = gameObject.GetComponent<CyclopsSmokeScreenFX>().mat;
			mat.SetFloat(ShaderPropertyID._Amount, strength);
			mat.color = new Color(0, 0, 0, strength*1F);
			Graphics.Blit(source, destination, mat);
		}
	}
	
	public class TemperatureEnvironment {
		
		public readonly float temperature;
		public readonly float damageScalar;
		public readonly float waterScalar;
		public readonly float damageScalarCyclops;
		public readonly int cyclopsFireChance;
		
		internal TemperatureEnvironment(float t, float dmg, float w, int cf, float dmgC) {
			temperature = t;
			damageScalar = dmg;
			waterScalar = w;
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
   		
   		internal EnviroAlert(RebreatherDepthWarnings warn, Func<Player, bool> f, string pda, FMODAsset snd) {
   			alert = warn.gameObject.AddComponent<PDANotification>();
   			alert.text = pda;
   			alert.sound = snd;
   			applicability = f;
   		}
   		
   		internal EnviroAlert(RebreatherDepthWarnings warn, Func<Player, bool> f, XMLLocale.LocaleEntry e) : this(warn, f, e.desc, SoundManager.registerSound("enviroAlert_"+e.key, e.pda, SoundSystem.voiceBus)) {
   			
   		}
   		
   		internal EnviroAlert(RebreatherDepthWarnings warn, int depth, XMLLocale.LocaleEntry e) : this(warn, depth, e.desc, SoundManager.registerSound("enviroAlert_"+e.key, e.pda, SoundSystem.voiceBus)) {
   			
   		}
   	
   		internal EnviroAlert(RebreatherDepthWarnings warn, int depth, string pda, FMODAsset snd) : this(warn, null, pda, snd) {
   			alertDepth = depth;
	   	}
   	
	   	internal EnviroAlert(RebreatherDepthWarnings.DepthAlert from) {
   			this.alertDepth = from.alertDepth;
   			this.alert = from.alert;
   			this.alertCooldown = from.alertCooldown;
	   	}
   		
   		internal bool isActive() {
   			Player p = Player.main;
   			bool valid = applicability != null ? applicability(p) : p.GetDepth() >= alertDepth && GameModeUtils.RequiresOxygen();
   			if (!valid)
   				return false;
   			foreach (TechType prevent in preventiveItem) {
   				if (Inventory.main.equipment.GetCount(prevent) != 0)
   					return false;
   			}
   			return true;
   		}
   		
		public override string ToString() {
			return alert.text+" @ "+alertDepth+"/"+applicability;
		}
   	
	}
	
}

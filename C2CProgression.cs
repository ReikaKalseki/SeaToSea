using System;
using System.Collections.Generic;
using System.Linq;

using Story;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using UnityEngine;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea
{
	public class C2CProgression : IStoryGoalListener {
		
		public static readonly C2CProgression instance = new C2CProgression();
		
	    private readonly Vector3 pod12Location = new Vector3(1117, -268, 568);
	    private readonly Vector3 pod3Location = new Vector3(-33, -23, 409);
	    private readonly Vector3 pod6Location = new Vector3(363, -110, 309);
	    private readonly Vector3 dronePDACaveEntrance = new Vector3(-80, -79, 262);
	    
	    private readonly Vector3[] seacrownCaveEntrances = new Vector3[]{
	    	new Vector3(279, -140, 288),//new Vector3(300, -120, 288)/**0.67F+pod6Location*0.33F*/,
	    	//new Vector3(66, -100, -608), big obvious but empty one
	    	new Vector3(-621, -130, -190),//new Vector3(-672, -100, -176),
	    	//new Vector3(-502, -80, -102), //empty in vanilla, and right by pod 17
	    };
	    
	    private float lastDunesEntry = -1;
		
		private C2CProgression() {
	    	StoryHandler.instance.addListener(this);
	    	
			StoryHandler.instance.registerTrigger(new StoryTrigger("AuroraRadiationFixed"), new DelayedProgressionEffect(VoidSpikesBiome.instance.fireRadio, VoidSpikesBiome.instance.isRadioFired, 0.00003F));
			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.PrecursorKey_Orange), new DelayedStoryEffect(SeaToSeaMod.crashMesaRadio, 0.00004F));
			StoryHandler.instance.registerTrigger(new ProgressionTrigger(ep => ep.GetVehicle() is SeaMoth), new DelayedProgressionEffect(SeaToSeaMod.treaderSignal.fireRadio, SeaToSeaMod.treaderSignal.isRadioFired, 0.000018F));
			
			
			StoryGoal pod12Radio = new StoryGoal("RadioKoosh26", Story.GoalType.Radio, 0);
			DelayedStoryEffect ds = new DelayedStoryEffect(pod12Radio, 0.00008F);
			StoryHandler.instance.registerTrigger(new StoryTrigger("SunbeamCheckPlayerRange"), ds);
			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.BaseNuclearReactor), ds);
			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.HighCapacityTank), ds);
			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.PrecursorKey_Purple), ds);
			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.BaseUpgradeConsole), ds);
			StoryHandler.instance.registerTrigger(new TechTrigger(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType), ds);
			StoryHandler.instance.registerTrigger(new EncylopediaTrigger("SnakeMushroom"), ds);
			
			addPDAPrompt(PDAMessages.Messages.KooshCavePrompt, ep => Vector3.Distance(pod12Location, ep.transform.position) <= 75);
			addPDAPrompt(PDAMessages.Messages.RedGrassCavePrompt, isNearSeacrownCave);
			addPDAPrompt(PDAMessages.Messages.UnderwaterIslandsPrompt, isInUnderwaterIslands);
			PDAPrompt kelp = addPDAPrompt(PDAMessages.Messages.KelpCavePrompt, ep => MathUtil.isPointInCylinder(dronePDACaveEntrance.setY(-40), ep.transform.position, 60, 40) || (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.FollowRadioPrompt).key) && Vector3.Distance(pod3Location, ep.transform.position) <= 60));
			/*
			PDAPrompt kelpLate = addPDAPrompt(PDAMessages.Messages.KelpCavePromptLate, new TechTrigger(TechType.HighCapacityTank), 0.0001F);
			addPDAPrompt(kelpLate, new TechTrigger(TechType.StasisRifle));
			addPDAPrompt(kelpLate, new TechTrigger(TechType.BaseMoonpool));
			*/
			StoryHandler.instance.registerTrigger(new PDAPromptCondition(new ProgressionTrigger(doDunesCheck)), new DunesPrompt());
			
			addPDAPrompt(PDAMessages.Messages.FollowRadioPrompt, hasMissedRadioSignals);
		}
	    
	    private bool hasMissedRadioSignals(Player ep) {
	    	bool late = KnownTech.knownTech.Contains(TechType.StasisRifle) || KnownTech.knownTech.Contains(TechType.BaseMoonpool) || KnownTech.knownTech.Contains(TechType.HighCapacityTank);
	    	bool all = PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.RedGrassCavePrompt).key) && PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePrompt).key) && PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KooshCavePrompt).key);
	    	return late && !all;
	    }
	    
	    private bool isInUnderwaterIslands(Player ep) {
	    	return ep.transform.position.y <= -150 && (ep.transform.position-new Vector3(-112.3F, ep.transform.position.y, 990.3F)).magnitude <= 180 && ep.GetBiomeString().ToLowerInvariant().Contains("underwaterislands");
	    }
	    
	    private bool isNearSeacrownCave(Player ep) {
	    	Vector3 pos = ep.transform.position;
		    foreach (Vector3 vec in seacrownCaveEntrances) {
				if (pos.y <= vec.y && MathUtil.isPointInCylinder(vec, pos, 30, 10)) {
	    			return true;
			    }
			}
	    	return false;
	    }
	    
	    private PDAPrompt addPDAPrompt(PDAMessages.Messages m, Func<Player, bool> condition, float ch = 0.01F) {
	    	return addPDAPrompt(m, new ProgressionTrigger(condition), ch);
	    }
	    
	    private PDAPrompt addPDAPrompt(PDAMessages.Messages m, ProgressionTrigger pt, float ch = 0.01F) {
	    	PDAPrompt p = new PDAPrompt(m, ch);
	    	addPDAPrompt(p, pt);
	    	return p;
	    }
	    
	    private void addPDAPrompt(PDAPrompt m, ProgressionTrigger pt) {
	    	StoryHandler.instance.registerTrigger(new PDAPromptCondition(pt), m);
	    }
	    
	    private bool doDunesCheck(Player ep) {
	    	if (ep.GetBiomeString() != null && ep.GetBiomeString().ToLowerInvariant().Contains("dunes")) {
	    		float time = DayNightCycle.main.timePassedAsFloat;
	    		if (lastDunesEntry < 0)
	    			lastDunesEntry = time;
	    		//SNUtil.writeToChat(lastDunesEntry+" > "+(time-lastDunesEntry));
	    		if (time-lastDunesEntry >= 90) { //in dunes for at least 90s
	    			return true;
	    		}
	    	}
	    	else {
	    		lastDunesEntry = -1;
	    	}
	    		return false;
	    }
		
		public void NotifyGoalComplete(string key) {
			if (key.StartsWith("OnPlay", StringComparison.InvariantCultureIgnoreCase)) {
				if (key.Contains(SeaToSeaMod.treaderSignal.storyGate)) {
					SeaToSeaMod.treaderSignal.activate(20);
				}
				else if (key.Contains(VoidSpikesBiome.instance.getSignalKey())) {
					VoidSpikesBiome.instance.activateSignal();
				}
				else if (key.Contains(SeaToSeaMod.crashMesaRadio.key)) {
					Player.main.gameObject.EnsureComponent<CrashMesaCallback>().Invoke("trigger", 25);
				}
			}
			else if (key == PDAManager.getPage("voidpod").id) { //id is pda page story key
				SeaToSeaMod.voidSpikeDirectionHint.activate(4);
			}
			else {
				switch(key) {
					case "SunbeamCheckPlayerRange":
						Player.main.gameObject.EnsureComponent<AvoliteSpawner.TriggerCallback>().Invoke("trigger", 39);
					break;
					case "drfwarperheat":
						KnownTech.Add(SeaToSeaMod.cyclopsHeat.TechType);
					break;
				}
			}
		}
	
		internal bool canTriggerPDAPrompt(Player ep) {
	    	return SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.PROMPTS) && (ep.IsSwimming() || ep.GetVehicle() != null) && ep.currentSub == null;
		}
	}
	
	internal class PDAPromptCondition : ProgressionTrigger {
		
		private readonly ProgressionTrigger baseline;
		
		public PDAPromptCondition(ProgressionTrigger p) : base(ep => C2CProgression.instance.canTriggerPDAPrompt(ep) && p.isReady(ep)) {
			baseline = p;
		}
		
		public override string ToString() {
			return "Free-swimming "+baseline;
		}
		
	}
	
	internal class PDAPrompt : DelayedProgressionEffect {
		
		private readonly PDAMessages.Messages prompt;
		
		public PDAPrompt(PDAMessages.Messages m, float f) : base(() => PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(m).key), () => PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(m).key), f) {
			prompt = m;
		}
		
		public override string ToString() {
			return "PDA Prompt "+prompt;
		}
		
	}
	
	internal class DunesPrompt : DelayedProgressionEffect {
		
		private static readonly PDAManager.PDAPage page = PDAManager.getPage("dunearchhint");
		
		public DunesPrompt() : base(() => {PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.DuneArchPrompt).key); page.unlock(false);}, () => PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.DuneArchPrompt).key), 0.006F) {
			
		}
		
		public override string ToString() {
			return "Dunes Prompt";
		}
		
	}
	
	class CrashMesaCallback : MonoBehaviour {
			
		void trigger() {
			SNUtil.playSound("event:/tools/scanner/new_encyclopediea"); //triple-click
			SNUtil.playSound("event:/player/story/RadioShallows22NoSignalAlt"); //"signal coordinates corrupted"
			PDAManager.getPage("crashmesahint").unlock(false);
		}
		
	}
		
}
	
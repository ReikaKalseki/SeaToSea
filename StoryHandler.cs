using System;
using System.Collections.Generic;
using System.Linq;

using Story;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public class StoryHandler : IStoryGoalListener {
		
		public static readonly StoryHandler instance = new StoryHandler();
		
		private readonly Dictionary<ProgressionTrigger, DelayedProgressionTrigger> triggers = new Dictionary<ProgressionTrigger, DelayedProgressionTrigger>();
		
		private StoryHandler() {
			triggers[new StoryTrigger("AuroraRadiationFixed")] = new DelayedProgressionTrigger(VoidSpikesBiome.instance.fireRadio, VoidSpikesBiome.instance.isRadioFired, 0.00003F);
			triggers[new TechTrigger(TechType.PrecursorKey_Orange)] = new DelayedStoryTrigger(SeaToSeaMod.crashMesaRadio, 0.00004F);
			triggers[new ProgressionTrigger(ep => ep.GetVehicle() is SeaMoth)] = new DelayedProgressionTrigger(SeaToSeaMod.treaderSignal.fireRadio, SeaToSeaMod.treaderSignal.isRadioFired, 0.000018F);
			
			
			StoryGoal pod12Radio = new StoryGoal("RadioKoosh26", Story.GoalType.Radio, 0);
			DelayedStoryTrigger ds = new DelayedStoryTrigger(pod12Radio, 0.00008F);
			triggers[new StoryTrigger("SunbeamCheckPlayerRange")] = ds;
			triggers[new TechTrigger(TechType.BaseNuclearReactor)] = ds;
			triggers[new TechTrigger(TechType.HighCapacityTank)] = ds;
			triggers[new TechTrigger(TechType.PrecursorKey_Purple)] = ds;
			triggers[new TechTrigger(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType)] = ds;
			triggers[new EncylopediaTrigger("SnakeMushroom")] = ds;
		}
		
		public void tick(Player ep) {
			foreach (KeyValuePair<ProgressionTrigger, DelayedProgressionTrigger> kvp in triggers) {
				if (kvp.Key.isReady(ep)) {
					//SNUtil.writeToChat("Trigger "+kvp.Key+" is ready");
					DelayedProgressionTrigger dt = kvp.Value;
					if (!dt.isFired() && UnityEngine.Random.Range(0, 1F) <= dt.chancePerTick*Time.timeScale) {
						//SNUtil.writeToChat("Firing "+dt);
						dt.fire();
					}
				}
				else {
					//SNUtil.writeToChat("Trigger "+kvp.Key+" condition is not met");
				}
			}
		}
		
		public void NotifyGoalComplete(string key) {
			//SNUtil.writeToChat("Story '"+key+"'");
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
	}
	
	class ProgressionTrigger {
		
		internal readonly Func<Player, bool> isReady;
		
		internal ProgressionTrigger(Func<Player, bool> b) {
			isReady = b;
		}
		
		public override string ToString() {
			return isReady.Method != null ? isReady.Method.Name : "unnamed callback";
		}
		
	}
	
	class TechTrigger : ProgressionTrigger {
		
		private readonly TechType tech;
		
		internal TechTrigger(TechType tt) : base(ep => KnownTech.knownTech.Contains(tt)) {
			tech = tt;
		}
		
		public override string ToString() {
			return "Tech "+tech;
		}
		
	}
	
	class EncylopediaTrigger : ProgressionTrigger {
		
		private readonly string pdaKey;
		
		internal EncylopediaTrigger(PDAManager.PDAPage g) : this(g.id) {
			
		}
		
		internal EncylopediaTrigger(string key) : base(ep => PDAEncyclopedia.entries.ContainsKey(key)) {
			pdaKey = key;
		}
		
		public override string ToString() {
			return "Ency "+pdaKey;
		}
		
	}
	
	class StoryTrigger : ProgressionTrigger {
		
		private readonly string storyKey;
		
		internal StoryTrigger(StoryGoal g) : this(g.key) {
			
		}
		
		internal StoryTrigger(string key) : base(ep => StoryGoalManager.main.completedGoals.Contains(key)) {
			storyKey = key;
		}
		
		public override string ToString() {
			return "Story "+storyKey;
		}
		
	}
	
	class DelayedProgressionTrigger {
		
		internal readonly Action fire;
		internal readonly Func<bool> isFired;
		internal readonly float chancePerTick;
		
		internal DelayedProgressionTrigger(Action a, Func<bool> b, float f) {
			fire = a;
			isFired = b;
			chancePerTick = f;
		}
		
		public override string ToString() {
			return fire.Method != null ? fire.Method.Name : "unnamed action";
		}
		
	}
	
	class DelayedStoryTrigger : DelayedProgressionTrigger {
		
		internal readonly StoryGoal goal;
		
		internal DelayedStoryTrigger(StoryGoal g, float f) : base(() => StoryGoal.Execute(g.key, g.goalType), () => StoryGoalManager.main.completedGoals.Contains(g.key), f) {
			goal = g;
		}
		
		public override string ToString() {
			return "Story "+goal.key;
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
	
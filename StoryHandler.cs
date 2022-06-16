using System;

using Story;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public class StoryHandler : IStoryGoalListener {
		
		public static readonly StoryHandler instance = new StoryHandler();
		
		private StoryHandler() {
			
		}
		
		public void NotifyGoalComplete(string key) {
			//SBUtil.writeToChat("Story '"+key+"'");
			if (key.StartsWith("OnPlay", StringComparison.InvariantCultureIgnoreCase)) {
				if (key.Contains(SeaToSeaMod.treaderSignal.getRadioStoryKey())) {
					SeaToSeaMod.treaderSignal.activate(15);
				}
				else if (key.Contains(VoidSpikesBiome.instance.getSignalKey())) {
					VoidSpikesBiome.instance.activateSignal();
				}
				else if (key.Contains(SeaToSeaMod.crashMesaRadio.key)) {
					Player.main.gameObject.EnsureComponent<CrashMesaCallback>().Invoke("trigger", 15);
				}
			}
			switch(key) {
				case "SunbeamCheckPlayerRange":
					Player.main.gameObject.EnsureComponent<AvoliteSpawner.TriggerCallback>().Invoke("trigger", 39);
				break;
				case "drfwarperheat":
					KnownTech.Add(SeaToSeaMod.cyclopsHeat.TechType);
				break;
				case "AuroraRadiationFixed":
    				VoidSpikesBiome.instance.fireRadio();
				break;
			}
		}
	}
	
	class CrashMesaCallback : MonoBehaviour {
			
		void trigger() {
			SBUtil.playSound("event:/tools/scanner/new_encyclopediea"); //triple-click
			PDAManager.getPage("crashmesahint").unlock(false);
		}
		
	}
		
}
	
using System;

using Story;

using SMLHelper.V2.Handlers;

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
					SeaToSeaMod.treaderSignal.activate();
				}
				else if (key.Contains(VoidSpikesBiome.instance.getSignalKey())) {
					VoidSpikesBiome.instance.activateSignal();
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
					StoryGoal.Execute(SeaToSeaMod.crashMesaRadio.key, SeaToSeaMod.crashMesaRadio.goalType);
				break;
			}
		}
	}
}
	
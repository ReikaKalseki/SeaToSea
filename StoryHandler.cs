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
			SBUtil.writeToChat("'"+key+"'");
		}
		
	}
}
	
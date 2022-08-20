using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml;

using Story;

using SMLHelper.V2.Handlers;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public static class PDAMessages {
		
		private static readonly Dictionary<Messages, StoryGoal> mappings = new Dictionary<Messages, StoryGoal>();
		
		public static void addAll() {
			foreach (Messages m in Enum.GetValues(typeof(Messages))) {
				Message attr = getAttr(m);
				string id = attr.key;//Enum.GetName(typeof(Messages), m);
				SNUtil.log("Constructing PDA message "+id);
				XMLLocale.LocaleEntry e = SeaToSeaMod.miscLocale.getEntry(id);
				StoryGoal item = SNUtil.addVOLine(e.key, Story.GoalType.PDA, e.desc, SoundManager.registerSound("prompt_"+e.key, e.pda, SoundSystem.voiceBus), attr.delay);
				mappings[m] = item;
			}
		}
		
		public enum Messages {
			[Message("voidspikeenter", 5)]VoidSpike,
			[Message("aurorafire")]AuroraFireWarn,
			[Message("auroracut")]AuroraSalvage,
			[Message("kelpcavedrone")]KelpCavePrompt,
		}
		
		private static Message getAttr(Messages key) {
			FieldInfo info = typeof(Messages).GetField(Enum.GetName(typeof(Messages), key));
			return (Message)Attribute.GetCustomAttribute(info, typeof(Message));
		}
		
		public static StoryGoal getMessage(Messages key) {
			return mappings[key];
		}
		
		public static bool trigger(Messages m) {
			StoryGoal sg = getMessage(m);
	    	if (!StoryGoalManager.main.completedGoals.Contains(sg.key)) {
	    		StoryGoal.Execute(sg.key, sg.goalType);
	    		return true;
	    	}
			return false;
		}
		
		public class Message : Attribute {
			
			public readonly string key;
			public readonly int delay;
			
			public Message(string s, int d = 0) {
				key = s;
			}
		}
	}
}

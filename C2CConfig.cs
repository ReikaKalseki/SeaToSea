using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public class C2CConfig
	{		
		public enum ConfigEntries {
			[ConfigEntry("Additional Exploration Prompts", true)]PROMPTS, //Whether to make progression a bit more straightforward by enabling additional prompts from your PDA
			[ConfigEntry("Platinum Theft Chance", typeof(float), 0.5F, 0.25F, 1F, 0)]PLATTHEFT, //How likely platinum is to be stolen from your inventory by [redacted]
			[ConfigEntry("Hard Mode", false)]HARDMODE, //Whether to enable hard mode and all of its effects
			[ConfigEntry("Enable Lifepod Drift", false)]PODFAIL, //Whether pod 5 should after a short time begin drifting and then sinking, before being carried out of the map entirely
			[ConfigEntry("Save Warning Threshold", typeof(float), 15F, 0F, 720F, 0)]SAVETHRESH, //How long, in minutes, after a save should you be prompted to save again, if possible
			[ConfigEntry("Save Warning Cooldown", typeof(float), 2F, 0F, 720F, 0)]SAVECOOL, //How long must elapse between save prompts, in minutes
		}
	}
}

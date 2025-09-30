using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	public class C2CConfig {
		public enum ConfigEntries {
			[ConfigEntry("Additional Exploration Prompts", true)]PROMPTS, //Whether to make progression a bit more straightforward by enabling additional prompts from your PDA
			[ConfigEntry("Platinum Theft Chance", typeof(float), 0.5F, 0.25F, 1F, float.NaN)]PLATTHEFT, //How likely platinum is to be stolen from your inventory by [redacted]
			[ConfigEntry("Hard Mode", false)]HARDMODE, //Whether to enable hard mode and all of its effects
			[ConfigEntry("Enable Lifepod Drift", false, true)]PODFAIL, //Whether pod 5 should after a short time begin drifting and then sinking, before being carried out of the map entirely
			[ConfigEntry("Save Warning Threshold", typeof(float), 15F, 0F, 720F, float.NaN)]SAVETHRESH, //How long, in minutes, after a save should you be prompted to save again, if possible
			[ConfigEntry("Save Warning Cooldown", typeof(float), 2F, 0F, 720F, float.NaN)]SAVECOOL, //How long must elapse between save prompts, in minutes
			//[ConfigEntry("Sleep Morale Restoration", typeof(int), 25, 5, 50, float.NaN)]SLEEPMORALE, //How much morale sleeping restores.
			[ConfigEntry("Deco Morale Decay/Gain Speed", typeof(float), 1, 0.1F, 10, float.NaN)]MORALESPEED, //A multiplier for how fast morale decays or grows based on deco rating.
		}
	}
}

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
		}
	}
}

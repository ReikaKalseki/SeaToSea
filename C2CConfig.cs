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
			[ConfigEntry("Additional Exploration Prompts", true)]PROMPTS,
			[ConfigEntry("Platinum Theft Chance", typeof(float), 0.5F, 0.25F, 1F, 0)]PLATTHEFT,
			[ConfigEntry("Hard Mode", false)]HARDMODE,
			[ConfigEntry("Enable Lifepod Drift", false)]PODFAIL,
		}
	}
}

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
		}
	}
}

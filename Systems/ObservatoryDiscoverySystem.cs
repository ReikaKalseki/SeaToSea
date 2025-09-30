using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	public class ObservatoryDiscoverySystem {

		public static readonly ObservatoryDiscoverySystem instance = new ObservatoryDiscoverySystem();

		private ObservatoryDiscoverySystem() {

		}

		public void tick(Player ep) {

		}

		enum BiomeTypes {
			SHALLOW,
			MODERATE,
			DEEP,
			LOST,
			//ILZ,
			
		}
	}
}

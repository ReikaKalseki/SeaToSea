using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class PrecursorPipeFastTravelConsole : PrecursorStoryConsole {

		internal PrecursorPipeFastTravelConsole(XMLLocale.LocaleEntry e) : base(e) {
			setPopup(TechType.PrecursorSurfacePipe);
		}

		public override bool isUsable(StoryConsoleTag tag) {
			return true;
		}

	}
}

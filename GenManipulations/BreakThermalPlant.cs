/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	internal class BreakThermalPlant : ManipulationBase {

		private bool disablePowergen;
		private bool deleteHead;
		private bool disableBar;
		private bool deleteBar;
		private string textOverride;
		private Color? textColor;

		public override void applyToObject(GameObject go) {
			go.removeComponent<ThermalPlant>();
			if (deleteHead)
				go.removeChildObject("model/root/head");
			GameObject text = go.getChildObject("UI/Canvas/Text");
			Text t = text.GetComponent<Text>();
			if (!string.IsNullOrEmpty(textOverride))
				t.text = textOverride;
			if (textColor != null && textColor.HasValue)
				t.color = textColor.Value;
			if (deleteBar)
				go.removeChildObject("UI/Canvas/temperatureBar");
			if (disableBar)
				go.removeChildObject("UI/Canvas/temperatureBar/temperatureBarForeground");
			if (disablePowergen) {
				go.removeComponent<PowerSource>();
				go.removeComponent<PowerFX>();
				go.removeComponent<PowerRelay>();
				go.removeComponent<PowerSystemPreview>();
			}
		}

		public override void applyToObject(PlacedObject go) {
			this.applyToObject(go.obj);
		}

		public override void loadFromXML(XmlElement e) {
			disablePowergen = e.getBoolean("RemovePower");
			deleteHead = e.getBoolean("DeleteHead");
			disableBar = e.getBoolean("DisableBar");
			deleteBar = e.getBoolean("DeleteBar");
			textOverride = e.getProperty("SetText", true);
			textColor = e.getColor("TextColor", false, true);
		}

		public override void saveToXML(XmlElement e) {
			e.addProperty("RemovePower", disablePowergen);
			e.addProperty("DeleteHead", deleteHead);
			e.addProperty("DisableBar", disableBar);
			e.addProperty("DeleteBar", deleteBar);
			if (!string.IsNullOrEmpty(textOverride))
				e.addProperty("SetText", textOverride);
			if (textColor != null && textColor.HasValue)
				e.addProperty("TextColor", textColor.Value);
		}

	}
}

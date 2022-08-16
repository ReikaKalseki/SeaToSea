/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{		
	internal class BreakThermalPlant : ManipulationBase {
		
		private bool disablePowergen;
		private bool deleteHead;
		private bool disableBar;
		private bool deleteBar;
		private string textOverride;
		private Color? textColor;
		
		static BreakThermalPlant() {
			
		}
		
		internal override void applyToObject(GameObject go) {
			ObjectUtil.removeComponent<ThermalPlant>(go);
			if (deleteHead)
				ObjectUtil.removeChildObject(go, "model/root/head");
			GameObject text = ObjectUtil.getChildObject(go, "UI/Canvas/Text");
			Text t = text.GetComponent<Text>();
			if (!string.IsNullOrEmpty(textOverride))
				t.text = textOverride;
			if (textColor != null && textColor.HasValue)
				t.color = textColor.Value;
			if (deleteBar)
				ObjectUtil.removeChildObject(go, "UI/Canvas/temperatureBar");
			if (disableBar)
				ObjectUtil.removeChildObject(go, "UI/Canvas/temperatureBar/temperatureBarForeground");
			if (disablePowergen) {
				ObjectUtil.removeComponent<PowerSource>(go);
				ObjectUtil.removeComponent<PowerFX>(go);
				ObjectUtil.removeComponent<PowerRelay>(go);
				ObjectUtil.removeComponent<PowerSystemPreview>(go);
			}
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			disablePowergen = e.getBoolean("RemovePower");
			deleteHead = e.getBoolean("DeleteHead");
			disableBar = e.getBoolean("DisableBar");
			deleteBar = e.getBoolean("DeleteBar");
			textOverride = e.getProperty("SetText", true);
			textColor = e.getColor("TextColor", false, true);
		}
		
		internal override void saveToXML(XmlElement e) {
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

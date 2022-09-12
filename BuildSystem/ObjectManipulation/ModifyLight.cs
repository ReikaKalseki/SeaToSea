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
	internal sealed class ModifyLight : ModifyComponent<Light> {
		
		private double range = 1;
		private double intensity = 1;
		private Color? color = Color.white;
		
		internal override void modifyComponent(Light c) {
			c.range = (float)range;
			c.intensity = (float)intensity;
			if (color != null && color.HasValue)
				c.color = color.Value;
		}
		
		internal override void loadFromXML(XmlElement e) {
			range = e.getFloat("range", double.NaN);
			intensity = e.getFloat("intensity", double.NaN);
			color = e.getColor("color", true);
		}
		
		internal override void saveToXML(XmlElement e) {
			e.addProperty("intensity", intensity);
			e.addProperty("range", range);
			if (color != null && color.HasValue)
				e.addProperty("color", color.Value);
		}
		
	}
}

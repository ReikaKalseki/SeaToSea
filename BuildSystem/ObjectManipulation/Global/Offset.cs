﻿/*
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
	internal class Offset : ManipulationBase {
		
		private Vector3 translate = Vector3.zero;
		
		internal override void applyToObject(GameObject go) {
			go.transform.position = (go.transform.position+translate);
		}
		
		internal override void applyToObject(PlacedObject go) {
			go.move(translate.x, translate.y, translate.z);
		}
		
		internal override void loadFromXML(XmlElement e) {
			translate.x = (float)e.getFloat("x", double.NaN);
			translate.y = (float)e.getFloat("y", double.NaN);
			translate.z = (float)e.getFloat("z", double.NaN);
		}
		
		internal override void saveToXML(XmlElement e) {
			e.addProperty("x", translate.x);
			e.addProperty("y", translate.y);
			e.addProperty("z", translate.z);
		}
		
	}
}
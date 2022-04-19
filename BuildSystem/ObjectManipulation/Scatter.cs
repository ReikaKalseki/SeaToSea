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
	internal class Scatter : ManipulationBase {
		
		private Vector3 range = Vector3.zero;
		
		internal override void applyToObject(PlacedObject go) {
			double dx = UnityEngine.Random.Range(-range.x, range.x);
			double dy = UnityEngine.Random.Range(-range.y, range.y);
			double dz = UnityEngine.Random.Range(-range.z, range.z);
			go.move(dx, dy, dz);
		}
		
		internal override void loadFromXML(XmlElement e) {
			range = e.getVector("Scatter");
		}
		
		internal override void saveToXML(XmlElement e) {
			e.addProperty("x", range.x);
			e.addProperty("y", range.y);
			e.addProperty("z", range.z);
		}
		
	}
}

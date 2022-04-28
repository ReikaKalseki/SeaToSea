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
	internal class Scale : ManipulationBase {
		
		private Vector3 min = Vector3.one;
		private Vector3 max = Vector3.one;
		
		internal override void applyToObject(GameObject go) {
			Vector3 sc = MathUtil.getRandomVectorBetween(min, max);
			Vector3 vec = go.transform.position;
			vec.x *= sc.x;
			vec.y *= sc.y;
			vec.z *= sc.z;
			go.transform.position = vec;
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
			go.setPosition(go.obj.transform.position);
		}
		
		internal override void loadFromXML(XmlElement e) {
			min = e.getVector("min").Value;
			max = e.getVector("max").Value;
		}
		
		internal override void saveToXML(XmlElement e) {
			e.addProperty("min", min);
			e.addProperty("max", max);
		}
		
	}
}

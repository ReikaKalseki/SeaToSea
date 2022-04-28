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
	internal class Rotate : ManipulationBase {
		
		private Vector3 min = Vector3.zero;
		private Vector3 max = Vector3.zero;
		private Vector3 origin = Vector3.zero;
		
		internal override void applyToObject(GameObject go) {
			Vector3 rot = MathUtil.getRandomVectorBetween(min, max);
			MathUtil.rotateObjectAround(go, origin, rot.y);
			
			go.transform.RotateAround(origin, Vector3.right, (float)rot.x);
			go.transform.RotateAround(origin, Vector3.forward, (float)rot.z);
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
			go.setPosition(go.obj.transform.position);
			go.setRotation(go.obj.transform.rotation);
		}
		
		internal override void loadFromXML(XmlElement e) {
			min = e.getVector("min").Value;
			max = e.getVector("max").Value;
			Vector3? or = e.getVector("origin", true);
			if (or != null && or.HasValue)
				origin = or.Value;
		}
		
		internal override void saveToXML(XmlElement e) {
			e.addProperty("min", min);
			e.addProperty("max", max);
			e.addProperty("origin", origin);
		}
		
	}
}

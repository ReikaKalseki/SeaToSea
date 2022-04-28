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
	internal class Rotate : GlobalManipulation {
		
		private Vector3 min = Vector3.zero;
		private Vector3 max = Vector3.zero;
		private Vector3 origin = Vector3.zero;
		
		internal override void applyToGlobalObject(GameObject go) {
			Vector3 rot = MathUtil.getRandomVectorBetween(min, max);
			MathUtil.rotateObjectAround(go, origin, rot.y);
			
			go.transform.RotateAround(origin, Vector3.right, (float)rot.x);
			go.transform.RotateAround(origin, Vector3.forward, (float)rot.z);
		}
		
		internal override void applyToGlobalObject(PlacedObject go) {
			applyToObject(go.obj);
			go.setPosition(go.obj.transform.position);
			go.setRotation(go.obj.transform.rotation);
		}
		
		internal override void applyToSpecificObject(PlacedObject go) {
			applyToSpecificObject(go.obj);
			go.setRotation(go.obj.transform.rotation);
		}
		
		internal override void applyToSpecificObject(GameObject go) {
			Vector3 rot = MathUtil.getRandomVectorBetween(min, max);
			go.transform.RotateAround(go.transform.position, Vector3.up, rot.y);
			go.transform.RotateAround(go.transform.position, Vector3.right, rot.x);
			go.transform.RotateAround(go.transform.position, Vector3.forward, rot.z);
		}
		
		internal override void loadFromXML(XmlElement e) {
			base.loadFromXML(e);
			min = e.getVector("min").Value;
			max = e.getVector("max").Value;
			Vector3? or = e.getVector("origin", true);
			if (or != null && or.HasValue)
				origin = or.Value;
		}
		
		internal override void saveToXML(XmlElement e) {
			base.saveToXML(e);
			e.addProperty("min", min);
			e.addProperty("max", max);
			e.addProperty("origin", origin);
		}
		
	}
}

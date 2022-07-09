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
	internal sealed class AddChild : ManipulationBase {
		
		private string id;
		private string objName;
		private Vector3 relativePos;
		
		internal override void applyToObject(GameObject go) {
			if (!string.IsNullOrEmpty(objName)) {
				if (ObjectUtil.getChildObject(go, objName) != null)
					return;
			}
			GameObject add = ObjectUtil.createWorldObject(id);
			add.transform.parent = go.transform;
			add.transform.localPosition = relativePos;
			if (!string.IsNullOrEmpty(objName))
				add.name = objName;
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			id = e.getProperty("prefab");
			Vector3? vec = e.getVector("position", true);
			relativePos = vec != null && vec.HasValue ? vec.Value : Vector3.zero;
			objName = e.getProperty("name", true);
		}
		
		internal override void saveToXML(XmlElement e) {
			e.addProperty("prefab", id);
			e.addProperty("position", relativePos);
			if (!string.IsNullOrEmpty(objName))
				e.addProperty("name", objName);
		}
		
		public override bool needsReapplication() {
			return false;
		}
		
	}
}

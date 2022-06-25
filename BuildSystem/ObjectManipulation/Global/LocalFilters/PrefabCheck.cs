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
	internal sealed class PrefabCheck : LocalCheck {
		
		private string id;
		
		internal override bool apply(GameObject go) {
			return ObjectUtil.getPrefabID(go) == id;
		}
		
		internal override void loadFromXML(XmlElement e) {
			id = e.getProperty("id");
		}
		
		internal override void saveToXML(XmlElement e) {
			e.addProperty("id", id);
		}
		
	}
}

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
	internal class LocalizeGlobalRotationScale : ManipulationBase {
		
		internal override void applyToObject(GameObject go) {
			//NOOP
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
			go.setRotation(go.obj.transform.rotation);
			go.scale = go.obj.transform.localScale;
		}
		
		internal override void loadFromXML(XmlElement e) {
			
		}
		
		internal override void saveToXML(XmlElement e) {
			
		}
		
	}
}

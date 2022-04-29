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
	internal abstract class ManipulationBase {
		
		internal abstract void applyToObject(PlacedObject go);
		internal abstract void applyToObject(GameObject go);
		
		internal abstract void loadFromXML(XmlElement e);
		internal abstract void saveToXML(XmlElement e);
		
		public override string ToString() {
			XmlDocument doc = new XmlDocument();
			XmlElement e = doc.CreateElement(this.GetType().Name);
			this.saveToXML(e);
			return this.GetType()+" : "+e.InnerText;
		}
		
	}
}

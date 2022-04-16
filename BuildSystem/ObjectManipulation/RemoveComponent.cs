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
	internal class RemoveComponent : ManipulationBase {
		
		private Type type;
		
		internal override void applyToObject(PlacedObject go) {
			Component[] cc = go.obj.GetComponentsInParent(type);
			foreach (Component c in cc) {
				UnityEngine.Object.Destroy(c);
			}
		}
		
		internal override void loadFromXML(XmlElement e) {
			type = InstructionHandlers.getTypeBySimpleName(e.InnerText);
		}
		
		internal override void saveToXML(XmlElement e) {
			e.InnerText = type.Name;
		}
		
	}
}

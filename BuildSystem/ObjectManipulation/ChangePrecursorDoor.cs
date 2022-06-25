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
	internal class ChangePrecursorDoor : ManipulationBase {
		
		private PrecursorKeyTerminal.PrecursorKeyType targetType; 
		
		static ChangePrecursorDoor() {
			
		}
		
		internal override void applyToObject(GameObject go) {
			PrecursorKeyTerminal pk = go.GetComponentInChildren<PrecursorKeyTerminal>();
			if (pk == null) {
				foreach (Component c in go.GetComponentsInChildren<Component>()) {
					SNUtil.log("extra Component "+c+"/"+c.GetType()+" in "+c.gameObject);
				}
			}
			pk.acceptKeyType = targetType;
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			targetType = (PrecursorKeyTerminal.PrecursorKeyType)Enum.Parse(typeof(PrecursorKeyTerminal.PrecursorKeyType), e.InnerText);
		}
		
		internal override void saveToXML(XmlElement e) {
			e.InnerText = Enum.GetName(typeof(PrecursorKeyTerminal.PrecursorKeyType), targetType);
		}
		
	}
}

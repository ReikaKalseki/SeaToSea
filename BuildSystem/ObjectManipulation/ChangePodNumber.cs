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
	internal class ChangePodNumber : ManipulationBase {
		
		private static readonly string textureSeek = "life_pod_exterior_decals_";
		private static readonly string newTexBase = "lifepod_numbering_";
		
		static ChangePodNumber() {
			
		}
		
		private int targetNumber; 
		
		internal override void applyToObject(GameObject go) {
		 	foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
			 	foreach (Material m in r.materials) {
			 		foreach (string n in m.GetTexturePropertyNames()) {
			 			Texture tex = m.GetTexture(n);
			 			if (tex is Texture2D) {
			 				string file = tex.name;
			 				if (file.Contains(textureSeek)) {
			 					Texture2D tex2 = TextureManager.getTexture("Textures/"+newTexBase+"_"+targetNumber);
			 					m.SetTexture(n, tex2);
			 				}
			 			}
			 		}
			 	}
			 	r.UpdateGIMaterials();
		 	}
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			targetNumber = int.Parse(e.InnerText);
		}
		
		internal override void saveToXML(XmlElement e) {
			e.InnerText = targetNumber.ToString();
		}
		
	}
}

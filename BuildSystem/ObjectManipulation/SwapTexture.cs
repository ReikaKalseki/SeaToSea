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
	internal class SwapTexture : ManipulationBase {
		
		private readonly Dictionary<string, string> swaps = new Dictionary<string, string>();
		
		public SwapTexture() {
			
		}
		
		protected void addSwap(string from, string to) {
			swaps[from] = to;
		}
		
		protected virtual Texture2D getTexture(string name, string texType) {
			return TextureManager.getTexture("Textures/"+name);
		}
		
		internal override void applyToObject(GameObject go) {
		 	foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
			 	foreach (Material m in r.materials) {
					if (m.mainTexture != null) {
						string put = swaps.ContainsKey(m.mainTexture.name) ? swaps[m.mainTexture.name] : null;
				 		if (put != null) {
				 			Texture2D tex2 = getTexture(put, "main");
			 				if (tex2 != null)
				 				m.mainTexture = tex2;
			 				else
			 					SNUtil.writeToChat("Could not find texture "+put);
				 		}
					}
			 		foreach (string n in m.GetTexturePropertyNames()) {
			 			Texture tex = m.GetTexture(n);
			 			if (tex is Texture2D) {
			 				string file = tex.name;
			 				string put = swaps.ContainsKey(file) ? swaps[file] : null;
			 				//SNUtil.writeToChat(n+" > "+file+" > "+put);
			 				if (put != null) {
			 					Texture2D tex2 = getTexture(put, n);
			 					//SNUtil.writeToChat(">>"+tex2);
			 					if (tex2 != null)
			 						m.SetTexture(n, tex2);
			 					else
			 						SNUtil.writeToChat("Could not find texture "+put);
			 				}
			 			}
			 		}
			 	}
			 	r.UpdateGIMaterials();
		 	}
		}
		
		internal sealed override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			swaps.Clear();
			foreach (XmlNode n2 in e.ChildNodes) {
				if (n2 is XmlElement) {
					XmlElement e2 = (XmlElement)n2;
					swaps[e2.getProperty("from")] = e2.getProperty("to");
				}
			}
		}
		
		internal override void saveToXML(XmlElement e) {
			foreach (KeyValuePair<string, string> kvp in swaps) {
				XmlElement e2 = e.OwnerDocument.CreateElement("swap");
				e2.addProperty("from", kvp.Key);
				e2.addProperty("to", kvp.Value);
				e.AppendChild(e2);
			}
		}
		
	}
}

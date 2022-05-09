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
	internal class CleanupDegasiProp : ManipulationBase {
		
		private static readonly Dictionary<string, string> textureSwaps = new Dictionary<string, string>();
		
		static CleanupDegasiProp() {
			textureSwaps["Base_abandoned_Foundation_Platform_01"] = "Base_Foundation_Platform_01";
			textureSwaps["Base_abandoned_Foundation_Platform_01_normal"] = "Base_Foundation_Platform_01_normal";
			textureSwaps["Base_abandoned_Foundation_Platform_01_illum"] = "Base_Foundation_Platform_01_illum";
		}
		
		internal override void applyToObject(GameObject go) {
			Transform t = go.transform.Find("BaseCell/Coral");
		 	if (t != null)
				UnityEngine.Object.Destroy(t.gameObject);//t.gameObject.SetActive(false);
		 	t = go.transform.Find("BaseCell/Decals");
		 	if (t != null)
		 		UnityEngine.Object.Destroy(t.gameObject);//t.gameObject.SetActive(false);
		 	foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
			 	foreach (Material m in r.materials) {
					string put = textureSwaps.ContainsKey(m.mainTexture.name) ? textureSwaps[m.mainTexture.name] : null;
			 		if (put != null) {
			 			Texture2D tex2 = TextureManager.getTexture("Textures/"+put);
			 			m.mainTexture = tex2;
			 		}
			 		foreach (string n in m.GetTexturePropertyNames()) {
			 			Texture tex = m.GetTexture(n);
			 			if (tex is Texture2D) {
			 				string file = tex.name;
			 				put = textureSwaps.ContainsKey(file) ? textureSwaps[file] : null;
			 				//SBUtil.writeToChat(n+" > "+file+" > "+put);
			 				if (put != null) {
			 					Texture2D tex2 = TextureManager.getTexture("Textures/"+put);
			 					//SBUtil.writeToChat(">>"+tex2);
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
			
		}
		
		internal override void saveToXML(XmlElement e) {
			
		}
		
	}
}

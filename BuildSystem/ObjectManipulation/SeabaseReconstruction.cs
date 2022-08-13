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
	internal class SeabaseReconstruction : ManipulationBase {
		
		private readonly XmlElement data;
		
		internal SeabaseReconstruction(XmlElement e) {
			data = e;
		}
		
		internal override void applyToObject(GameObject go) {
			SNUtil.log("Reconstructing seabase: "+data.OuterXml);
			foreach (XmlElement e2 in data.getDirectElementsByTagName("part")) {
				CustomPrefab pfb = new CustomPrefab("9d3e9fa5-a5ac-496e-89f4-70e13c0bedd5"); //BaseCell
				pfb.loadFromXML(e2);
				SNUtil.log("Reconstructed BaseCell: "+pfb);
				GameObject go2 = pfb.createWorldObject();
				go2.transform.parent = go.transform;
				List<XmlElement> li1 = e2.getDirectElementsByTagName("cellData");
				if (li1.Count == 1) {
					foreach (XmlElement e3 in li1[0].getDirectElementsByTagName("component")) {
						CustomPrefab pfb2 = new CustomPrefab("basePart");
						//Base.Piece type = Enum.Parse(typeof(Base.Piece), e3.getProperty("piece"));
						pfb2.loadFromXML(e3);
						if (pfb2.prefabName == PlacedObject.BUBBLE_PREFAB)
							continue;
						SNUtil.log("Reconstructed base component: "+pfb2);
						GameObject go3 = pfb2.createWorldObject();
						go3.transform.parent = go2.transform;
						rebuildNestedObjects(go3, e3);
						List<XmlElement> li0 = e3.getDirectElementsByTagName("supportData");
						if (li0.Count == 1)
							new SeabaseLegLengthPreservation(li0[0]).applyToObject(go3);
					}
				}
			}
		}
			
		private void rebuildNestedObjects(GameObject main, XmlElement e) {
			foreach (XmlElement e2 in e.getDirectElementsByTagName("child")) {
				CustomPrefab pfb = new CustomPrefab(e2.getProperty("prefab"));
				pfb.loadFromXML(e2);
				GameObject go = pfb.createWorldObject();
				if (go != null) {
					go.transform.parent = main.transform;
					rebuildNestedObjects(go, e2);
				}
			}
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			
		}
		
		internal override void saveToXML(XmlElement e) {
			
		}
		
		public override bool needsReapplication() {
			return true;
		}
		
	}
}

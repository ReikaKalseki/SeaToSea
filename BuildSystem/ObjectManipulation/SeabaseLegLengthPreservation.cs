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
	internal class SeabaseLegLengthPreservation : ManipulationBase {
		
		private readonly XmlElement data;
		
		internal SeabaseLegLengthPreservation(XmlElement e) {
			data = e;
		}
		
		internal override void applyToObject(GameObject go) {
			BaseFoundationPiece bf = go.GetComponent<BaseFoundationPiece>();
			if (bf != null) {
				if (data == null) {
					UnityEngine.Object.DestroyImmediate(bf.gameObject);
				}
				else {
					bf.maxPillarHeight = (float)data.getFloat("maxHeight", double.NaN);
					bf.extraHeight = (float)data.getFloat("extra", double.NaN);
					bf.minHeight = (float)data.getFloat("minHeight", double.NaN);
					List<XmlElement> li = data.getDirectElementsByTagName("pillar");
					foreach (BaseFoundationPiece.Pillar p in bf.pillars) {
						Transform l = p.adjustable;
						if (l) {
							bool matched = false;
							foreach (XmlElement e3 in li) {
								Vector3 pos = e3.getVector("position").Value;
								//SNUtil.log("Comparing xml pos "+pos+" to "+l.position+" = "+Vector3.Distance(pos, l.position));
								if (Vector3.Distance(pos, l.position) <= 0.25) {
									l.rotation = e3.getQuaternion("rotation").Value;
									l.localScale = e3.getVector("scale").Value;
									SNUtil.log("Applied pillar match "+e3.OuterXml);
									matched = true;
								}
							}
							if (!matched || (!p.bottom && p.adjustable.localScale == Vector3.one)) {
								//SNUtil.log("Destroying base leg @ "+l.position);
								if (p.bottom)
									UnityEngine.Object.DestroyImmediate(p.bottom.gameObject);
								UnityEngine.Object.DestroyImmediate(p.adjustable.gameObject);
								UnityEngine.Object.DestroyImmediate(p.root);
							}
							foreach (Shocker s in UnityEngine.Object.FindObjectsOfType<Shocker>()) {
								if (s && s.gameObject)
									ObjectUtil.ignoreCollisions(s.gameObject, l.gameObject);
							}
						}
					}
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
			return false;
		}
		
	}
}

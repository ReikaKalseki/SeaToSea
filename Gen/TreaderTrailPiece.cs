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
	internal class TreaderTrailPiece : WorldGenerator {
		
		private readonly PodDebris debris;
		
		private float intensity = 1;
		
		public TreaderTrailPiece(Vector3 vec) : base(vec) {
			debris = new PodDebris(vec);
		}
		
		public override bool generate(List<GameObject> li) {
			debris.generateRecognizablePieces = false;
			debris.scrapCount = Math.Max(1, (int)(intensity*UnityEngine.Random.Range(2, 6)));
			debris.paperCount = 0;
			debris.debrisScale = 0.25F;
			debris.debrisAmount = intensity*1.5F;
			debris.yBaseline = position.y+0.55F;
			debris.areaSpread = 0.5F;
			debris.bounds = new Vector3(4, 0, 4);
			return debris.generate(li);
		}
		
		public override LargeWorldEntity.CellLevel getCellLevel() {
			return LargeWorldEntity.CellLevel.Far;
		}
		
		public override void loadFromXML(XmlElement e) {
			intensity = (float)e.getFloat("intensity", intensity);
		}
		
		public override void saveToXML(XmlElement e) {			
			e.addProperty("intensity", intensity);
		}
		
	}
}

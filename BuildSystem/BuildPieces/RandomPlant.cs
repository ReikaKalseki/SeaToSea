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
	internal class RandomPlant : PieceBase {
		
		private readonly WeightedRandom<VanillaFlora> plants = new WeightedRandom<VanillaFlora>();
		
		private bool preferLit = true;
		
		public RandomPlant(Vector3 vec) : base(vec) {
			
		}
		
		public override void generate(List<GameObject> li) {
			GameObject go = PlacedObject.createWorldObject(plants.getRandomEntry().getRandomPrefab(preferLit));
			go.transform.position = position;
			go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360F), Vector3.up);
			li.Add(go);
		}
		
		public override void loadFromXML(XmlElement e) {
			foreach (XmlElement e2 in e.getDirectElementsByTagName("plant")) {
				string name = e2.getProperty("name");
				string wt = e2.getProperty("weight");
				plants.addEntry(VanillaFlora.getByName(name), double.Parse(wt));
			}
			preferLit = e.getBoolean("lit");
		}
		
		public override void saveToXML(XmlElement e) {
			foreach (VanillaFlora f in plants.getValues()) {
				XmlElement e2 = e.OwnerDocument.CreateElement("plant");
				e2.addProperty("name", f.getName());
				e2.addProperty("weight", plants.getWeight(f));
				e.AppendChild(e2);
			}
			e.addProperty("lit", preferLit);
		}
		
	}
}

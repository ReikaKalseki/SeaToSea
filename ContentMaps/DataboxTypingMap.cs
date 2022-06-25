/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 08/04/2022
 * Time: 4:55 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public class DataboxTypingMap
	{
		public static readonly DataboxTypingMap instance = new DataboxTypingMap();
		
		private readonly Dictionary<Vector3, Dictionary<Vector3, TechType>> data = new Dictionary<Vector3, Dictionary<Vector3, TechType>>();
		
		private DataboxTypingMap() {

		}
		
		public void load() {
			string xml = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "XML/databoxes.xml");
			if (File.Exists(xml)) {
				SNUtil.log("Loading databox map from XML @ "+xml);
				XmlDocument doc = new XmlDocument();
				doc.Load(xml);
				foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
					try {
						Vector3 pos = e.getVector("position").Value;
						string tech = e.getProperty("tech");
						TechType techt = (TechType)Enum.Parse(typeof(TechType), tech);
						addValue(pos, techt);
					}
					catch (Exception ex) {
						SNUtil.log("Could not load element "+e.InnerText);
						SNUtil.log(ex.ToString());
					}
				}
			}
			else {
				SNUtil.log("Databox XML not found!");
			}
		}
		
		public void addValue(double x, double y, double z, TechType type) {
			addValue(new Vector3((float)x, (float)y, (float)z), type);
		}
		
		public void addValue(Vector3 pos, TechType type) {
			Vector3 rnd = getRounded(pos);
			if (!data.ContainsKey(rnd)) {
				data[rnd] = new Dictionary<Vector3, TechType>();
			}
			data[rnd][pos] = type;
			SNUtil.log("Registered databox mapping "+type+" @ "+pos);
		}
		
		public TechType getOverride(BlueprintHandTarget bpt) {
			Vector3 pos = bpt.gameObject.transform.position;
			Vector3 rounded = getRounded(pos);
			Dictionary<Vector3, TechType> map = null;
			if (data.TryGetValue(rounded, out map)) {
				foreach (KeyValuePair<Vector3, TechType> kvp in map) {
					if (kvp.Key.DistanceSqrXZ(pos) <= 1) {
						return kvp.Value;
					}
				}
			}
			return TechType.None;
		}
		
		private Vector3 getRounded(Vector3 vec) {
			int x = (int)Math.Floor(vec.x);
			int y = (int)Math.Floor(vec.y);
			int z = (int)Math.Floor(vec.z);
			return new Vector3(x/64, y/64, z/64);
		}
		
	}
}

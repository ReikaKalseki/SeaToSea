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
	internal class GrandReefPodRandomizer : ManipulationBase {
		
		private static readonly Dictionary<string, Pod> prefabs = new Dictionary<string, Pod>();
		
		private static readonly double measuredYDatum = -25.26;
		
		static GrandReefPodRandomizer() { //median 4.81
			addPodType("228e5af5-a579-4c99-9fb0-04b653f73cd3", -19.98-2.5, -26.62); //"WorldEntities/Environment/Coral_reef_floating_stones_small_01"
			addPodType("1645f35d-af23-4b98-b1e4-44d430421721", -20.18, -31.74); //"WorldEntities/Environment/Coral_reef_floating_stones_small_02"
			addPodType("1cafd118-47e6-48c4-bfd7-718df9984685", -20.45, -32.52); //"WorldEntities/Environment/Coral_reef_floating_stones_mid_01"
			addPodType("7444baa0-1416-4cb6-aa9a-162ccd4b98c7", -20.78, -43.45); //"WorldEntities/Environment/Coral_reef_floating_stones_mid_02"
			addPodType("c72724f3-125d-4e87-b82f-a91b5892c936", -20.7, -52.36); //"WorldEntities/Environment/Coral_reef_floating_stones_big_02"
		}
		
		private static void addPodType(string prefab, double ymax, double ymin) {
			prefabs[prefab] = new Pod(prefab, ymax-measuredYDatum, ymin-measuredYDatum);
		}
		
		private bool randomType = false;
		private bool allowMediumSize = true;
		private bool allowLargeSize = true;
		
		private bool randomHeight = false;
		private double referenceY = 0;
		private double groundThickness = 0;
		private double maxSinkFraction = 1;
		
		internal override void applyToObject(GameObject go) {
			string id = ObjectUtil.getPrefabID(go);
			double hoff = 0;
			if (randomType) {
				Pod old = prefabs[id];
				List<Pod> li = new List<Pod>(prefabs.Values);
				int max = allowLargeSize ? prefabs.Count : (allowMediumSize ? prefabs.Count-1 : 2);
				Pod p = li[UnityEngine.Random.Range(0, max)];
				double dh = go.transform.position.y-referenceY;
				go = ObjectUtil.replaceObject(go, p.prefab);
				hoff = p.vineBaseOffset-old.vineBaseOffset;
			}
			if (randomHeight) {
				Pod p = prefabs[id];
				double maxSink = Math.Min(p.maximumSink*maxSinkFraction, groundThickness);
				float sink = UnityEngine.Random.Range(0F, (float)maxSink);
				double newY = referenceY+p.vineBaseOffset-sink;
				hoff = newY-go.transform.position.y;
			}
			if (Math.Abs(hoff) > 0.01) {
				go.transform.position = (go.transform.position-new Vector3(0, (float)hoff, 0));
				SNUtil.writeToChat(id+" > "+hoff);
			}
		}
		
		internal override void applyToObject(PlacedObject go) {
			double hoff = 0;
			if (randomType) {
				Pod old = prefabs[go.prefabName];
				List<Pod> li = new List<Pod>(prefabs.Values);
				int max = allowLargeSize ? prefabs.Count : (allowMediumSize ? prefabs.Count-1 : 2);
				Pod p = li[UnityEngine.Random.Range(0, max)];
				double dh = go.obj.transform.position.y-referenceY;
				go.replaceObject(p.prefab);
				hoff = p.vineBaseOffset-old.vineBaseOffset;
			}
			if (randomHeight) {
				Pod p = prefabs[go.prefabName];
				double maxSink = Math.Min(p.maximumSink*maxSinkFraction, groundThickness);
				float sink = UnityEngine.Random.Range(0F, (float)maxSink);
				double newY = referenceY+p.vineBaseOffset-sink;
				hoff = newY-go.position.y;
			}
			if (Math.Abs(hoff) > 0.01) {
				go.move(0, hoff, 0);
				SNUtil.writeToChat(go.prefabName+" > "+hoff);
			}
		}
		
		internal override void loadFromXML(XmlElement e) {
			XmlElement type;
			randomType = e.getBoolean("randomType", out type);
			allowMediumSize = randomType && bool.Parse(type.GetAttribute("medium"));
			allowLargeSize = randomType && bool.Parse(type.GetAttribute("large"));
			
			randomHeight = e.getBoolean("randomHeight");
			referenceY = e.getFloat("referenceY", double.NaN);
			groundThickness = e.getFloat("groundThickness", double.NaN);
			maxSinkFraction = e.getFloat("maxSinkFraction", 1);
		}
		
		internal override void saveToXML(XmlElement e) {
			XmlElement prop = e.addProperty("randomType", randomType);
			prop.SetAttribute("medium", allowMediumSize.ToString());
			prop.SetAttribute("large", allowLargeSize.ToString());
			
			e.addProperty("randomHeight", randomHeight);
			e.addProperty("referenceY", referenceY);
			e.addProperty("groundThickness", groundThickness);
			e.addProperty("maxSinkFraction", maxSinkFraction);
		}
		
		private class Pod {
			
			internal readonly string prefab;
			internal readonly double vineBaseOffset; //amount needed to rise to only just embed, always > 0
			internal readonly double maximumSink; //further sinkability from @ vineBaseOffset, always > 0
			
			internal Pod(string pfb, double y, double ym) {
				prefab = pfb;
				vineBaseOffset = y;
				maximumSink = -ym-y;
			}
			
		}
		
	}
}

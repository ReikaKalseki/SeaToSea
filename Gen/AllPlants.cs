﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class AllPlants : WorldGenerator {
		
		private static double step = 25;
		private double offsetX = -80;
		private double offsetZ = -step;
		private int index = 0;
		
		public AllPlants(Vector3 pos) : base(pos) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override void generate() {			
			GameObject rock = PlacedObject.createWorldObject("a474e5fa-1552-4cea-abdb-945f85ed4b1a");
			rock.transform.position = position;
			rock.transform.localScale = new Vector3(150, 1, 40);
			
			foreach (FieldInfo f in typeof(VanillaFlora).GetFields()) {
				if (f.IsStatic && f.FieldType == typeof(VanillaFlora)) {
					VanillaFlora vf = (VanillaFlora)f.GetValue(null);
					spawnPlant(vf);
					if (vf.maximumSink > 0.01) {
						spawnPlant(vf, vf.maximumSink);
					}
				}
			}
		}
		
		private void spawnPlant(VanillaFlora f, double sink = 0) {
			offsetZ = ((index%3)-1)*step;
			GameObject go = PlacedObject.createWorldObject(f.getRandomPrefab(false));
			double d = f.baseOffset < -99 ? 6 : f.baseOffset;
			d -= sink;
			go.transform.position = new Vector3(position.x+(float)offsetX, position.y+(float)d, position.z+(float)offsetZ);
			index++;
			if (index%3 == 2)
				offsetX += 8;
		}
	}
}

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
		[Serializable]
		//[ProtoContract]
		//[ProtoInclude(30000, typeof(WaveBob))]
		internal class WaveBob : MonoBehaviour {
			
			public Vector3 rootPosition = Vector3.zero;
			public double speed = 0.05;
			public double amplitude = 1;
			
			void Start() {
				
			}
			
			void Update() {
				double y = amplitude*Math.Sin(speed*DayNightCycle.main.timePassedAsDouble);
				Vector3 pos = gameObject.transform.position;
				pos.y += (float)y;
				gameObject.transform.position = pos;
			}
			
		}
}

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
using System.Linq;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{		
	internal class PipeReconnection : ManipulationBase {
		
		private readonly Vector3 data;
		
		internal PipeReconnection(Vector3 vec) {
			data = vec;
		}
		
		internal override void applyToObject(GameObject go) {
			go.EnsureComponent<PipeReconnector>().position = data;
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
	
	class PipeReconnector : MonoBehaviour {
		
		IPipeConnection pipe;
		internal Vector3 position;
		IPipeConnection connection;
		
		void Update() {
			if (pipe == null)
				pipe = gameObject.GetComponent<IPipeConnection>();
			
			if (connection == null) {
				double dist = 9999;
				List<IPipeConnection> li = new List<IPipeConnection>();
				li.AddRange(UnityEngine.Object.FindObjectsOfType<OxygenPipe>());
				li.AddRange(UnityEngine.Object.FindObjectsOfType<BasePipeConnector>());
				SNUtil.log(string.Join(",", li.Select<IPipeConnection, string>(p => p+" @ "+((MonoBehaviour)p).transform.position)));
				foreach (IPipeConnection conn in li) {
					Vector3 pos = ((MonoBehaviour)conn).transform.position;
					double dd = Vector3.Distance(pos, position);
					SNUtil.log(gameObject.transform.position+" connected to "+pos+" @ dist="+dd);
					if (connection == null || dd < dist) {
						connection = conn;
						dist = dd;
					}
				}
				if (connection != null)
					pipe.SetParent(connection);
			}
		}
		
	}
}

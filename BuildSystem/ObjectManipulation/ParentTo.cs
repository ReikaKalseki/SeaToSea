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
	internal class ParentTo : ManipulationBase {
		
		private SeekType type;
		private string seekID;
		
		public ParentTo() {
			
		}
		
		internal override void applyToObject(GameObject go) {
			GameObject find = findObject(go);
			if (find != null) {
				go.transform.parent = find.transform;
			}
		}
		
		internal sealed override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			type = (SeekType)Enum.Parse(typeof(SeekType), e.getProperty("type"));
			seekID = e.getProperty("key");
		}
		
		internal override void saveToXML(XmlElement e) {
			e.addProperty("type", Enum.GetName(typeof(SeekType), type));
			e.addProperty("key", seekID);
		}
		
		private GameObject findObject(GameObject from) {
			switch(type) {
				case SeekType.FindNearTechType:
					return findNear(from, go => go.GetComponent<TechTag>().type == SNUtil.getTechType(seekID));
				case SeekType.FindNearClassID:
					return findNear(from, go => go.GetComponent<PrefabIdentifier>().classId == seekID);
				default:
					return null;
			}
		}
		
		private GameObject findNear(GameObject from, Func<GameObject, bool> f) {
			RaycastHit[] hit = UnityEngine.Physics.SphereCastAll(from.transform.position, 4, new Vector3(1, 1, 1), 4);
			if (hit == null || hit.Length == 0)
				return null;
			foreach (RaycastHit rh in hit) {
				if (rh.transform != null && rh.transform.gameObject != null && f(rh.transform.gameObject))
					return rh.transform.gameObject;
			}
			return null;
		}
		
	}
	
	internal enum SeekType {
		FindNearTechType,
		FindNearClassID,	
	}
}

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
	internal abstract class ModifyComponent<T> : ManipulationBase where T : Component {
				
		internal override sealed void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
				
		internal override sealed void applyToObject(GameObject go) {
			T component = go.GetComponentInParent<T>();
			if (component != null)
				modifyComponent(component);
		}
		
		internal abstract void modifyComponent(T component);
		
	}
}

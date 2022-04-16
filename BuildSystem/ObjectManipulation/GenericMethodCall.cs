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
using System.Reflection;
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
	internal abstract class GenericMethodCall : ManipulationBase {
		
		private MethodInfo call;
				
		internal override void applyToObject(PlacedObject go) {
			call.Invoke(null, new object[]{go});
		}
		
		internal override void loadFromXML(XmlElement e) {
			string tn = e.getProperty("typeName");
			string name = e.getProperty("name");
			Type t = InstructionHandlers.getTypeBySimpleName(tn);
			//call = t.GetMethod(name, unchecked((System.Reflection.BindingFlags)0x7fffffff));
			call = t.GetMethod(name, new Type[]{typeof(PlacedObject)});
		}
		
		internal override void saveToXML(XmlElement e) {
			e.addProperty("typeName", call.DeclaringType.Name);
			e.addProperty("name", call.Name);
		}
		
	}
}

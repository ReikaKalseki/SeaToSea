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
	internal class SetACUStack : ManipulationBase {
		
		private bool isBottomOfStack;
		private bool isTopOfStack;
		private bool glassTop;
		
		internal override void applyToObject(GameObject go) {
			GameObject floor = ObjectUtil.getChildObject(go, "BaseWaterParkFloorBottom");
			GameObject middleBottom = ObjectUtil.getChildObject(go, "BaseWaterParkFloorMiddle");
			GameObject middleTop = ObjectUtil.getChildObject(go, "BaseWaterParkCeilingMiddle");
			GameObject ceiling = ObjectUtil.getChildObject(go, "BaseWaterParkCeilingTop");
			GameObject gt = ObjectUtil.getChildObject(go, "BaseWaterParkCeilingGlass");
			floor.SetActive(isBottomOfStack);
			middleBottom.SetActive(!isBottomOfStack);
			ceiling.SetActive(isTopOfStack);
			middleTop.SetActive(!isTopOfStack);
			gt.SetActive(isTopOfStack && glassTop);
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			isBottomOfStack = e.getBoolean("Bottom");
			isTopOfStack = e.getBoolean("Top");
			glassTop = e.getBoolean("GlassTop");
		}
		
		internal override void saveToXML(XmlElement e) {
			e.addProperty("Bottom", isBottomOfStack);
			e.addProperty("Top", isTopOfStack);
			e.addProperty("GlassTop", glassTop);
		}
		
	}
}

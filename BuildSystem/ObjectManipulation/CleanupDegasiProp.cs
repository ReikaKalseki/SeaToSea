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
	internal class CleanupDegasiProp : SwapTexture {
		
		bool removeLight = false;
				
		public CleanupDegasiProp() {
			init();
		}
		
		internal override void applyToObject(GameObject go) {
			base.applyToObject(go);
			
			ObjectUtil.removeChildObject(go, "BaseCell/Coral");
			ObjectUtil.removeChildObject(go, "BaseCell/Decals");
			
		 	if (removeLight)
				ObjectUtil.removeChildObject(go, "tech_light_deco");
		}
		
		private void init() {
			addSwap("Base_abandoned_Foundation_Platform_01" ,"Base_Foundation_Platform_01");
			addSwap("Base_abandoned_Foundation_Platform_01_normal", "Base_Foundation_Platform_01_normal");
			addSwap("Base_abandoned_Foundation_Platform_01_illum", "Base_Foundation_Platform_01_illum");
		}
		
		internal override void loadFromXML(XmlElement e) {
			base.loadFromXML(e);
			
			bool.TryParse(e.InnerText, out removeLight);
			
			init();
		}
		
		protected override Texture2D getTexture(string name, string texType) {
			GameObject go = Base.pieces[(int)Base.Piece.Foundation].prefab.gameObject;
			go = ObjectUtil.getChildObject(go, "models/BaseFoundationPlatform");
			return (Texture2D)RenderUtil.extractTexture(go, texType);
		}
		
	}
}

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
	internal sealed class AddGravity : ManipulationBase {
		
		internal override void applyToObject(GameObject go) {/*
			WorldForces wf = go.EnsureComponent<WorldForces>();
			wf.enabled = true;
			wf.handleDrag = true;
			wf.handleGravity = true;
			wf.aboveWaterGravity = 9.81F;
			wf.underwaterGravity = 2;
			wf.underwaterDrag = 0.5F;
			Rigidbody rb = go.EnsureComponent<Rigidbody>();
			rb.constraints = RigidbodyConstraints.None;
			rb.useGravity = false;//true;
			rb.detectCollisions = true;
			rb.drag = 0.5F;
			rb.angularDrag = 0.05F;
			rb.centerOfMass = new Vector3(0, 0.5F, 0);
			rb.inertiaTensor = new Vector3(0.2F, 0, 0.2F);
			wf.Awake();
			rb.WakeUp();*/
			SBUtil.applyGravity(go);
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			
		}
		
		internal override void saveToXML(XmlElement e) {
			
		}
		
	}
}

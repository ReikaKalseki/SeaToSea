using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	internal class Magnetic : MonoBehaviour {

		private Rigidbody body;
		private Vehicle vehicle;

		private bool searched = false;

		void FixedUpdate() {
			if (C2CHooks.skipMagnetic)
				return;
			if (!searched) {
				try {
					if (!body)
						body = this.GetComponentInChildren<Rigidbody>();
					if (!vehicle)
						vehicle = this.GetComponentInChildren<Vehicle>();
				}
				catch (Exception e) {
					SNUtil.log("Magnetic threw exception on search: " + e, SeaToSeaMod.modDLL);
				}
				searched = true;
			}
			float dT = Time.deltaTime;
			if (dT > 0 && body && !body.isKinematic && !vehicle) {
				HashSet<Magnetic> set = WorldUtil.getObjectsNearWithComponent<Magnetic>(transform.position, 18);
				foreach (Magnetic m in set) {
					attract(this, m, dT);
				}
			}
		}

		private static void attract(Magnetic m1, Magnetic m2, float dT) {
			Vector3 diff = m2.transform.position-m1.transform.position;
			float dist = diff.sqrMagnitude;
			diff = diff.normalized;
			float mag = 240F*dT/Mathf.Max(0.1F, dist);
			if (m1.body && !m1.body.isKinematic)
				m1.body.AddForce(diff.setLength(mag), ForceMode.Force);
			if (m2.body && !m2.body.isKinematic)
				m2.body.AddForce(-diff.setLength(mag), ForceMode.Force);
		}

	}
}

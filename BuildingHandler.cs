/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using UnityEngine;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public class BuildingHandler
	{
		public static readonly BuildingHandler instance = new BuildingHandler();
		
		private GameObject lastPlaced;
		private List<Selection> selected = new List<Selection>();
		
		private BuildingHandler()
		{
			
		}
		
		private class Selection {
			
			private static GameObject bubblePrefab = null;
			
			internal readonly GameObject obj;
			internal readonly GameObject fx;
			
			internal Selection(GameObject go) {
				obj = go;
				
				GameObject bubb = null;
				if (bubblePrefab == null) {
					if (!UWE.PrefabDatabase.TryGetPrefab("fca5cdd9-1d00-4430-8836-a747627cdb2f", out bubblePrefab)) {
						SBUtil.writeToChat("Bubbles not found.");
					}
				}
				else {
					Vector3 pos = go.transform.position;
					bubb = Utils.SpawnFromPrefab(bubblePrefab, null);
					bubb.transform.position = pos;
				}
				fx = bubb;
			}
			
		}
		
		public void highlightSelected() {

		}
		
		public void selectLooked(bool clearSel = true) {
			if (clearSel)
				clearSelection();
			GameObject found = null;
			float dist;
			Targeting.GetTarget(Player.main.gameObject, 40, out found, out dist);
			if (found != null) {
				//SBUtil.writeToChat("Selected "+found+" @ "+found.transform.position);
				TechType tech;
				GameObject use;
				if (Targeting.GetRoot(found, out tech, out use)) {
					if (use != null)
						select(use);
					SBUtil.writeToChat("Selected "+found+" @ "+found.transform.position);
				}
			}
			else {
				SBUtil.writeToChat("Raytrace found nothing.");
			}
		}
		
		public void selectLastPlaced() {
			if (lastPlaced != null)
				select(lastPlaced);
		}
		
		public void selectViaClick() {
			//TODO
		}
		
		public void select(GameObject go) {
			foreach (Selection s in selected) {
				if (s.obj == go)
					return;
			}
			selected.Add(new Selection(go));
		}
		
		public void deselect(GameObject go) {
			foreach (Selection s in selected) {
				if (s.obj == go)
					deselect(s);
			}
		}
		
		private void deselect(Selection go) {
			selected.Remove(go);
			GameObject.Destroy(go.fx);
		}
		
		public void clearSelection() {
			while (selected.Count > 0) {
				deselect(selected[0]);
			}
		}
		
		public void moveSelected(Vector3 mov) {
			foreach (Selection go in selected) {
				Vector3 vec = go.obj.transform.position;
				vec.x += mov.x;
				vec.y += mov.y;
				vec.z += mov.z;
				go.obj.transform.position = vec;
				SBUtil.writeToChat(go.obj.transform.position.ToString());
			}
		}
		
		public void rotateSelectedYaw(double ang) {
			rotateSelected(0, ang, 0);
		}
		
		public void rotateSelected(double roll, double yaw, double pitch) {
			foreach (Selection go in selected) {
				Vector3 euler = go.obj.transform.rotation.eulerAngles;
				go.obj.transform.rotation = Quaternion.Euler(euler.x+(float)roll, euler.y+(float)yaw, euler.z+(float)pitch);
				SBUtil.writeToChat(go.obj.transform.rotation.eulerAngles.ToString());
			}
		}
		
		public void spawnPrefabAtLook(string[] args) {
			spawnPrefabAtLook(args[0]);
		}
    
	    public void spawnPrefabAtLook(string id) {
	    	Transform transform = MainCamera.camera.transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			Vector3 pos = position+(forward.normalized*7.5F);
			GameObject prefab;
			if (UWE.PrefabDatabase.TryGetPrefab(id, out prefab)) {
				GameObject go = GameObject.Instantiate(prefab);
				go.SetActive(true);
				go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, 0));
				SBUtil.writeToChat("Spawned a "+PrefabData.getPrefab(id)+" at "+pos);
				SBUtil.log("Spawned a "+PrefabData.getPrefab(id)+" at "+pos);
				lastPlaced = go;
				selectLastPlaced();
			}
			else {
				SBUtil.writeToChat("Prefab not placed.");
				SBUtil.log("Prefab not placed.");
			}
	    }
	}
}

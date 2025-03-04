using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class KeypadCodeSwappingSystem {
		
		private static readonly string CAPTAIN_DOOR = "19feccc5-36a0-431c-ae97-16f87c21d5af";
		
		public static readonly KeypadCodeSwappingSystem instance = new KeypadCodeSwappingSystem();
		
		private readonly Dictionary<string, CodeSwap> data = new Dictionary<string, CodeSwap>();
		
		private readonly Vector3 door = new Vector3(978.9F, 11.2F, -68.6F);
		
		private readonly PDAManager.PDAPage captainCodeRedo;
		
		private KeypadCodeSwappingSystem() {
			addCodeSwap("48a5564b-e632-4666-9e7c-f377fbc4fd23", "Aurora_Office_PDA1", "1454"); //cargo bay
			addCodeSwap("3265d800-9ae0-478c-973c-ddf5351977c0", "Aurora_Locker_PDA2", "1869"); //sweet offer
			addCodeSwap(CAPTAIN_DOOR, /*"CaptainCode"*/null, "2679");
			addCodeSwap("38135f4d-5f31-4438-abce-2c8bbbc5c77c", "Aurora_RingRoom_Code_PDA", "6483"); //lab
			captainCodeRedo = PDAManager.getPage("captaindoor");
		}
		
		private void addCodeSwap(string pfb, string ency, string old) {
			data[pfb] = new CodeSwap(pfb, ency, old);
		}
		
		public void onCodeFailed(KeypadDoorConsole con) {
			if (con.gameObject.FindAncestor<PrefabIdentifier>().ClassId == "19feccc5-36a0-431c-ae97-16f87c21d5af" && con.numberField.text == data[CAPTAIN_DOOR].oldCode) {
				captainCodeRedo.unlock();
			}
		}
		
		internal void handleDoor(PrefabIdentifier pi) {
			if (data.ContainsKey(pi.ClassId)) {
				CodeSwap dd = data[pi.ClassId];
				string code = dd.getRandomizedDoorCode();
				foreach(KeypadDoorConsole pad in pi.GetComponentsInChildren<KeypadDoorConsole>())
					pad.accessCode = code;
		    	SNUtil.log("Swapping code on "+pi.name+" @ "+pi.transform.position+": "+dd.oldCode+" > "+code+" in "+dd.encyKey);
		    	/*
		    	if (pi.ClassId == CAPTAIN_DOOR) {
		    		StarshipDoor s = pi.GetComponentInChildren<StarshipDoor>();
		    		GameObject go = s.gameObject;
		    		ObjectUtil.removeComponent<StarshipDoorLocked>(go); //has to be removed first - locked depends on door and will prevent removal
		    		ObjectUtil.removeComponent<StarshipDoor>(go);
		    		ObjectUtil.removeComponent<ImmuneToPropulsioncannon>(go);
		    		Rigidbody rb = go.GetComponent<Rigidbody>();
		    		rb.isKinematic = true;
		    		rb.mass = 500; //default limit is 1300 and azurite makes x6
		    		go.EnsureComponent<CaptainDoor>();
		    	}*/
			}
		}
		
		internal void patchEncyPages() {
			foreach (CodeSwap c in data.Values) {
				if (c.encyKey == null)
					continue;
				string key = "EncyDesc_"+c.encyKey;
				string code = c.getRandomizedDoorCode();
				CustomLocaleKeyDatabase.registerKey(key, Language.main.Get(key).Replace(c.oldCode, code));
		    	SNUtil.log("Swapping code in ency entry "+c.encyKey+": "+c.oldCode+" > "+c);
			}
		}
		
		class CodeSwap {
			
			public readonly string prefab;
			public readonly string encyKey;
			public readonly string oldCode;
			
			internal CodeSwap(string pfb, string ency, string old) {
				prefab = pfb;
				encyKey = ency;
				oldCode = old;
			}
			
			public override string ToString()
			{
				return string.Format("[CodeSwap Prefab={0}, EncyKey={1}, OldCode={2}]", prefab, encyKey, oldCode);
			}
	    
		    internal string getRandomizedDoorCode() {
				if (prefab == CAPTAIN_DOOR)
					return "0000"; //impossible to enter
				UnityEngine.Random.InitState(SNUtil.getWorldSeedInt() ^ encyKey.GetHashCode());
		    	UnityEngine.Random.Range(0, 1);
		    	string ret = "";
		    	while (ret.Length < 4)
		    		ret += (char)UnityEngine.Random.Range('1', '9'+1);
		    	return ret;
		    }


			
		}
	}
	
}

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
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class BloodKelpBaseNuclearReactorMelter : Spawnable {
	        
		internal BloodKelpBaseNuclearReactorMelter() : base("BloodKelpBaseNuclearReactorMelter", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = new GameObject();
			go.EnsureComponent<BloodKelpBaseNuclearReactorMelterTag>();
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			return go;
	    }
		
		class BloodKelpBaseNuclearReactorMelterTag : MonoBehaviour {
			
			void Update() {
				BaseNuclearReactorGeometry go = WorldUtil.getClosest<BaseNuclearReactorGeometry>(C2CHooks.bkelpBaseNuclearReactor);
		    	if (go && Vector3.Distance(go.transform.position, C2CHooks.bkelpBaseNuclearReactor) < 5F) {
		    		GameObject child = ObjectUtil.getChildObject(go.gameObject, "UI/Canvas/Text");
		    		child.GetComponent<Text>().text = "<color=#ff0000>OPERATOR ERROR\n\nMOLTEN CORE WARNING\nTEMP AT SPIKEVALUE \n999999999999999</color>";
		    		/*
		    		LeakingRadiation lr = go.EnsureComponent<LeakingRadiation>();
		    		lr.leaks = new List<RadiationLeak>();
		    		lr.radiationFixed = false;
		    		lr.kGrowRate = 0;
		    		lr.kNaturalDissipation = 0;
		    		lr.kStartRadius = lr.kMaxRadius = lr.currentRadius = 9;
		    		lr.damagePlayerInRadius = go.EnsureComponent<DamagePlayerInRadius>();
		    		lr.damagePlayerInRadius.damageType = DamageType.Radiation;
		    		lr.damagePlayerInRadius.damageAmount = 3;
		    		lr.radiatePlayerInRange = go.EnsureComponent<RadiatePlayerInRange>();
		    		*/
		    		go.gameObject.EnsureComponent<BloodKelpBaseNuclearReactorGlower>();
		    	}
			}
			
		}
	    
	    class BloodKelpBaseNuclearReactorGlower : MonoBehaviour {
	    	
	    	private bool textured;
		
			private readonly List<ParticleSystem> bubbles = new List<ParticleSystem>();
	    	
			private void Update() {
	    		if (!textured) {
	    			textured = true;
	    			foreach (Renderer r in GetComponentsInChildren<Renderer>())
	    				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/bkelpreactor");
	    		}
			
				while (bubbles.Count < 11) {
					GameObject go = ObjectUtil.createWorldObject("0dbd3431-62cc-4dd2-82d5-7d60c71a9edf");
					go.transform.SetParent(transform);
					float y = UnityEngine.Random.Range(-0.2F, 1.2F);
					float r = 0.8F;
					if (y < 0.2)
						r += y*0.33F;
					float ang = UnityEngine.Random.Range(0F, 360F)*Mathf.PI/180F;
					go.transform.localPosition = new Vector3(r*Mathf.Cos(ang), -y, r*Mathf.Sin(ang));
					go.transform.rotation = Quaternion.Euler(270, 0, 0); //not local - force to always be up
					ParticleSystem ps = go.GetComponent<ParticleSystem>();
					go.SetActive(true);
					bubbles.Add(ps);
					ps.Play();
				}
			}
	    	
	    }
			
	}
}

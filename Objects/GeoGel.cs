using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class GeoGel : Spawnable {

		public readonly bool isDrip;

		internal GeoGel(XMLLocale.LocaleEntry e, bool drip) : base(e.key + "_" + drip, e.name, e.desc) {
			isDrip = drip;
		}

		protected override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/Geogel");
		}

		public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782");
			GasPod gp = world.GetComponent<GasPod>();
			if (isDrip) {
				Renderer r = world.GetComponentInChildren<Renderer>();
				r.transform.localScale *= 0.33F;
				r.materials[0].SetColor("_Color", new Color(0.1F, 0.35F, 0.5F)/*new Color(0.15F, 0.1F, 0.1F)*/);
				world.removeComponent<Pickupable>();
				//world.removeComponent<Collider>();
				foreach (Collider c in world.GetComponentsInChildren<Collider>())
					c.enabled = false;
			}
			else { //switch render to enzyme 42
				GameObject pfb = ObjectUtil.lookupPrefab("505e7eff-46b3-4ad2-84e1-0fadb7be306c");
				Renderer r = world.GetComponentInChildren<Renderer>();
				GameObject mdl = UnityEngine.Object.Instantiate(pfb.GetComponentInChildren<Animator>().gameObject);
				mdl.removeChildObject("root", false);
				mdl.transform.SetParent(r.transform.parent);
				mdl.transform.localPosition = r.transform.localPosition;
				gp.model.SetActive(false);
				r = mdl.GetComponentInChildren<Renderer>();
				Color c = new Color(0.2F, 0.67F, 0.95F);//new Color(0.4F, 0.3F, 0.1F);
				r.materials[0].SetColor("_Color", c);
				r.materials[0].SetColor("_SpecColor", c);
				r.materials[0].SetFloat("_Fresnel", 0F);
				r.materials[0].SetFloat("_Shininess", 5F);
				r.materials[0].SetFloat("_SpecInt", 1.5F);
				r.materials[0].SetFloat("_EmissionLM", 200F);
				r.materials[0].SetFloat("_EmissionLMNight", 200F);
				r.materials[0].SetFloat("_MyCullVariable", 1.6F);
				Light l = world.addLight(2, 4, c);
				Light l2 = world.addLight(0.67F, 15, c);
			}
			foreach (FMOD_StudioEventEmitter mod in world.GetComponents<FMOD_StudioEventEmitter>()) {
				mod.path = ""; //prevent playback
			}
			world.removeComponent<UWE.TriggerStayTracker>(isDrip);
			world.removeComponent<FMOD_StudioEventEmitter>(isDrip);
			world.removeComponent<FMOD_CustomEmitter>(isDrip);
			world.SetActive(false);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			gp.autoDetonateTime = isDrip ? 1 : 90;
			//make exploding oil not cause harm
			gp.gasEffectPrefab = ObjectUtil.lookupPrefab(isDrip ? SeaToSeaMod.geogelFogDrip.ClassID : SeaToSeaMod.geogelFog.ClassID);
			gp.damagePerSecond = 0;
			gp.damageRadius = 0;
			gp.damageInterval = 9999999999;
			gp.smokeDuration = 5F; //from 15
			return world;
		}

	}
}

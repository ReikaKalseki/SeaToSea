using System;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class VoidSpikeWreck : WorldGenerator {
		
		private static readonly List<Prop> pieces = new List<Prop>();
		private static readonly List<Prop> items = new List<Prop>();
		
		static VoidSpikeWreck() {
			pieces.Add(new Prop("e600a1f4-83df-447d-80ab-e3f4ec074b32", new float[]{-90}, 0.25F)); //max tank
			pieces.Add(new Prop("68462082-f714-4b5e-8d0d-623d2ec6058f", new float[]{0, 180}, 0.25F)); //broken seaglide
			pieces.Add(new Prop("0cb9b6b4-5f39-49f2-821e-6490829dad4b", new float[]{0, 180}, 0.25F)); //broken terraformer
			//pieces.Add(new Prop()); //storage cube
			//pieces.Add(new Prop("12c95e66-fb54-47b3-87f1-8e318394b839", null, 0.1F));	//flashlight
			pieces.Add(new Prop("7c1aa35f-759e-4861-a871-f58843698298", new float[]{0, 180}, 0.2F)); //broken stasis rifle
			//pieces.Add(new Prop("d4bfebc0-a5e6-47d3-b4a7-d5e47f614ed6"));	//battery
			//pieces.Add(new Prop("fde8c0c0-7588-4d0b-b24f-4632315bd86c"));	//pathfinder
			//pieces.Add(new Prop("9ef36033-b60c-4f8b-8c3a-b15035de3116", null, 0.4F)); //repair tool
			pieces.Add(new Prop("f4146f7a-d334-404a-abdc-dff98365eb10", new float[]{-90, 90}, 0.2F)); //broken transfuser
			
			items.Add(new Prop("bc70e8c8-f750-4c8e-81c1-4884fe1af34e", new float[]{0}, 0.05F)); //first aid
			items.Add(new Prop("30373750-1292-4034-9797-387cf576d150", new float[]{0}, 0.05F)); //nutrient
			items.Add(new Prop("22b0ce08-61c9-4442-a83d-ba7fb99f26b0", new float[]{0}, 0.15F)); //water
		}
		
		public VoidSpikeWreck(Vector3 pos) : base(pos) {
			spawner = VoidSpikesBiome.spawnEntity;
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override void generate(List<GameObject> li) {
			SBUtil.log("Generating void spike deep debris @ "+position);
			
			GameObject platform = spawner("255ed3c3-1973-40c0-9917-d16dd9a7018d");
			platform.transform.position = position+Vector3.down*0.1F;
			platform.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360F), 0);
			platform.transform.localScale = Vector3.one*0.72F;
			new CleanupDegasiProp().applyToObject(platform);
			li.Add(platform);
			
			Vector3 refPos = platform.transform.position+Vector3.up*0.85F;
			refPos = MathUtil.getRandomVectorAround(refPos, new Vector3(0.5F, 0, 0.5F));
			
			GameObject pda = spawner("0f1dd54e-b36e-40ca-aa85-d01df1e3e426");
			pda.transform.position = refPos+Vector3.up*0.2F;
			pda.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360F), 0);
			SBUtil.setPDAPage(pda.EnsureComponent<StoryHandTarget>(), PDAManager.getPage("voidspike"));
			li.Add(pda);
			
			GameObject bag = spawner("3616e7f3-5079-443d-85b4-9ad68fcbd924");
			StorageContainer con = bag.GetComponentInChildren<StorageContainer>();
			bag.transform.position = MathUtil.getRandomVectorAround(refPos, 1, 1.5F)+Vector3.up*0.15F;
			bag.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360F), 0);
			foreach (Prop s in items) {
				//SBUtil.writeToChat("Added "+s);
				for (int i = 0; i < 2; i++) {
					GameObject item = spawner(s.prefab);
					item.SetActive(false);
					SBUtil.refillItem(item);
					con.container.AddItem(item.GetComponent<Pickupable>());
					//UnityEngine.Object.Destroy(item);
				}
			}
			li.Add(bag);
			
			GameObject go = spawner("12c95e66-fb54-47b3-87f1-8e318394b839");//flashlight
			go.SetActive(false);
			SBUtil.refillItem(go);
			con.container.AddItem(go.GetComponent<Pickupable>());
			go = spawner("9ef36033-b60c-4f8b-8c3a-b15035de3116"); //repair tool
			go.SetActive(false);
			SBUtil.refillItem(go);
			con.container.AddItem(go.GetComponent<Pickupable>());
			
			foreach (Prop s in pieces) {
				li.Add(generateObjectInRange(refPos, 3.5F, s));
			}
		}
		
		private GameObject generateObjectInRange(Vector3 refPos, float r, Prop p) {
			float tilt = 0;
			if (p.freeAngle) {
				tilt = UnityEngine.Random.Range(0, 360F);
			}
			else {
				tilt = p.baseAngles[UnityEngine.Random.Range(0, p.baseAngles.Length)];
				tilt = UnityEngine.Random.Range(tilt-5F, tilt+5F);
			}
			return generateObjectInRange(refPos, r, p.prefab, p.yOffset, tilt);
		}
		
		private GameObject generateObjectInRange(Vector3 refPos, float r, string pfb, float y = 0, float tilt = 0) {
			GameObject go = spawner(pfb);
			if (go == null)
				return go;
			float ang = UnityEngine.Random.Range(0, 360F);
			float cos = (float)Math.Cos(ang*Math.PI/180D);
			float sin = (float)Math.Sin(ang*Math.PI/180D);
			Vector3 pos = refPos+UnityEngine.Random.Range(1.5F, r)*new Vector3(cos, 0, sin);
			pos.y = pos.y+(float)y;
			go.transform.position = pos;
			go.transform.rotation = Quaternion.Euler(tilt, UnityEngine.Random.Range(0, 360F), 0);/*
			WorldForces wf = go.EnsureComponent<WorldForces>();
			wf.enabled = true;
			wf.handleDrag = true;
			wf.useRigidbody = go.GetComponentInChildren<Rigidbody>();
			wf.handleGravity = true;
			wf.underwaterGravity = 1;*/
			foreach (WorldForces wf in go.GetComponentsInChildren<WorldForces>()) {
				//UnityEngine.Object.Destroy(wf);
				wf.handleDrag = wf.handleGravity = false;
				wf.underwaterGravity = wf.aboveWaterGravity = 0;
				wf.aboveWaterDrag = wf.underwaterDrag = 0;
			}
			Rigidbody b = go.GetComponentInChildren<Rigidbody>();
			//b.detectCollisions = false;
			b.constraints = RigidbodyConstraints.FreezeAll;
			SBUtil.refillItem(go);
			return go;
		}
		
		private class Prop {
			
			internal readonly string prefab;
			internal readonly float[] baseAngles;
			internal readonly bool freeAngle;
			internal readonly float yOffset;
			
			internal Prop(string pfb, float ang1, float ang2) : this(pfb, new float[]{ang1, ang2}) {
				
			}
			
			internal Prop(string pfb, float ang = 0) : this(pfb, new float[]{ang}) {
				
			}
			
			internal Prop(string pfb, float[] ang, float y = 0) {
				prefab = pfb;
				yOffset = y;
				baseAngles = ang;
				freeAngle = baseAngles == null || baseAngles.Length == 0;
			}
			
		}
	}
}

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
		private static readonly List<string> items = new List<string>();
		
		static VoidSpikeWreck() {
			pieces.Add(new Prop("e600a1f4-83df-447d-80ab-e3f4ec074b32", -90, 90)); //max tank
			pieces.Add(new Prop("68462082-f714-4b5e-8d0d-623d2ec6058f")); //broken seaglide
			pieces.Add(new Prop("0cb9b6b4-5f39-49f2-821e-6490829dad4b")); //broken terraformer
			//pieces.Add(new Prop()); //storage cube
			pieces.Add(new Prop("12c95e66-fb54-47b3-87f1-8e318394b839"));	//flashlight
			pieces.Add(new Prop("7c1aa35f-759e-4861-a871-f58843698298")); //broken stasis rifle
			//pieces.Add(new Prop("d4bfebc0-a5e6-47d3-b4a7-d5e47f614ed6"));	//battery
			//pieces.Add(new Prop("fde8c0c0-7588-4d0b-b24f-4632315bd86c"));	//pathfinder
			pieces.Add(new Prop("9ef36033-b60c-4f8b-8c3a-b15035de3116")); //repair tool
			pieces.Add(new Prop("f4146f7a-d334-404a-abdc-dff98365eb10")); //broken transfuser
			
			items.Add("bc70e8c8-f750-4c8e-81c1-4884fe1af34e"); //first aid
			items.Add("30373750-1292-4034-9797-387cf576d150"); //nutrient
			items.Add("22b0ce08-61c9-4442-a83d-ba7fb99f26b0"); //water
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
			foreach (Prop s in pieces) {
				li.Add(generateObjectInRange(4.5F, s));
			}
			foreach (string s in items) {
				li.Add(generateObjectInRange(3, s, 0.1F));
				li.Add(generateObjectInRange(3, s, 0.1F));
			}
			
			GameObject pda = spawner("0f1dd54e-b36e-40ca-aa85-d01df1e3e426");
			pda.transform.position = position;
			pda.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360F), 0);
			SBUtil.setPDAPage(pda.EnsureComponent<StoryHandTarget>(), VoidSpikesBiome.instance.getPDA());
			li.Add(pda);
		}
		
		private GameObject generateObjectInRange(float r, Prop p) {
			float tilt = 0;
			if (p.freeAngle) {
				tilt = UnityEngine.Random.Range(0, 360F);
			}
			else {
				tilt = p.baseAngles[UnityEngine.Random.Range(0, p.baseAngles.Length)];
				tilt = UnityEngine.Random.Range(tilt-15F, tilt+15F);
			}
			return generateObjectInRange(r, p.prefab, p.yOffset, tilt);
		}
		
		private GameObject generateObjectInRange(float r, string pfb, float y = 0, float tilt = 0) {
			GameObject go = spawner(pfb);
			if (go == null)
				return go;
			float ang = UnityEngine.Random.Range(0, 360F);
			float cos = (float)Math.Cos(ang*Math.PI/180D);
			float sin = (float)Math.Sin(ang*Math.PI/180D);
			Vector3 pos = position+UnityEngine.Random.Range(0, r)*new Vector3(cos, 0, sin);
			pos.y = pos.y+(float)y;
			go.transform.position = pos;
			go.transform.rotation = Quaternion.Euler(tilt, UnityEngine.Random.Range(0, 360F), 0);
			WorldForces wf = go.EnsureComponent<WorldForces>();
			wf.enabled = true;
			wf.handleGravity = true;
			wf.underwaterGravity = 1;
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

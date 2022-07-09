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
		
		private static readonly List<VoidWreckProp> pieces = new List<VoidWreckProp>();
		private static readonly List<VoidWreckProp> items = new List<VoidWreckProp>();
		
		private static readonly string platformPrefab;
		
		static VoidSpikeWreck() {
			pieces.Add(new VoidWreckProp("e600a1f4-83df-447d-80ab-e3f4ec074b32", new float[]{-90}, 0.25F)); //max tank
			pieces.Add(new VoidWreckProp("68462082-f714-4b5e-8d0d-623d2ec6058f", new float[]{0, 180}, 0.25F)); //broken seaglide
			pieces.Add(new VoidWreckProp("0cb9b6b4-5f39-49f2-821e-6490829dad4b", new float[]{0, 180}, 0.25F)); //broken terraformer
			//pieces.Add(new Prop()); //storage cube
			//pieces.Add(new Prop("12c95e66-fb54-47b3-87f1-8e318394b839", null, 0.1F));	//flashlight
			pieces.Add(new VoidWreckProp("7c1aa35f-759e-4861-a871-f58843698298", new float[]{0, 180}, 0.2F)); //broken stasis rifle
			//pieces.Add(new Prop("d4bfebc0-a5e6-47d3-b4a7-d5e47f614ed6"));	//battery
			//pieces.Add(new Prop("fde8c0c0-7588-4d0b-b24f-4632315bd86c"));	//pathfinder
			//pieces.Add(new Prop("9ef36033-b60c-4f8b-8c3a-b15035de3116", null, 0.4F)); //repair tool
			pieces.Add(new VoidWreckProp("f4146f7a-d334-404a-abdc-dff98365eb10", new float[]{-90, 90}, 0.2F)); //broken transfuser
			
			items.Add(new VoidWreckProp("bc70e8c8-f750-4c8e-81c1-4884fe1af34e", new float[]{0}, 0.05F)); //first aid
			items.Add(new VoidWreckProp("30373750-1292-4034-9797-387cf576d150", new float[]{0}, 0.05F)); //nutrient
			items.Add(new VoidWreckProp("22b0ce08-61c9-4442-a83d-ba7fb99f26b0", new float[]{0}, 0.15F)); //water
			
			string id = "255ed3c3-1973-40c0-9917-d16dd9a7018d";
			CustomPrefab pfb = new CustomPrefab(id);
			XmlElement e = new XmlDocument().CreateElement("customprefab");
			e.InnerXml = "<prefab>"+id+"</prefab><position><x>0</x><y>0</y><z>0</z></position><objectManipulation><CleanupDegasiProp>true</CleanupDegasiProp></objectManipulation>";
			pfb.loadFromXML(e);
			platformPrefab = pfb.prefabName;
		}
		
		public VoidSpikeWreck(Vector3 pos) : base(pos) {
			spawner = VoidSpikesBiome.spawnEntity;
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override void generate(List<GameObject> li) {
			SNUtil.log("Generating void spike deep debris @ "+position);
			
			GameObject platform = spawner(platformPrefab);
			platform.transform.position = position+Vector3.down*0.1F;
			platform.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360F), 0);
			platform.transform.localScale = Vector3.one*0.72F;
			li.Add(platform);
			
			Vector3 refPos = platform.transform.position+Vector3.up*0.85F;
			refPos = MathUtil.getRandomVectorAround(refPos, new Vector3(0.5F, 0, 0.5F));
			GameObject pda = spawner(PDAManager.getPage("voidspike").getPDAClassID());
			pda.transform.position = getPDALocation();
			pda.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360F), 0);
			li.Add(pda);
			
			GameObject bag = spawner("3616e7f3-5079-443d-85b4-9ad68fcbd924");
			StorageContainer con = bag.GetComponentInChildren<StorageContainer>();
			bag.transform.position = MathUtil.getRandomVectorAround(refPos, 1, 1.5F)+Vector3.up*0.15F;
			bag.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360F), 0);
			foreach (Prop s in items) {
				//SNUtil.writeToChat("Added "+s);
				for (int i = 0; i < 2; i++) {
					GameObject item = ObjectUtil.createWorldObject(s.prefab.baseTemplate.prefab, true, false);//spawner(s.prefab.ClassID);
					item.SetActive(false);
					ObjectUtil.refillItem(item);
					con.container.AddItem(item.GetComponent<Pickupable>());
					//UnityEngine.Object.Destroy(item);
				}
			}
			li.Add(bag);
			
			GameObject go = ObjectUtil.createWorldObject("12c95e66-fb54-47b3-87f1-8e318394b839");//flashlight
			go.SetActive(false);
			ObjectUtil.refillItem(go);
			con.container.AddItem(go.GetComponent<Pickupable>());
			go = ObjectUtil.createWorldObject("9ef36033-b60c-4f8b-8c3a-b15035de3116"); //repair tool
			go.SetActive(false);
			ObjectUtil.refillItem(go);
			con.container.AddItem(go.GetComponent<Pickupable>());
			
			foreach (VoidWreckProp s in pieces) {
				li.Add(generateObjectInRange(refPos, 3.5F, s));
			}
		}
		
		public Vector3 getPDALocation() {
			return position+Vector3.up*0.95F;
		}
		
		private GameObject generateObjectInRange(Vector3 refPos, float r, VoidWreckProp p) {
			float tilt = 0;
			if (p.freeAngle) {
				tilt = UnityEngine.Random.Range(0, 360F);
			}
			else {
				tilt = p.baseAngles[UnityEngine.Random.Range(0, p.baseAngles.Length)];
				tilt = UnityEngine.Random.Range(tilt-5F, tilt+5F);
			}
			return generateObjectInRange(refPos, r, p.prefab.baseTemplate.prefab.StartsWith("e600", StringComparison.InvariantCultureIgnoreCase) ? p.prefab.baseTemplate.prefab : p.prefab.ClassID, p.yOffset, tilt);
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
			go.transform.rotation = Quaternion.Euler(tilt, UnityEngine.Random.Range(0, 360F), 0);
			ObjectUtil.refillItem(go);
			Rigidbody b = go.GetComponentInChildren<Rigidbody>();
			if (b != null) {
				b.isKinematic = true;
				b.constraints = RigidbodyConstraints.FreezeAll;
			}
			return go;
		}
		
		private class VoidWreckProp : Prop {
			
			internal readonly float yOffset;
			
			internal VoidWreckProp(string pfb, float ang1, float ang2) : this(pfb, new float[]{ang1, ang2}) {
				
			}
			
			internal VoidWreckProp(string pfb, float ang = 0) : this(pfb, new float[]{ang}) {
				
			}
			
			internal VoidWreckProp(string pfb, float[] ang, float y = 0) : base(pfb, ang) {
				yOffset = y;
			}
			
		}
	}
}

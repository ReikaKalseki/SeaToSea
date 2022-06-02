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
	public sealed class PodDebris : WorldGenerator {
		
		private static readonly WeightedRandom<Prop> debrisProps = new WeightedRandom<Prop>();
		private static readonly List<Prop> papers = new List<Prop>();
		
		static PodDebris() {
			debrisProps.addEntry(new Prop("08a95141-7c00-4d55-b582-306fa2e217ed"), 100);
			debrisProps.addEntry(new Prop("0c65ee6e-a84a-4989-a846-19eb53c13071"), 100);
			debrisProps.addEntry(new Prop("0d798a35-29e8-4ddb-b1be-9d760d3a9eb6"), 30);
			debrisProps.addEntry(new Prop("1235093d-3e84-4e98-9823-602db2e8fa5f"), 100);
			debrisProps.addEntry(new Prop("1c147fcd-f727-4404-b10e-a1f03363e5bf"), 100);
			debrisProps.addEntry(new Prop("2f56b14c-d84c-407e-ad84-eab2df2fc09b"), 50);
			debrisProps.addEntry(new Prop("314e696f-67bc-4d6c-8ce5-cf9ed7f34746"), 100);
			debrisProps.addEntry(new Prop("3981a55f-0754-466a-8932-6e245b4ef846"), 20);
			debrisProps.addEntry(new Prop("4322ded1-04ba-44eb-afe5-44b9c4112c64"), 80);
			debrisProps.addEntry(new Prop("4e8f6009-fc9c-4774-9ddc-27a6b0081dde", -90, 90), 200); //hull panel
			debrisProps.addEntry(new Prop("f901b968-5b3c-4795-8ded-82db2fa23440", null), 30); //"power cyl"
			debrisProps.addEntry(new Prop("3616e7f3-5079-443d-85b4-9ad68fcbd924", null), 20); //bag
			
			papers.Add(new Prop("32e48451-8e81-428e-9011-baca82e9cd32", null));	
			papers.Add(new Prop("b4ec5044-5519-4743-b61b-92a8b6fe4a32", null));			
			
			//big platform 5a6279e2-fab9-48c9-bcb3-fdeb02fd4ce2
		}
		
		public PodDebris(Vector3 pos) : base(pos) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override void generate(List<GameObject> li) {		
			for (int i = 0; i < 6; i++) {
				li.Add(generateObjectInRange(9, 0, 9));
			}
			for (int i = 0; i < 12; i++) {
				li.Add(generateObjectInRange(24, 0, 24));
			}
			for (int i = 0; i < 6; i++) {
				li.Add(generateObjectInRange(4, 2, 4, 2, papers[UnityEngine.Random.Range(0, papers.Count)]));
			}
		}
		
		private GameObject generateObjectInRange(float dx, float dy, float dz, double offsetY = 0, Prop type = null) {
			Prop p = type == null ? debrisProps.getRandomEntry() : type;
			GameObject go = spawner(p.prefab);
			if (go == null)
				return go;
			Vector3 pos = MathUtil.getRandomVectorAround(position, new Vector3(dx, dy, dz));
			pos.y = Math.Min(pos.y, position.y)+(float)offsetY;
			go.transform.position = pos;
			go.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 360F), UnityEngine.Random.Range(0, 360F), 0);
			SBUtil.removeComponent<MedicalCabinet>(go);
			SBUtil.removeComponent<Fabricator>(go);
			SBUtil.removeComponent<Centrifuge>(go);
			SBUtil.removeComponent<Radio>(go);
			SBUtil.removeComponent<Constructable>(go);
			PreventDeconstruction prev = go.EnsureComponent<PreventDeconstruction>();
			if (!papers.Contains(p))
				SBUtil.applyGravity(go);
			prev.enabled = true;
			prev.inEscapePod = true;
			return go;
		}
		
		private class Prop {
			
			internal readonly string prefab;
			
			internal Prop(string pfb, float ang1, float ang2) : this(pfb, new float[]{ang1, ang2}) {
				
			}
			
			internal Prop(string pfb, float ang = 0) : this(pfb, new float[]{ang}) {
				
			}
			
			internal Prop(string pfb, float[] ang) {
				prefab = pfb;
			}
			
		}
	}
}

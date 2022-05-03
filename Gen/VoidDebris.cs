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
	public sealed class VoidDebris : WorldGenerator { //TODO add pieces like medkit box; scatter paper submerged below
		
		private static readonly WeightedRandom<Prop> debrisProps = new WeightedRandom<Prop>();
		private static readonly List<Prop> alwaysPieces = new List<Prop>();
		private static readonly List<Prop> papers = new List<Prop>();
		
		static VoidDebris() {
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
			debrisProps.addEntry(new Prop("f901b968-5b3c-4795-8ded-82db2fa23440"), 30);
			debrisProps.addEntry(new Prop("3616e7f3-5079-443d-85b4-9ad68fcbd924", null), 20); //bag
						
			alwaysPieces.Add(new Prop("c0175cf7-0b6a-4a1d-938f-dad0dbb6fa06", -90, 90)); //medkit fab
			alwaysPieces.Add(new Prop("4f045c69-1539-4c53-b157-767df47c1aa6", -90, 90)); //radio lookalike
			alwaysPieces.Add(new Prop("cdade216-3d4d-4adf-901c-3a91fb3b88c4", -90, 90)); //centrifuge
			alwaysPieces.Add(new Prop("9f16d82b-11f4-4eeb-aedf-f2fa2bfca8e3", -90, 90)); //fab
			
			papers.Add(new Prop("32e48451-8e81-428e-9011-baca82e9cd32", null));		
			
			//big platform 5a6279e2-fab9-48c9-bcb3-fdeb02fd4ce2
		}
		
		public VoidDebris(Vector3 pos) : base(pos) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override void generate(List<GameObject> li) {		
			for (int i = 0; i < 9; i++) {
				li.Add(generateObjectInRange(6, 0.1F, 6));
			}
			foreach (Prop s in alwaysPieces) {
				li.Add(generateObjectInRange(9, 0.25F, 9, 0, s));
			}
			for (int i = 0; i < 14; i++) {
				li.Add(generateObjectInRange(12, 0.5F, 12));
			}
			for (int i = 0; i < 6; i++) {
				li.Add(generateObjectInRange(4, 3, 4, -2, papers[UnityEngine.Random.Range(0, papers.Count)]));
			}
		}
		
		private GameObject generateObjectInRange(float dx, float dy, float dz, double offsetY = 0, Prop type = null) {
			Prop p = type == null ? debrisProps.getRandomEntry() : type;
			float tilt = 0;
			if (p.freeAngle) {
				tilt = UnityEngine.Random.Range(0, 360F);
			}
			else {
				tilt = p.baseAngles[UnityEngine.Random.Range(0, p.baseAngles.Length)];
				tilt = UnityEngine.Random.Range(tilt-15F, tilt+15F);
			}
			GameObject go = SBUtil.createWorldObject(p.prefab);
			if (go == null)
				return go;
			Vector3 pos = MathUtil.getRandomVectorAround(position, new Vector3(dx, dy, dz));
			pos.y = Math.Min(pos.y, position.y)+(float)offsetY;
			go.transform.position = pos;
			go.transform.rotation = Quaternion.Euler(tilt, UnityEngine.Random.Range(0, 360F), 0);
			SBUtil.removeComponent<MedicalCabinet>(go);
			SBUtil.removeComponent<Fabricator>(go);
			SBUtil.removeComponent<Centrifuge>(go);
			SBUtil.removeComponent<Radio>(go);
			SBUtil.removeComponent<Constructable>(go);
			PreventDeconstruction prev = go.EnsureComponent<PreventDeconstruction>();
			prev.enabled = true;
			prev.inEscapePod = true;
			VoidSpikesBiome.checkAndAddWaveBob(go, true);
			return go;
		}
		
		private class Prop {
			
			internal readonly string prefab;
			internal readonly float[] baseAngles;
			internal readonly bool freeAngle;
			
			internal Prop(string pfb, float ang1, float ang2) : this(pfb, new float[]{ang1, ang2}) {
				
			}
			
			internal Prop(string pfb, float ang = 0) : this(pfb, new float[]{ang}) {
				
			}
			
			internal Prop(string pfb, float[] ang) {
				prefab = pfb;
				baseAngles = ang;
				freeAngle = baseAngles == null || baseAngles.Length == 0;
			}
			
		}
	}
}

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
		
		private static readonly WeightedRandom<string> debrisProps = new WeightedRandom<string>();
		private static readonly List<string> alwaysPieces = new List<string>();
		private static readonly List<string> papers = new List<string>();
		
		static PodDebris() {
			debrisProps.addEntry("08a95141-7c00-4d55-b582-306fa2e217ed", 100);
			debrisProps.addEntry("0c65ee6e-a84a-4989-a846-19eb53c13071", 100);
			debrisProps.addEntry("0d798a35-29e8-4ddb-b1be-9d760d3a9eb6", 30);
			debrisProps.addEntry("1235093d-3e84-4e98-9823-602db2e8fa5f", 100);
			debrisProps.addEntry("1c147fcd-f727-4404-b10e-a1f03363e5bf", 100);
			debrisProps.addEntry("2f56b14c-d84c-407e-ad84-eab2df2fc09b", 50);
			debrisProps.addEntry("314e696f-67bc-4d6c-8ce5-cf9ed7f34746", 100);
			debrisProps.addEntry("3981a55f-0754-466a-8932-6e245b4ef846", 20);
			debrisProps.addEntry("4322ded1-04ba-44eb-afe5-44b9c4112c64", 80);
			debrisProps.addEntry("4e8f6009-fc9c-4774-9ddc-27a6b0081dde", 200); //hull panel
			debrisProps.addEntry("f901b968-5b3c-4795-8ded-82db2fa23440", 30); //"power cyl"
			debrisProps.addEntry("3616e7f3-5079-443d-85b4-9ad68fcbd924", 20); //bag
						
			alwaysPieces.Add("c0175cf7-0b6a-4a1d-938f-dad0dbb6fa06"); //medkit fab
			alwaysPieces.Add("4f045c69-1539-4c53-b157-767df47c1aa6"); //radio lookalike
			//alwaysPieces.Add("cdade216-3d4d-4adf-901c-3a91fb3b88c4", -90, 90)); //centrifuge
			alwaysPieces.Add("9f16d82b-11f4-4eeb-aedf-f2fa2bfca8e3"); //fab
			alwaysPieces.Add("f901b968-5b3c-4795-8ded-82db2fa23440"); //"power cyl"
			
			papers.Add("32e48451-8e81-428e-9011-baca82e9cd32");	
			papers.Add("b4ec5044-5519-4743-b61b-92a8b6fe4a32");			
			
			//big platform 5a6279e2-fab9-48c9-bcb3-fdeb02fd4ce2
		}
		
		private bool generateRecognizablePieces = false;
		private int paperCount = 6;
		private float debrisAmount = 1;
		private float debrisScale = 0.5F;
		private int scrapCount = 0;
		
		public PodDebris(Vector3 pos) : base(pos) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			paperCount = e.getInt("paperCount", paperCount);
			generateRecognizablePieces = e.getBoolean("generateRecognizablePieces");
			debrisAmount = (float)e.getFloat("debrisAmount", debrisAmount);
			debrisScale = (float)e.getFloat("debrisScale", debrisScale);
			scrapCount = e.getInt("scrapCount", scrapCount);
		}
		
		public override void saveToXML(XmlElement e) {
			e.addProperty("paperCount", paperCount);
			e.addProperty("generateRecognizablePieces", generateRecognizablePieces);
			e.addProperty("debrisAmount", debrisAmount);
			e.addProperty("debrisScale", debrisScale);
			e.addProperty("scrapCount", scrapCount);
		}
		
		public override void generate(List<GameObject> li) {		
			for (int i = 0; i < 6*debrisAmount; i++) {
				li.Add(generateObjectInRange(9, 0, 9));
			}
			if (generateRecognizablePieces) {
				foreach (string s in alwaysPieces) {
					li.Add(generateObjectInRange(15, 0, 15, 0, s, false));
				}
			}
			for (int i = 0; i < 12*debrisAmount; i++) {
				li.Add(generateObjectInRange(24, 0, 24));
			}
			for (int i = 0; i < paperCount; i++) {
				li.Add(generateObjectInRange(4, 2, 4, 2, papers[UnityEngine.Random.Range(0, papers.Count)], false));
			}
			for (int i = 0; i < scrapCount; i++) {
				VanillaResources mtl = VanillaResources.SCRAP1;
				switch(UnityEngine.Random.Range(0, 4)) {
					case 0:
						mtl = VanillaResources.SCRAP1;
						break;
					case 1:
						mtl = VanillaResources.SCRAP2;
						break;
					case 2:
						mtl = VanillaResources.SCRAP3;
						break;
					case 3:
						mtl = VanillaResources.SCRAP4;
						break;
				}
				li.Add(generateObjectInRange(18, 0, 18, 0, mtl.prefab, false));
			}
		}
		
		private GameObject generateObjectInRange(float dx, float dy, float dz, double offsetY = 0, string pfb = null, bool scale = true) {
			if (pfb == null)
				pfb = debrisProps.getRandomEntry();
			GameObject go = spawner(pfb);
			if (go == null)
				return go;
			Vector3 pos = MathUtil.getRandomVectorAround(position, new Vector3(dx, dy, dz));
			pos.y = Math.Min(pos.y, position.y)+(float)offsetY;
			go.transform.position = pos;
			go.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 360F), UnityEngine.Random.Range(0, 360F), 0);
			if (scale)
				go.transform.localScale = Vector3.one*debrisScale;
			SBUtil.removeComponent<MedicalCabinet>(go);
			SBUtil.removeComponent<Fabricator>(go);
			SBUtil.removeComponent<Centrifuge>(go);
			SBUtil.removeComponent<Radio>(go);
			SBUtil.removeComponent<Constructable>(go);
			PreventDeconstruction prev = go.EnsureComponent<PreventDeconstruction>();
			prev.enabled = true;
			prev.inEscapePod = true;
			if (!papers.Contains(pfb)) {
				SBUtil.applyGravity(go);
			}
			return go;
		}
	}
}

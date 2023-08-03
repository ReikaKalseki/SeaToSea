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

namespace ReikaKalseki.SeaToSea
{
		internal class C2CLifepod : MonoBehaviour {
		
			private Simplex1DGenerator pathVariation;
		
			private EscapePod pod;
			private Rigidbody body;
			private Stabilizer stabilizer;
			private WorldForces forces;
			private LiveMixin live;
		
			private Vector3 rotationSpeed;
			
			private Vector3 lastPosition;
			private MovingAverage movementSpeedXZ = new MovingAverage(90);
			
			private BiomeBase currentBiome;
		
			private static readonly float MAX_ROTATE_SPEED = 2F;
			
			void FixedUpdate() {
		    	if (C2CHooks.skipPodTick)
		    		return;
				if (!pod)
					pod = GetComponent<EscapePod>();
				if (!body)
					body = GetComponent<Rigidbody>();
				if (!stabilizer)
					stabilizer = GetComponent<Stabilizer>();
				if (!forces)
					forces = GetComponent<WorldForces>();
				if (!live)
					live = GetComponent<LiveMixin>();
				if (body && SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.PODFAIL)) {
					currentBiome = BiomeBase.getBiome(transform.position.setY(Mathf.Min(-3, transform.position.y-10)));
					float sp = getMovementSpeed();
					float dT = Time.fixedDeltaTime;
					if (pathVariation == null)
						getOrCreateNoise();
					if (sp > 0) {
						body.constraints = RigidbodyConstraints.None;
						body.drag = 0;
						body.angularDrag = 0;
						stabilizer.enabled = false;
						forces.enabled = false;
						float sp2 = sp < 1 ? sp*sp : sp;
						if (currentBiome == VanillaBiomes.DUNES) {
							sp2 *= 2F;
						}
						else if (currentBiome == VanillaBiomes.SPARSE || currentBiome == VanillaBiomes.GRANDREEF || currentBiome == VanillaBiomes.TREADER) {
							sp2 *= 1.5F;
						}
						else if (currentBiome == VanillaBiomes.MUSHROOM) {
							sp2 *= 1.25F;
						}
						Vector3 force = 0.2F*new Vector3(-1F, 0, 0.25F+1.8F*(float)pathVariation.getValue(new Vector3(DayNightCycle.main.timePassedAsFloat, 0, 0)))*sp2;
						//SNUtil.writeToChat(sp.ToString("0.000")+">"+force.ToString("F4"));
						body.velocity = force;
						
						live.TakeDamage(2*sp*dT, transform.position, DamageType.Normal);
						
						if (transform.position.y < -5) {
							body.transform.Rotate(rotationSpeed*dT, Space.Self);
							rotationSpeed += new Vector3(UnityEngine.Random.Range(-0.15F, 0.15F), UnityEngine.Random.Range(-0.15F, 0.15F), UnityEngine.Random.Range(-0.15F, 0.15F));
							rotationSpeed.x = Mathf.Clamp(rotationSpeed.x, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
							rotationSpeed.y = Mathf.Clamp(rotationSpeed.y, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
							rotationSpeed.z = Mathf.Clamp(rotationSpeed.z, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
						}
					}
					float depth = -transform.position.y;
					float tgt = getTargetDepth();
					//SNUtil.writeToChat(depth.ToString("000.0")+"/"+tgt.ToString("000.0"));
					if (tgt > 0.1F && tgt > depth) {
						float sink = 0.25F;
						if (tgt-depth > 80 || tgt >= 150)
							sink = 0.75F;
						else if (tgt-depth > 40 || tgt >= 80)
							sink = 0.5F;
						if (currentBiome == VanillaBiomes.VOID)
							sink = 2.5F;
						if (currentBiome == VanillaBiomes.DUNES)
							sink = 1F;
						body.velocity = body.velocity.setY(-sink);
						//SNUtil.writeToChat(body.velocity.ToString("F4"));
					}
					else if (depth > 10 && tgt < depth) {
						float rise = 0.2F;
						if (isStuck())
							rise = 1.5F*(float)Math.Max(0.1, 1-movementSpeedXZ.getAverage()*100);
						body.velocity = body.velocity.setY(rise);
					}
				}
				if (transform.position.y < -900) {
					float delay = 0F;
					if (Player.main.currentEscapePod == pod) {
						//delay = 2.5F;
						SoundManager.playSoundAt(SoundManager.buildSound("event:/sub/cyclops/explode"), transform.position);
						Player.main.liveMixin.Kill(DamageType.Explosive);
					}
					UnityEngine.Object.Destroy(gameObject, delay);
				}
				movementSpeedXZ.addValue(Vector3.Distance(transform.position.setY(lastPosition.y), lastPosition));
				//SNUtil.writeToChat(movementSpeedXZ.getAverage().ToString("00.0000"));
				lastPosition = transform.position;
			}
		
			private void getOrCreateNoise() {
				long use = SaveLoadManager.main.firstStart;
				if (pathVariation == null || pathVariation.seed != use) {
					pathVariation = (Simplex1DGenerator)new Simplex1DGenerator(use).setFrequency(0.004F);
				}
			}
			
			private float getPassedDays() {
				if (!Story.StoryGoalManager.main.completedGoals.Contains("Goal_Builder"))
					return 0;
				float time = DayNightCycle.main.timePassedAsFloat-0.4F;
				//SNUtil.writeToChat((time*10/DayNightCycle.kDayLengthSeconds).ToString("00.00"));
				return time/DayNightCycle.kDayLengthSeconds-3; //subtract 3 days as that is the assumed time to build a builder tool
			}
			
			private float getMovementSpeed() {
				float days = getPassedDays();
				if (days < 3)
					return 0;
				else if (days < 8)
					return 1;
				else if (days < 20)
					return 1+1.5F*(days-8)/12F;
				else
					return Mathf.Min(5F, 2.5F+(days-20)*0.5F);
			}
			
			private float getTargetDepth() {
				float days = getPassedDays();
				if (days < 20)
					return 0;
				days -= 20;
				if (isStuck())
					return 0;
				if (currentBiome == VanillaBiomes.SHALLOWS)
					return Mathf.Min(1, days/15F)*10;
				if (currentBiome == VanillaBiomes.KELP)
					return Mathf.Min(1, days/10F)*36;
				if (currentBiome == VanillaBiomes.REDGRASS)
					return Mathf.Min(1, days/10F)*80;
				if (currentBiome == VanillaBiomes.BLOODKELP)
					return Mathf.Min(1, days/8F)*120;
				if (currentBiome == VanillaBiomes.MUSHROOM)
					return Mathf.Min(1, days/6F)*150;
				if (currentBiome == VanillaBiomes.SPARSE)
					return Mathf.Min(1, days/6F)*180;
				if (currentBiome == VanillaBiomes.DUNES || currentBiome == VanillaBiomes.SPARSE || currentBiome == VanillaBiomes.GRANDREEF || currentBiome == VanillaBiomes.TREADER)
					return Mathf.Min(1, days/4F)*220;
				if (currentBiome == VanillaBiomes.VOID)
					return 9999;
				return -transform.position.y;
			}
			
			private bool isStuck() {
				return movementSpeedXZ.getAverage() <= 0.01F;
			}
			
		}
}

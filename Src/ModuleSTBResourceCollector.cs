using System;
using UnityEngine;

namespace FarFromKerbin
{
	internal class ModuleSTBResourceCollector : PartModule
	{
		[KSPField]
		public string resource;

		[KSPField]
		public float unitPerSec;

		[KSPField]
		public string input;

		[KSPField]
		public float inPerSec;

		[KSPField]
		public string deployAnim;

		[KSPField]
		public float maxSpeed;

		[KSPField]
		public string impactTransform;

		[KSPField(guiActive = true, guiActiveEditor = false, guiName = "Status ")]
		public string stat = "Off";

		[KSPField(guiActive = true, guiActiveEditor = false, guiName = "Ratio ")]
		public double rat = 0.0;

		[KSPField(isPersistant = true)]
		public bool isRunning = false;

		private Animation anim;

		private bool animState = false;

		private Transform impactTrans;

		[KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Toggle collector")]
		public void Toggle()
		{
            Debug.Log("Toggle, isRunning: " + isRunning.ToString());
			if (!this.isRunning)
			{
				if (base.vessel.situation != Vessel.Situations.LANDED && base.vessel.situation != Vessel.Situations.PRELAUNCH && !this.HasSurfaceConatct(this.impactTrans))
				{
					ScreenMessages.PostScreenMessage("You are not landed!", 5f, ScreenMessageStyle.UPPER_CENTER);
					this.stat = "Not landed";
				}
				else
				{
					this.DeployAnim();
					this.isRunning = true;
					this.stat = "Collecting";
				}
			}
			else
			{
				this.RetractAnim();
				this.isRunning = false;
				this.stat = "Off";
			}
		}

		public override void OnLoad(ConfigNode node)
		{
			if (base.part.FindModelAnimators(this.deployAnim)[0] != null)
			{
				this.anim = base.part.FindModelAnimators(this.deployAnim)[0];
				this.anim.wrapMode = WrapMode.Once;
				this.anim.playAutomatically = false;
			}
			Transform trans = GameObject.Find(this.impactTransform).GetComponent<Transform>();
			if (trans != null)
			{
				this.impactTrans = trans;
			}
			else
			{
				this.impactTrans = new GameObject("impactTransform").GetComponent<Transform>();
			}
		}

		public override void OnUpdate()
		{
			if (this.isRunning)
			{
				if (base.vessel.situation != Vessel.Situations.LANDED && base.vessel.situation != Vessel.Situations.PRELAUNCH && !this.HasSurfaceConatct(this.impactTrans))
				{
					ScreenMessages.PostScreenMessage("You are not landed!", 5f, ScreenMessageStyle.UPPER_CENTER);
					this.isRunning = false;
					this.stat = "Not landed";
					this.RetractAnim();
				}
				else if (MBToolbox.AmountAvailable(this.input, base.part) < (double)(this.inPerSec * TimeWarp.deltaTime))
				{
					ScreenMessages.PostScreenMessage("Not enough" + this.input, 5f, ScreenMessageStyle.UPPER_CENTER);
					this.isRunning = false;
					this.stat = "Not enough " + this.input;
					this.RetractAnim();
				}
				else if (base.vessel.srfSpeed > (double)this.maxSpeed)
				{
					ScreenMessages.PostScreenMessage("Too fast!", 5f, ScreenMessageStyle.UPPER_CENTER);
					this.isRunning = false;
					this.stat = "Too fast!";
				}
				else
				{
					MBToolbox.RequestResource(this.input, (double)(this.inPerSec * TimeWarp.deltaTime), base.part);
                    double free = MBToolbox.FreeSpaceAvailable(this.resource, base.part);
                    double d = (double)(this.unitPerSec * TimeWarp.deltaTime / this.maxSpeed) * base.vessel.srfSpeed;
                    this.rat = MBToolbox.AddResource(this.resource, d, base.part);
                    Debug.Log("Free space available: " + free.ToString());
                    Debug.Log("d: " + d.ToString());
                    Debug.Log("rat: " + rat.ToString());
                    double free2 = MBToolbox.FreeSpaceAvailable(this.resource, base.part);

                    if (free == 0)
					{
						ScreenMessages.PostScreenMessage("Not enough space for the resources!", 5f, ScreenMessageStyle.UPPER_CENTER);
						this.isRunning = false;
						this.stat = "Not enough free space";
						this.RetractAnim();
					}
				}
			}
		}

		public override string GetInfo()
		{
			return string.Concat(new string[]
			{
				"Resource Collector \n Collects resource from planetary surfaces. \n \n <color=lime>Requires:</color>\n ",
				this.inPerSec.ToString(),
				this.input,
				"per sec\n <color=lime>Produces:</color> \n ",
				this.unitPerSec.ToString(),
				this.resource,
				"per sec."
			});
		}

		private bool HasSurfaceConatct(Transform trans)
		{
			Debug.Log("[STB]: " + MBToolbox.GetSurfaceAlt(trans.position));
			return MBToolbox.GetSurfaceAlt(trans.position) <= 0.1;
		}

		private void DeployAnim()
		{
			if (!this.animState)
			{
                Debug.Log("DeployAnim");
				this.anim[this.deployAnim].speed = 0.5f;
                this.anim[this.deployAnim].normalizedTime = 0f;
                this.anim.Play();
				this.animState = true;
			}
		}

		private void RetractAnim()
		{
            this.isRunning = false;

            if (this.animState)
			{
                Debug.Log("RetractAnim");
                
                this.anim[this.deployAnim].normalizedTime = 1f;
                //this.anim[this.deployAnim].time = this.anim[this.deployAnim].length;
				this.anim[this.deployAnim].speed = -0.5f;
				this.anim.Play();
				this.animState = false;
			}
		}
	}
}

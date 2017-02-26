using KIS;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FarFromKerbin
{
	public class ModuleSTBBlockCompresser : PartModule
	{
		public struct STBPrintable
		{
			public AvailablePart apart;

			public Texture icon;

			public int iconRes;

			private KIS_IconViewer _iconViewer;

			public STBPrintable(string partName)
			{
				this.iconRes = 64;
				this.apart = PartLoader.getPartInfoByName(partName);
				this._iconViewer = new KIS_IconViewer(this.apart.partPrefab, this.iconRes);
				this.icon = this._iconViewer.texture;
			}

			public STBPrintable(AvailablePart apart)
			{
				this.iconRes = 64;
				this.apart = apart;
				this._iconViewer = new KIS_IconViewer(apart.partPrefab, this.iconRes);
				this.icon = this._iconViewer.texture;
			}
		}

		[KSPField]
		public int maxQueue;

		[KSPField]
		public double EcPerSec;

		[KSPField]
		public float reqSec;

		[KSPField]
		public float dirtPerBlock = 100f;

		private float dtime;

		private bool isBusy = false;

		private System.Collections.Generic.List<ModuleSTBBlockCompresser.STBPrintable> queue = new System.Collections.Generic.List<ModuleSTBBlockCompresser.STBPrintable>();

		public GUISkin skin;

		private System.Collections.Generic.List<ModuleSTBBlockCompresser.STBPrintable> blockList = new System.Collections.Generic.List<ModuleSTBBlockCompresser.STBPrintable>();

		private Rect _windowPos = new Rect(0f, 0f, 500f, 500f);

		private bool _guiVisible = false;

		[KSPEvent(guiActive = true, guiName = "Compress!")]
		public void OpenGui()
		{
			if (!this._guiVisible)
			{
				this._guiVisible = true;
			}
			else
			{
				this._guiVisible = false;
			}
		}

		public override void OnLoad(ConfigNode node)
		{
			for (int i = 0; i < this.maxQueue; i++)
			{
				if (node.HasValue("queue" + i.ToString()))
				{
					if (this.queue.Count > i + 1)
					{
						this.queue[i] = new ModuleSTBBlockCompresser.STBPrintable(node.GetValue("queue" + i.ToString()));
					}
					else
					{
						this.queue.Add(new ModuleSTBBlockCompresser.STBPrintable(node.GetValue("queue" + i.ToString())));
					}
				}
			}
			if (node.HasValue("isBusy"))
			{
				this.isBusy = System.Convert.ToBoolean(node.GetValue("isBusy"));
			}
			else
			{
				this.isBusy = false;
			}
			if (node.HasValue("dtime"))
			{
				this.dtime = float.Parse(node.GetValue("dtime"));
			}
			else
			{
				this.dtime = 0f;
			}
			foreach (AvailablePart apart in PartLoader.LoadedPartsList)
			{
				if (apart.name.Contains("block"))
				{
					this.blockList.Add(new ModuleSTBBlockCompresser.STBPrintable(apart));
				}
			}
			if (this.skin == null)
			{
				this.skin = HighLogic.Skin;
				this.skin.window.padding = new RectOffset(20, 20, 20, 20);
				this.skin.button.padding = new RectOffset(10, 10, 10, 10);
			}
			this._windowPos.x = (float)(Screen.width / 2) - this._windowPos.width / 2f;
			this._windowPos.y = (float)(Screen.height / 2) - this._windowPos.height / 2f;
		}

		public override void OnSave(ConfigNode node)
		{
			int i = 0;
			foreach (ModuleSTBBlockCompresser.STBPrintable b in this.queue)
			{
				node.AddValue("queue" + i.ToString(), this.queue[i].apart.name);
			}
			node.AddValue("isBusy", this.isBusy);
			node.AddValue("dtime", this.dtime);
		}

		public override void OnUpdate()
		{
			if (this.queue.Count != 0)
			{
				if (this.dtime == 0f)
				{
					this.isBusy = true;
					this.Compress();
				}
				else if (this.dtime >= this.reqSec)
				{
					this.EndCompress();
				}
				else if (0f < this.dtime && this.dtime < this.reqSec)
				{
					this.Compress();
				}
			}
		}

		public override string GetInfo()
		{
			return string.Concat(new string[]
			{
				"Dirt Compresser \n Compress dirt into solid blocks. \n \n <color=lime>Requires:</color>\n ",
				this.EcPerSec.ToString(),
				"Ec. per sec.\n <color=lime>Required dirt per block:</color>\n ",
				this.dirtPerBlock.ToString(),
				" \n<color=lime>Compress time:</color> \n ",
				this.reqSec.ToString(),
				"sec."
			});
		}

		private void Compress()
		{
			if (MBToolbox.AmountAvailable("ElectricCharge", base.part) < this.EcPerSec)
			{
				if (this.isBusy)
				{
					ScreenMessages.PostScreenMessage("Not enough electric charge! Nowadays nothing works without elctricity!", 2f, ScreenMessageStyle.UPPER_CENTER);
				}
				this.isBusy = false;
			}
			else if (MBToolbox.AmountAvailable("Dirt", base.part) < (double)(this.dirtPerBlock / 100f))
			{
				if (this.isBusy)
				{
					ScreenMessages.PostScreenMessage("Not enough dirt! What do you want?", 2f, ScreenMessageStyle.UPPER_CENTER);
				}
				this.isBusy = false;
			}
			else
			{
				this.isBusy = true;
				MBToolbox.RequestResource("ElectricCharge", this.EcPerSec * (double)TimeWarp.deltaTime, base.part);
				MBToolbox.RequestResource("Dirt", (double)(this.dirtPerBlock / this.reqSec * TimeWarp.deltaTime), base.part);
				this.dtime += TimeWarp.deltaTime;
			}
		}

		private void EndCompress()
		{
			Part prefab = this.queue[this.queue.Count - 1].apart.partPrefab;
			this.dtime = 0f;
			this.isBusy = false;
			this.queue.RemoveAt(this.queue.Count - 1);
			System.Collections.Generic.List<ModuleKISInventory> invs = MBToolbox.GetInventories(base.vessel);
			if (invs.Count == 0)
			{
				ScreenMessages.PostScreenMessage("No inventories found!", 2f, ScreenMessageStyle.UPPER_CENTER);
			}
			else
			{
				foreach (ModuleKISInventory inv in invs)
				{
					if (inv.part.name == base.part.name)
					{
						if (inv.isFull() || inv.GetContentVolume() + 1000f > inv.maxVolume)
						{
							break;
						}
						inv.AddItem(prefab, 1f, -1);
						return;
					}
				}
				foreach (ModuleKISInventory inv2 in invs)
				{
					if (!inv2.isFull() || inv2.GetContentVolume() + 1000f <= inv2.maxVolume)
					{
						inv2.AddItem(prefab, 1f, -1);
						return;
					}
				}
				ScreenMessages.PostScreenMessage("No free inventories found!", 2f, ScreenMessageStyle.UPPER_CENTER);
			}
		}

		private void OnGUI()
		{
			GUI.skin = this.skin;
			if (this._guiVisible)
			{
				this._windowPos = GUILayout.Window(base.GetInstanceID(), this._windowPos, new GUI.WindowFunction(this.drawWindow), "Ulti-Compress-Machine", new GUILayoutOption[0]);
			}
		}

		private void drawWindow(int windowID)
		{
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Space(20f);
			int eInRow = 0;
			foreach (ModuleSTBBlockCompresser.STBPrintable b in this.blockList)
			{
				if (eInRow == 0)
				{
					GUILayout.BeginHorizontal(new GUILayoutOption[0]);
					this.DrawIcon(b);
					eInRow++;
				}
				else if (eInRow <= 5)
				{
					this.DrawIcon(b);
					eInRow++;
				}
				else if (eInRow == 6)
				{
					this.DrawIcon(b);
					GUILayout.EndHorizontal();
					eInRow = 0;
				}
			}
			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		private void DrawIcon(ModuleSTBBlockCompresser.STBPrintable b)
		{
			if (GUILayout.Button(new GUIContent(b.apart.title, b.icon, b.apart.description), new GUILayoutOption[0]))
			{
				this.IconClicked(b);
			}
		}

		private void IconClicked(ModuleSTBBlockCompresser.STBPrintable b)
		{
			if (this.queue.Count == this.maxQueue)
			{
				ScreenMessages.PostScreenMessage("Queue limit reached! Hold on!", 2f, ScreenMessageStyle.UPPER_CENTER);
			}
			else
			{
				this.queue.Add(b);
				this.isBusy = true;
				this._guiVisible = false;
			}
		}
	}
}

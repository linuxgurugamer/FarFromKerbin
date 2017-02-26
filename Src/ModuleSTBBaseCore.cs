using System;
using UnityEngine;

namespace FarFromKerbin
{
	internal class ModuleSTBBaseCore : PartModule
	{
		private bool showGui = false;

		private Rect ibvGui = new Rect(0f, 0f, 0f, 0f);

		[KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Internal walk")]
		public void ToggleGui()
		{
			this.showGui = !this.showGui;
		}

		public override void OnStart(PartModule.StartState state)
		{
			if (state != PartModule.StartState.Editor)
			{
				base.part.CrewCapacity = this.GetBaseCrewCapacity();
			}
			this.ibvGui.height = 200f;
			this.ibvGui.width = 200f;
			this.ibvGui.x = (float)(Screen.width / 2);
			this.ibvGui.y = (float)(Screen.height / 2);
		}

		private void OnGUI()
		{
			GUI.skin = HighLogic.Skin;
			if (this.showGui)
			{
				this.ibvGui = GUILayout.Window(base.GetInstanceID(), this.ibvGui, new GUI.WindowFunction(this.drawWindow), "Select a kerbal", new GUILayoutOption[0]);
			}
		}

		private void drawWindow(int id)
		{
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			if (GUILayout.Toggle(false, "", new GUILayoutOption[0]))
			{
				this.showGui = false;
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Space(10f);
			foreach (ProtoCrewMember i in base.vessel.GetVesselCrew())
			{
				if (GUILayout.Button(i.name, new GUILayoutOption[]
				{
					GUILayout.Height(20f)
				}))
				{
					this.GoToIBV(i);
				}
			}
			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		private void GoToIBV(ProtoCrewMember k)
		{
			this.showGui = false;
			GameObject intHatch = new GameObject("intHatch");
			intHatch.transform.position = base.part.partTransform.TransformPoint(new Vector3(0f, 0f, -1.5f));
            //ProtoCrewMember intEva = new ProtoCrewMember(HighLogic.CurrentGame.Mode, k);
            ProtoCrewMember intEva = new ProtoCrewMember(k);
            ProtoCrewMember expr_55 = intEva;
            expr_55.ChangeName(expr_55.name +"int");
			if (FlightEVA.fetch.spawnEVA(intEva, base.vessel.rootPart, intHatch.transform))
			{
				FlightCamera.fetch.SetDistanceImmediate(2f);
			}
		}

		private int GetBaseCrewCapacity()
		{
			int cc = 0;
			foreach (Part p in base.vessel.Parts)
			{
				if (p.name.Contains("block"))
				{
					cc++;
				}
			}
			return cc;
		}
	}
}

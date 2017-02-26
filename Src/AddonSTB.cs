using System;
using UnityEngine;

namespace FarFromKerbin
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class AddonSTB : MonoBehaviour
	{
		private Vessel oldVessel;

		public void Start()
		{
			GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(this.onVesselChange));
		}

		private void onVesselChange(Vessel v)
		{
			if (this.oldVessel != null)
			{
				if (this.oldVessel.isEVA)
				{
					foreach (ProtoCrewMember item in this.oldVessel.GetVesselCrew())
					{
						if (item.KerbalRef.crewMemberName.Contains("int"))
						{
							this.oldVessel.Die();
							break;
						}
					}
				}
			}
			this.oldVessel = v;
		}
	}
}

using System;

namespace FarFromKerbin
{
	internal class ModuleSTBEVAResourceRefueler : PartModule
	{
		[KSPEvent(guiName = "Take Resources", guiActiveUncommand = true, guiActiveUnfocused = true, unfocusedRange = 20f)]
		public void fuel()
		{
			Vessel v = FlightGlobals.ActiveVessel;
			foreach (PartResource r in v.rootPart.Resources)
			{
				foreach (PartResource rt in base.part.Resources)
				{
					if (r.resourceName.Equals(rt.resourceName))
					{
						MBToolbox.magicResourceTransfer(r.resourceName, r.maxAmount, rt.part, r.part);
					}
				}
			}
		}
	}
}

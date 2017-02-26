using KIS;
using System;

namespace FarFromKerbin
{
	internal class ModuleSTBEVABackpack : ModuleKISItem
	{
		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			foreach (PartResource r in base.part.Resources)
			{
				if (r.amount != 0.0)
				{
					r.amount = 0.0;
				}
			}
		}

		public override void OnEquip(KIS_Item item)
		{
			base.OnEquip(item);
			Vessel v = FlightGlobals.ActiveVessel;
			if (v.isEVA)
			{
				foreach (PartResource r in base.part.Resources)
				{
					if (!v.rootPart.Resources.Contains(r.resourceName))
					{
						ConfigNode i = new ConfigNode("RESOURCE");
						i.AddValue("name", r.resourceName);
						i.AddValue("amount", 0);
						i.AddValue("maxAmount", r.maxAmount);
						v.rootPart.Resources.Add(i);
					}
					else
					{
						ConfigNode i = new ConfigNode("RESOURCE");
						i.AddValue("name", r.resourceName);
						i.AddValue("amount", 0);
						i.AddValue("maxAmount", r.maxAmount);
						v.rootPart.SetResource(i);
					}
				}
			}
		}

		public override void OnUnEquip(KIS_Item item)
		{
			base.OnUnEquip(item);
			Vessel v = FlightGlobals.ActiveVessel;
			if (v.isEVA)
			{
				foreach (PartResource re in v.rootPart.Resources)
				{
					foreach (PartResource r in base.part.Resources)
					{
						if (re.resourceName == r.resourceName)
						{
							ConfigNode i = new ConfigNode("RESOURCE");
							i.AddValue("name", r.resourceName);
							i.AddValue("amount", 0);
							i.AddValue("maxAmount", 0);
							v.rootPart.SetResource(i);
						}
					}
				}
			}
		}
	}
}

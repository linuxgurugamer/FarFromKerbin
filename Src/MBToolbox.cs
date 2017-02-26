using KIS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FarFromKerbin
{
	internal static class MBToolbox
	{
		public static double GetSurfaceAlt(Vector3 pos)
		{
			double result;
			if (FlightGlobals.ActiveVessel.terrainAltitude > 0.0)
			{
				result = FlightGlobals.ship_altitude - FlightGlobals.ActiveVessel.terrainAltitude;
			}
			else
			{
				result = FlightGlobals.ship_altitude;
			}
			return result;
		}

		public static double AddResource(string resource, double amount, Part part)
		{
            Debug.Log("AddResource, resource: " + resource + "  amount: " + amount.ToString());
            double amt = part.RequestResource(resource, -amount);
            Debug.Log("Amount added: " + amt.ToString());
            return -amt;
#if false

            PartResourceDefinition res = PartResourceLibrary.Instance.GetDefinition(resource);
            System.Collections.Generic.List<PartResource> resList = new System.Collections.Generic.List<PartResource>();
			part.GetConnectedResources(res.id, res.resourceFlowMode, resList);
			double demandLeft = amount;
			double amountAdded = 0.0;
			foreach (PartResource r in resList)
			{
				if (r.maxAmount >= demandLeft + r.amount)
				{
					amountAdded += demandLeft;
					r.amount += demandLeft;
					demandLeft = 0.0;
				}
				else
				{
					amountAdded += r.maxAmount - r.amount;
					demandLeft -= r.maxAmount - r.amount;
					r.amount = r.maxAmount;
				}
			}
			return amountAdded;
#endif
		}

        public static double FreeSpaceAvailable(string resource, Part part)
        {
            PartResourceDefinition res = PartResourceLibrary.Instance.GetDefinition(resource);
            double amount, maxAmount;
            part.GetConnectedResourceTotals(res.id, out amount, out maxAmount);
            return maxAmount - amount;
        }

        public static double AmountAvailable(string resource, Part part)
		{
			PartResourceDefinition res = PartResourceLibrary.Instance.GetDefinition(resource);
            double amount, maxAmount;
            part.GetConnectedResourceTotals(res.id, out amount, out maxAmount);
            return amount;
            //return maxAmount - amount;
#if false
            System.Collections.Generic.List<PartResource> resList = new System.Collections.Generic.List<PartResource>();
			part.GetConnectedResources(res.id, res.resourceFlowMode, resList);
			return resList.Sum((PartResource r) => r.amount);
#endif
		}

		public static double RequestResource(string resource, double amount, Part part)
		{
            Debug.Log("RequestResource, resource: " + resource + "  amount requested: " + amount.ToString());
            double amt = part.RequestResource(resource, amount);
            Debug.Log("Amount returned: " + amt.ToString());
            return amt;
#if false
            PartResourceDefinition res = PartResourceLibrary.Instance.GetDefinition(resource);
			System.Collections.Generic.List<PartResource> resList = new System.Collections.Generic.List<PartResource>();
			part.GetConnectedResources(res.id, res.resourceFlowMode, resList);
			double demandLeft = amount;
			double amountTaken = 0.0;
			foreach (PartResource r in resList)
			{
				if (r.amount >= demandLeft)
				{
					amountTaken += demandLeft;
					r.amount -= demandLeft;
					demandLeft = 0.0;
				}
				else
				{
					amountTaken += r.amount;
					demandLeft -= r.amount;
					r.amount = 0.0;
				}
			}
			return amountTaken;
#endif
		}

		public static System.Collections.Generic.List<ModuleKISInventory> GetInventories(Vessel vessel)
		{
			System.Collections.Generic.List<ModuleKISInventory> inventories = new System.Collections.Generic.List<ModuleKISInventory>();
			foreach (Part part in vessel.parts)
			{
				foreach (PartModule module in part.Modules)
				{
					if (typeof(ModuleKISInventory) == module.GetType())
					{
						inventories.Add((ModuleKISInventory)module);
					}
				}
			}
			return inventories;
		}

		public static double magicResourceTransfer(string resName, double amount, Part from, Part to)
		{
			return MBToolbox.AddResource(resName, MBToolbox.RequestResource(resName, amount, from), to);
		}

		public static System.Collections.Generic.List<PartResource> GetVesselResources(Vessel v)
		{
			System.Collections.Generic.List<PartResource> list = new System.Collections.Generic.List<PartResource>();
			foreach (Part p in v.parts)
			{
				foreach (PartResource r in p.Resources)
				{
					list.Add(r);
				}
			}
			return list;
		}
	}
}

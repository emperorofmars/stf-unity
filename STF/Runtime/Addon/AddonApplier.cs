
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STF.Serialisation;
using STF.Types;
using STF.Util;
using UnityEngine;

namespace STF.Addon
{
	public static class STFAddonApplierRegistry
	{
		public static readonly Dictionary<Type, ISTFNodeComponentAddonApplier> DefaultAddonAppliers = new()
		{
			{typeof(SkinnedMeshRenderer), new STFMeshInstanceAddonApplier()},
		};
		private static Dictionary<Type, ISTFNodeComponentAddonApplier> RegisteredAddonAppliers = new();
		public static Dictionary<Type, ISTFNodeComponentAddonApplier> AddonAppliers => CollectionUtil.Combine(DefaultAddonAppliers, RegisteredAddonAppliers);

		public static void RegisterAddonApplier(Type Type, ISTFNodeComponentAddonApplier Applier) { RegisteredAddonAppliers.Add(Type, Applier); }
	}

	public interface ISTFAddonApplierContext
	{
		GameObject Root {get;}

		void AddTask(Task Task);
	}

	public class DefaultSTFAddonApplierContext : ISTFAddonApplierContext
	{
		public GameObject _Root;
		public GameObject Root => _Root;

		public List<Task> Tasks = new();
		public void AddTask(Task Task) { Tasks.Add(Task); }

		public DefaultSTFAddonApplierContext(GameObject Root)
		{
			this._Root = Root;
		}
	}

	public static class STFAddonApplier
	{
		public static GameObject Apply(ISTFAsset Base, STFAddonAsset Addon, bool InPlace = false)
		{
			GameObject ret = InPlace ? Base.gameObject : UnityEngine.Object.Instantiate(Base.gameObject);
			ret.name = Base.name + "_applied_" + Addon.STFName;

			var ApplierContext = new DefaultSTFAddonApplierContext(ret);

			for(int addonNodeIdx = 0; addonNodeIdx < Addon.transform.childCount; addonNodeIdx++)
			{
				var addonGo = Addon.transform.GetChild(addonNodeIdx);
				var addonNode = addonGo.GetComponent<ISTFNode>();
				if(addonNode.Type == STFAppendageNode._TYPE)
				{
					var target = ret.GetComponentsInChildren<ISTFNode>().FirstOrDefault(c => c.Id == (addonNode as STFAppendageNode).TargetId);
					if(target != null)
					{
						var t = UnityEngine.Object.Instantiate(addonGo);
						t.SetParent(target.transform);
						t.name = addonGo.name;
						// transform components
						foreach(var component in t.GetComponents<Component>())
						{
							if(STFAddonApplierRegistry.AddonAppliers.ContainsKey(component.GetType()))
							{
								STFAddonApplierRegistry.AddonAppliers[component.GetType()].Apply(ApplierContext, target.gameObject, component);
							}
						}
					}
					else
					{
						throw new System.Exception("Target node not found!");
					}
				}
				else if(addonNode.Type == STFPatchNode._TYPE)
				{
					var target = ret.transform.GetComponentsInChildren<ISTFNode>().FirstOrDefault(c => c.Id == (addonNode as STFPatchNode).TargetId);
					if(target != null)
					{
						// copy children
						for(int addonChildIdx = 0; addonChildIdx < addonGo.transform.childCount; addonChildIdx++)
						{
							var instance = UnityEngine.Object.Instantiate(addonGo.transform.GetChild(addonChildIdx).gameObject);
							instance.transform.SetParent(target.transform);
							instance.name = addonGo.transform.GetChild(addonChildIdx).name;
						}
						// copy acomponents
						foreach(var component in addonGo.GetComponents<Component>())
						{
							if(STFAddonApplierRegistry.AddonAppliers.ContainsKey(component.GetType()))
							{
								// transform components
								STFAddonApplierRegistry.AddonAppliers[component.GetType()].Apply(ApplierContext, target.gameObject, component);
							}
							else
							{	// copy the component
								STFDefaultNodeComponentAddonApplier.Apply(ApplierContext, target.gameObject, component);
							}
						}
					}
					else
					{
						throw new System.Exception("Target node not found!");
					}
				}
			}
			Utils.RunTasks(ApplierContext.Tasks);
			ret.GetComponent<ISTFAsset>().AppliedAddonMetas.Add(new ISTFAsset.AppliedAddonMeta { AddonId = Addon.Id });
			return ret;
		}
	}
}

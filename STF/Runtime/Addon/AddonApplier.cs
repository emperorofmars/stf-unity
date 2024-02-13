
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STF.Serialisation;
using STF.Util;
using UnityEngine;

namespace STF.Addon
{
	public static class STFAddonApplierRegistry
	{
		public static readonly Dictionary<Type, ISTFNodeComponentAddonApplier> DefaultAddonAppliers = new Dictionary<Type, ISTFNodeComponentAddonApplier> {
			{typeof(SkinnedMeshRenderer), new STFMeshInstanceAddonApplier()},
		};
		private static Dictionary<Type, ISTFNodeComponentAddonApplier> RegisteredAddonAppliers = new Dictionary<Type, ISTFNodeComponentAddonApplier>();
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

		public List<Task> Tasks = new List<Task>();
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
			ret.name = Base.name + "_applied_" + Addon.Name;

			var ApplierContext = new DefaultSTFAddonApplierContext(ret);

			for(int addonNodeIdx = 0; addonNodeIdx < Addon.transform.childCount; addonNodeIdx++)
			{
				var addonGo = Addon.transform.GetChild(addonNodeIdx);
				var addonNode = addonGo.GetComponent<ISTFNode>();
				if(addonNode.Type == STFAppendageNode._TYPE)
				{
					var target = ret.transform.GetComponentsInChildren<ISTFNode>().FirstOrDefault(c => c.Id == (addonNode as STFAppendageNode).TargetId);
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
						for(int addonChildIdx = 0; addonChildIdx < target.transform.childCount; addonChildIdx++)
						{
							var t = UnityEngine.Object.Instantiate(addonGo.GetChild(addonChildIdx));
							t.SetParent(target.transform);
							t.name = addonGo.GetChild(addonChildIdx).name;
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
			return ret;
		}
	}
}

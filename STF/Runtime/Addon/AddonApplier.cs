
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
		public static readonly Dictionary<Type, ISTFAddonApplier> DefaultAddonAppliers = new Dictionary<Type, ISTFAddonApplier> {
			{typeof(SkinnedMeshRenderer), new STFMeshInstanceAddonApplier()},
		};
		private static Dictionary<Type, ISTFAddonApplier> RegisteredAddonAppliers = new Dictionary<Type, ISTFAddonApplier>();
		public static Dictionary<Type, ISTFAddonApplier> AddonAppliers => CollectionUtil.Combine(DefaultAddonAppliers, RegisteredAddonAppliers);

		public static void RegisterAddonApplier(Type Type, ISTFAddonApplier Applier) { RegisteredAddonAppliers.Add(Type, Applier); }
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
				var addonNode = addonGo.GetComponent<ASTFNode>();
				if(addonNode.Type == STFAppendageNode._TYPE)
				{
					var target = ret.transform.GetComponentsInChildren<ASTFNode>().FirstOrDefault(c => c.Id == (addonNode as STFAppendageNode).TargetId);
					if(target != null)
					{
						UnityEngine.Object.Instantiate(addonGo).SetParent(target.transform);
					}
					else
					{
						throw new System.Exception("Target node not found!");
					}
				}
				else if(addonNode.Type == STFPatchNode._TYPE)
				{
					var target = ret.transform.GetComponentsInChildren<ASTFNode>().FirstOrDefault(c => c.Id == (addonNode as STFPatchNode).TargetId);
					if(target != null)
					{
						// copy children
						for(int addonChildIdx = 0; addonChildIdx < target.transform.childCount; addonChildIdx++)
						{
							UnityEngine.Object.Instantiate(addonGo.GetChild(addonChildIdx)).SetParent(target.transform);
						}
						// copy acomponents
						foreach(var component in addonGo.GetComponents<Component>())
						{
							if(STFAddonApplierRegistry.AddonAppliers.ContainsKey(component.GetType()))
							{
								STFAddonApplierRegistry.AddonAppliers[component.GetType()].Apply(ApplierContext, target.gameObject, component);
							}
							else
							{	// copy the component
								STFDefaultAddonApplier.Apply(ApplierContext, target.gameObject, component);
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

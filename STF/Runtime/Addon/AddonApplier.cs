
using System;
using System.Collections.Generic;
using System.Linq;
using STF.Serialisation;
using UnityEngine;

namespace STF.Addon
{
	public static class STFAddonApplierRegistry
	{
		public static readonly Dictionary<Type, ISTFAddonApplier> DefaultAddonAppliers = new Dictionary<Type, ISTFAddonApplier>();
		private static Dictionary<Type, ISTFAddonApplier> RegisteredAddonAppliers = new Dictionary<Type, ISTFAddonApplier>();
		public static Dictionary<Type, ISTFAddonApplier> AddonAppliers => CollectionUtil.Combine(DefaultAddonAppliers, RegisteredAddonAppliers);

		public static void RegisterAddonApplier(Type Type, ISTFAddonApplier Applier) { RegisteredAddonAppliers.Add(Type, Applier); }
	}

	public static class STFAddonApplier
	{
		public static GameObject Apply(ISTFAsset Base, STFAddonAsset Addon, bool InPlace = false)
		{
			GameObject ret = InPlace ? Base.gameObject : UnityEngine.Object.Instantiate(Base.gameObject);
			ret.name = Base.name + "_applied_" + Addon.Name;

			for(int addonNodeIdx = 0; addonNodeIdx < Addon.transform.childCount; addonNodeIdx++)
			{
				var addonGo = Addon.transform.GetChild(addonNodeIdx);
				var addonNode = addonGo.GetComponent<ISTFNode>();
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
							// Do this with addon appliers
							var newComponent = target.gameObject.AddComponent(component.GetType());
							System.Reflection.FieldInfo[] fields = component.GetType().GetFields(); 
							foreach (System.Reflection.FieldInfo field in fields)
							{
								field.SetValue(newComponent, field.GetValue(component));
							}
						}
					}
					else
					{
						throw new System.Exception("Target node not found!");
					}
				}
			}
			return ret;
		}
	}
}

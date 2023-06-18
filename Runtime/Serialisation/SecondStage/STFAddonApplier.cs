using System;
using System.Collections.Generic;
using System.Linq;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	public static class STFAddonTriggerRegistry
	{
		public static Dictionary<Type, ISTFAddonTrigger> RegisteredAddonTriggers = new Dictionary<Type, ISTFAddonTrigger>() {
			{typeof(STFSkinnedMeshRendererAddon), new STFMeshInstanceAddonApplier()}
		};

		public static STFAddonContext GetDefaultAddonContext()
		{
			return new STFAddonContext(RegisteredAddonTriggers);
		}
	}

	public class STFAddonContext
	{
		public STFAddonContext(Dictionary<Type, ISTFAddonTrigger> addonTriggers) { AddonTriggers = addonTriggers; }
		public readonly Dictionary<Type, ISTFAddonTrigger> AddonTriggers = new Dictionary<Type, ISTFAddonTrigger>();
	}

	public static class AddonApplier
	{
		public static GameObject ApplyAddon(GameObject originalRoot, STFAddonAssetInfo addon)
		{
			return ApplyAddon(originalRoot, addon, STFAddonTriggerRegistry.GetDefaultAddonContext());
		}

		public static GameObject ApplyAddon(GameObject originalRoot, STFAddonAssetInfo addon, STFAddonContext context)
		{
			var root = UnityEngine.Object.Instantiate(originalRoot);
			root.name = originalRoot.name;
			for(int i = 0; i < addon.transform.childCount; i++)
			{
				var a = addon.transform.GetChild(i);
				var appendage = a.GetComponent<STFAppendageNode>();
				var patch = a.GetComponent<STFPatchNode>();
				if(appendage != null)
				{
					var target = root.GetComponentsInChildren<STFUUID>().First(t => t.id == appendage.targetId);
					var appendageInstance = UnityEngine.Object.Instantiate(appendage.gameObject);
					appendageInstance.name = appendage.name;
					appendageInstance.transform.parent = target.transform;
				}
				else if(patch != null)
				{
					var target = root.GetComponentsInChildren<STFUUID>().First(t => t.id == patch.targetId);
					foreach(var c in patch.GetComponents<Component>())
					{
						var newComponent = target.gameObject.AddComponent(c.GetType());
						System.Reflection.FieldInfo[] fields = c.GetType().GetFields(); 
						foreach (System.Reflection.FieldInfo field in fields)
						{
							field.SetValue(newComponent, field.GetValue(c));
						}
					}
					for(int patchChildIdx = 0; patchChildIdx < patch.transform.childCount; patchChildIdx++)
					{
						var newChild = UnityEngine.Object.Instantiate(patch.transform.GetChild(patchChildIdx));
						newChild.name = patch.transform.GetChild(patchChildIdx).name;
						newChild.transform.parent = target.transform;
					}
				}
				else
				{
					throw new System.Exception("Invalid addon asset. Root nodes must be either addon or patch nodes!");
				}
			}
			foreach(var addonType in context.AddonTriggers.Keys)
			{
				foreach(var addonTriggerComponent in root.GetComponentsInChildren(addonType))
				{
					context.AddonTriggers[addonType].apply(addonTriggerComponent, root);
				}
			}
			return root;
		}
	}
}
using System.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public static class AddonApplier
	{
		public static GameObject ApplyAddon(GameObject originalRoot, STFAddonAssetInfo addon)
		{
			var root = Object.Instantiate(originalRoot);
			root.name = originalRoot.name;
			for(int i = 0; i < addon.transform.childCount; i++)
			{
				var a = addon.transform.GetChild(i);
				var appendage = a.GetComponent<STFAppendageNode>();
				var patch = a.GetComponent<STFPatchNode>();
				if(appendage != null)
				{
					var target = root.GetComponentsInChildren<STFUUID>().First(t => t.id == appendage.targetId);
					var appendageInstance = Object.Instantiate(appendage.gameObject);
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
						var newChild = Object.Instantiate(patch.transform.GetChild(patchChildIdx));
						newChild.name = patch.transform.GetChild(patchChildIdx).name;
						newChild.transform.parent = target.transform;
					}
				}
				else
				{
					throw new System.Exception("Invalid addon asset. Root nodes must be either addon or patch nodes!");
				}
			}
			return root;
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using stf.serialisation;
using UnityEditor;
using UnityEngine;

namespace stf
{
	public static class STFSetup
	{
		public static void SetupInplace(GameObject root, string resourcePath)
		{
			var asset = root.AddComponent<STFAssetInfo>();
			asset.assetId = Guid.NewGuid().ToString();
			asset.assetType = "asset";
			foreach(var c in root.GetComponentsInChildren<Transform>())
			{
				var id = c.gameObject.AddComponent<STFUUID>();
				id.id = Guid.NewGuid().ToString();
				// handle supported non stf components
				var skm = c.gameObject.GetComponent<SkinnedMeshRenderer>();
				if(skm != null)
				{
					id.componentIds.Add(new STFUUID.ComponentIdMapping{component = skm, id = Guid.NewGuid().ToString()});
				}
			}
			var armatures = STFArmatureUtil.FindAndSetupArmatures(root);
			foreach(var armature in armatures)
			{
				AssetDatabase.CreateAsset(armature, resourcePath + Path.DirectorySeparatorChar + armature.name + ".asset");
			}
		}
	}
}

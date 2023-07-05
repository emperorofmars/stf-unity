using System;
using System.Collections.Generic;
using System.IO;
using stf.serialisation;
using UnityEngine;

namespace stf
{
	public static class STFSetup
	{
		public static List<UnityEngine.Object> SetupInplace(GameObject root, string resourcePath)
		{
			var ret = new List<UnityEngine.Object>();
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
			ret.AddRange(STFArmatureUtil.FindAndSetupArmatures(root));
			return ret;
		}
	}
}

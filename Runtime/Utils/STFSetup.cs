using System;
using System.Collections.Generic;
using System.IO;
using stf.serialisation;
using UnityEngine;

namespace stf
{
	public static class STFSetup
	{
		public static List<UnityEngine.Object> SetupStandaloneAssetInplace(GameObject root)
		{
			var ret = new List<UnityEngine.Object>();

			// Setup main asset info
			if(root.GetComponent<STFAssetInfo>() == null)
			{
				var asset = root.AddComponent<STFAssetInfo>();
				asset.assetId = Guid.NewGuid().ToString();
				asset.assetType = "asset";
				asset.assetName = root.name;
			}

			// Setup Id's for all gameobjects and suitable components
			foreach(var c in root.GetComponentsInChildren<Transform>())
			{
				var id = c.gameObject.GetComponent<STFUUID>();
				if(id == null)
				{
					id = c.gameObject.AddComponent<STFUUID>();
					id.id = Guid.NewGuid().ToString();
				}
				// handle supported non stf components
				var skm = c.gameObject.GetComponent<SkinnedMeshRenderer>();
				if(skm != null)
				{
					if(id.componentIds.Find(i => i.component == skm) == null) id.componentIds.Add(new STFUUID.ComponentIdMapping{component = skm, id = Guid.NewGuid().ToString()});
				}
			}
			ret.AddRange(STFArmatureUtil.FindAndSetupArmatures(root));
			return ret;
		}
		
		// TODO: seperate function to determine some components from naming, like twistbones and such
	}
}

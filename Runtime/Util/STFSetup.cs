using System;
using System.Collections.Generic;
using System.IO;
using stf.serialisation;
using UnityEngine;

namespace stf
{
	// Utility to setup a non STF Unity scene into the STF Unity intermediary format. Adds uuid's to everything, determines armatures and places the appropriate STF components. Will be used by the default exporter automatically if the exported scene is not set up.

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
				asset.assetType = STFAssetExporter._TYPE;
				asset.assetName = root.name;
			}
			SetupIds(root);

			ret.AddRange(STFArmatureUtil.FindAndSetupArmatures(root));
			return ret;
		}

		public static List<UnityEngine.Object> SetupAddonAssetInplace(GameObject root)
		{
			var ret = new List<UnityEngine.Object>();
			if(root.GetComponent<STFAssetInfo>() == null)
			{
				var asset = root.AddComponent<STFAssetInfo>();
				asset.assetId = Guid.NewGuid().ToString();
				asset.assetType = STFAddonAssetExporter._TYPE;
				asset.assetName = root.name;
			}
			for(int i = 0; i < root.transform.childCount; i++)
			{
				var child = root.transform.GetChild(i).gameObject;
				SetupIds(child);
			}

			ret.AddRange(STFArmatureUtil.FindAndSetupArmatures(root));
			STFArmatureUtil.FindAndSetupExternalArmatures(root);
			return ret;
		}

		public static void SetupIds(GameObject root)
		{
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
		}
		
		// TODO: seperate optional function to determine some components from naming, like twistbones and such
	}
}

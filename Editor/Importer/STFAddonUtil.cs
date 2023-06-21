
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using stf.Components;
using UnityEditor;
using UnityEngine;
using static stf.STFScriptedImporter;

namespace stf.serialisation
{
	[Serializable]
	public class AddonExternal
	{
		public STFMeta Origin;
		public STFMeta.AssetInfo Addon;
		public string TargetId;
	}

	public static class STFAddonUtil
	{
		public static List<AddonExternal> GatherAddons(STFMeta exclude)
		{
			var ret = new List<AddonExternal>();
			string[] externalGuids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(STFMeta)));
			foreach(var guid in externalGuids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var meta = AssetDatabase.LoadAssetAtPath<STFMeta>(path);
				if(meta != null)
				{
					foreach(var externalAsset in meta.importedRawAssets)
					{
						if(externalAsset.assetType == "addon" && exclude.importedRawAssets.Find(a => a.assetId == externalAsset.assetId) == null)
						{
							var addonInfo = ((GameObject)externalAsset.assetRoot)?.GetComponent<STFAddonAssetInfo>();
							if(addonInfo != null && exclude.importedRawAssets.Find(a => a.assetId == addonInfo.targetAssetId) != null)
							{
								ret.Add(new AddonExternal{Origin = meta, Addon = externalAsset, TargetId = addonInfo.targetAssetId});
							}
						}
					}
				}
			}
			return ret;
		}
	}
}

#endif

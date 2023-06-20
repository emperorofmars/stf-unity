
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using stf.serialisation;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace stf
{
	public static class STFAddonRegistry
	{
		[Serializable]
		private class RegisteredAddon
		{
			public string TargetId;
			public STFAssetInfo AddonAsset;
		}

		private static List<RegisteredAddon> Addons = new List<RegisteredAddon>();

		public static void RegisterAddon(string targetId, STFAssetInfo asset)
		{
			Addons.RemoveAll(a => a.AddonAsset == null);

			var existing = Addons.Find(a => a.AddonAsset.assetId == asset.assetId);
			if(existing != null)
			{
				existing.AddonAsset = asset;
			}
			else
			{
				Addons.Add(new RegisteredAddon{TargetId = targetId, AddonAsset = asset});
			}
		}

		public static List<STFAssetInfo> GetAddons(string targetId)
		{
			Addons.RemoveAll(a => a.AddonAsset == null);

			return Addons.FindAll(a => a.TargetId == targetId).Select(a => a.AddonAsset).ToList();
		}
	}
}

#endif

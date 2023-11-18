
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serde
{
	public class STFAssetExporter : ISTFAssetExporter
	{
		public JObject SerializeToJson(STFExportState State, System.Object Asset)
		{
			var ret = new JObject();

			return ret;
		}
	}
	
	public class STFAssetImporter : ISTFAssetImporter
	{
		public static string _TYPE = "STF.asset";

		public UnityEngine.Object ParseFromJson(STFImportState State, JObject JsonAsset, string Id)
		{
			var assetImportState = new STFAssetImportState(
				new STFAssetInfo {
					assetId = Id,
					assetType = (string)JsonAsset["type"],
					assetName = (string)JsonAsset["name"],
					assetVersion = (string)JsonAsset["version"],
					assetAuthor = (string)JsonAsset["author"],
					assetURL = (string)JsonAsset["url"],
					assetLicense = (string)JsonAsset["license"],
					assetLicenseLink = (string)JsonAsset["license_link"]
				},
				State,
				State.Context
			);
			var rootId = (string)JsonAsset["root_node"];
			var nodeJson = (JObject)State.JsonRoot["nodes"][rootId];
			var type = (string)nodeJson["type"];
			if(type == null || type.Length == 0) type = STFNodeImporter._TYPE;

			if(assetImportState.Context.NodeImporters.ContainsKey(type))
			{
				Debug.Log($"Parsing Node: {type}");
				var rootGo = assetImportState.Context.NodeImporters[type].ParseFromJson(assetImportState, nodeJson, rootId);

				var assetComponent = rootGo.AddComponent<STFAsset>();
				assetComponent.assetInfo = assetImportState.AssetInfo;

				if(State.MainAssetId == Id)
				{
					PrefabUtility.SaveAsPrefabAsset(rootGo, Path.Combine(State.TargetLocation, assetImportState.AssetInfo.assetName + ".Prefab"));
				}
				else
				{
					PrefabUtility.SaveAsPrefabAsset(rootGo, Path.Combine(State.TargetLocation, STFConstants.SecondaryAssetsDirectoryName, assetImportState.AssetInfo.assetName + ".Prefab"));
				}
			}
			else
			{
				Debug.LogWarning($"Unrecognized Node: {type}");
				// Unrecognized Node
			}
			return null;
		}
	}
}

#endif

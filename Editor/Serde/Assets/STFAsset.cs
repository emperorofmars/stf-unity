
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
		public string SerializeToJson(STFExportState State, STFAsset Asset)
		{
			var ret = new JObject
			{
				{"type", STFAssetImporter._TYPE},
				{"name", Asset.assetInfo.assetName},
				{"version", Asset.assetInfo.assetVersion},
				{"author", Asset.assetInfo.assetAuthor},
				{"url", Asset.assetInfo.assetURL},
				{"license", Asset.assetInfo.assetLicense},
				{"license_link", Asset.assetInfo.assetLicenseLink}
			};
			if(Asset.assetInfo.assetPreview) ret.Add("preview", STFSerdeUtil.SerializeResource(State, Asset.assetInfo.assetPreview));

			ret.Add("root_node", STFSerdeUtil.SerializeNode(State, Asset.gameObject));

			return State.AddAsset(Asset, ret, Asset.assetInfo.assetId);
		}
	}
	
	public class STFAssetImporter : ISTFAssetImporter
	{
		public static string _TYPE = "STF.asset";

		public void ParseFromJson(STFImportState State, JObject JsonAsset, string Id)
		{
			try
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
				if(type == null || type.Length == 0) type = STFNode._TYPE;

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
			}
			catch(Exception e)
			{
				throw new Exception("Error during Asset import: ", e);
			}
			finally
			{
				foreach(var trashObject in State.Trash)
				{
					if(trashObject != null)
					{
						UnityEngine.Object.DestroyImmediate(trashObject);
					}
				}
			}
			return;
		}
	}
}

#endif

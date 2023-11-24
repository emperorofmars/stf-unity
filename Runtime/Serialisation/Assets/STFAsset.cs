
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using System.IO;

namespace STF.Serialisation
{
	public class STFAssetExporter : ISTFAssetExporter
	{
		public string SerializeToJson(ISTFExportState State, STFAsset Asset)
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
			if(Asset.assetInfo.assetPreview) ret.Add("preview", SerdeUtil.SerializeResource(State, Asset.assetInfo.assetPreview));

			ret.Add("root_node", SerdeUtil.SerializeNode(State, Asset.gameObject));

			return State.AddAsset(Asset, ret, Asset.assetInfo.assetId);
		}
	}
	
	public class STFAssetImporter : ISTFAssetImporter
	{
		public const string _TYPE = "STF.asset";

		public void ParseFromJson(ISTFImportState State, JObject JsonAsset, string Id)
		{
			STFAssetImportState assetImportState = null;
			try
			{
				assetImportState = new STFAssetImportState(
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
					var rootGo = assetImportState.Context.NodeImporters[type].ParseFromJson(assetImportState, nodeJson, rootId);

					var assetComponent = rootGo.AddComponent<STFAsset>();
					assetComponent.assetInfo = assetImportState.AssetInfo;

					State.SaveAsset(rootGo, assetImportState.AssetInfo.assetName, State.MainAssetId == Id);
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
			return;
		}
	}
}

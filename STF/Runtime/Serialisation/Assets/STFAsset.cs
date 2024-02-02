
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFAsset : ISTFAsset
	{
		public const string _TYPE = "STF.asset";
		public override string Type => _TYPE;
		public List<string> appliedAddonIds = new List<string>();
		public Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
	}

	public class STFAssetExporter : ISTFAssetExporter
	{
		public string SerializeToJson(ISTFExportState State, ISTFAsset Asset)
		{
			var ret = new JObject
			{
				{"type", STFAsset._TYPE},
				{"name", Asset.Name},
				{"version", Asset.Version},
				{"author", Asset.Author},
				{"url", Asset.URL},
				{"license", Asset.License},
				{"license_link", Asset.LicenseLink}
			};
			if(Asset.Preview) ret.Add("preview", SerdeUtil.SerializeResource(State, Asset.Preview));

			ret.Add("root_node", SerdeUtil.SerializeNode(State, Asset.gameObject));

			return State.AddAsset(Asset, ret, Asset.Id);
		}
	}
	
	public class STFAssetImporter : ISTFAssetImporter
	{
		public void ParseFromJson(ISTFImportState State, JObject JsonAsset, string Id)
		{
			try
			{
				var rootId = (string)JsonAsset["root_node"];
				var nodeJson = (JObject)State.JsonRoot["nodes"][rootId];

				var assetImportState = new STFAssetImportState(Id, State, State.Context);
				var nodeType = (string)nodeJson["type"] != null && ((string)nodeJson["type"]).Length > 0 ? (string)nodeJson["type"] : STFNode._TYPE;
				
				GameObject rootGo;
				if(State.Context.NodeImporters.ContainsKey(nodeType))
				{
					rootGo = State.Context.NodeImporters[nodeType].ParseFromJson(assetImportState, nodeJson, rootId);
				}
				else
				{
					rootGo = STFUnrecognizedNodeImporter.ParseFromJson(assetImportState, nodeJson, rootId);
				}

				var asset = rootGo.AddComponent<STFAsset>();
				asset.Id = Id;
				asset.Name = (string)JsonAsset["name"];
				asset.Version = (string)JsonAsset["version"];
				asset.Author = (string)JsonAsset["author"];
				asset.URL = (string)JsonAsset["url"];
				asset.License = (string)JsonAsset["license"];
				asset.LicenseLink = (string)JsonAsset["license_link"];

				if(JsonAsset["preview"] != null)
				{
					asset.Preview = (Texture2D)(State.Resources[(string)JsonAsset["preview"]] as ISTFResource).Resource;
				}

				asset.ImportPath = State.TargetLocation;

				State.SaveAsset(asset);
			}
			catch(Exception e)
			{
				throw new Exception("Error during Asset import: ", e);
			}
			return;
		}
	}
}

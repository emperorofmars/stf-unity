
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using System.IO;
using System.Collections.Generic;

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
				var rootGo = State.Context.NodeImporters[(string)nodeJson["type"]].ParseFromJson(assetImportState, nodeJson, rootId);
				
				var asset = rootGo.AddComponent<STFAsset>();
				asset.Id = Id;
				asset.Name = (string)JsonAsset["name"];
				asset.Version = (string)JsonAsset["version"];
				asset.Author = (string)JsonAsset["author"];
				asset.URL = (string)JsonAsset["url"];
				asset.License = (string)JsonAsset["license"];
				asset.LicenseLink = (string)JsonAsset["license_link"];

				State.SaveAsset(rootGo, asset.Name, State.MainAssetId == Id);

				var type = (string)nodeJson["type"];
				if(type == null || type.Length == 0) type = STFNode._TYPE;
			}
			catch(Exception e)
			{
				throw new Exception("Error during Asset import: ", e);
			}
			return;
		}
	}
}

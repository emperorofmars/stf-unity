
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using STF.Serialisation;
using STF.Util;

namespace STF.Types
{
	public class STFAsset : ISTFAsset
	{
		public const string _TYPE = "STF.asset";
		public override string Type => _TYPE;
		public Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta = new();
	}

	public class STFAssetExporter : ISTFAssetExporter
	{
		public JObject SerializeToJson(STFExportState State, ISTFAsset Asset)
		{
			var ret = new JObject
			{
				{"id", Asset.Id},
				{"type", STFAsset._TYPE},
				{"name", Asset.STFName},
				{"version", Asset.Version},
				{"author", Asset.Author},
				{"url", Asset.URL},
				{"license", Asset.License},
				{"license_link", Asset.LicenseLink}
			};
			var rf = new RefSerializer(ret);

			if(Asset.Preview) ret.Add("preview", rf.ResourceRef(ExportUtil.SerializeResource(State, Asset.Preview)));

			ret.Add("root_node", rf.NodeRef(ExportUtil.SerializeNode(State, Asset.gameObject)));

			return ret;
		}
	}
	
	public class STFAssetImporter : ISTFAssetImporter
	{
		public ISTFAsset ParseFromJson(STFImportState State, JObject JsonAsset)
		{
			GameObject rootGo = null;
			try
			{
				var rf = new RefDeserializer(JsonAsset);

				var rootId = rf.NodeRef(JsonAsset["root_node"]);
				var nodeJson = (JObject)State.JsonRoot["nodes"][rootId];

				var nodeType = (string)nodeJson["type"] != null && ((string)nodeJson["type"]).Length > 0 ? (string)nodeJson["type"] : STFNode._TYPE;
				
				rootGo = State.Context.GetNodeImporter(nodeType).ParseFromJson(State, nodeJson, rootId);

				var asset = rootGo.AddComponent<STFAsset>();
				asset.Id = (string)JsonAsset["id"];
				asset.STFName = (string)JsonAsset["name"];
				asset.Version = (string)JsonAsset["version"];
				asset.Author = (string)JsonAsset["author"];
				asset.URL = (string)JsonAsset["url"];
				asset.License = (string)JsonAsset["license"];
				asset.LicenseLink = (string)JsonAsset["license_link"];

				if(JsonAsset["preview"] != null)
				{
					asset.Preview = (Texture2D)State.Resources[rf.ResourceRef(JsonAsset["preview"])].Resource;
				}

				return asset;
			}
			catch(Exception e)
			{
				if(rootGo != null) UnityEngine.Object.DestroyImmediate(rootGo);
				throw new Exception("Error during Asset import: ", e);
			}
		}
	}
}


using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace STF.Serialisation
{
	public class STFAddonAsset : ISTFAsset
	{
		public const string _TYPE = "STF.addon_asset";
		public override string Type => _TYPE;
		
		// Give information onto which asset this addon can be applied. Very temporary implementation, should be more thought out.
		// Possibilities: Restrict to ID(s), to type(s), check if a type/ID or something is present, etc...
		public List<string> TargetConstraints = new List<string>();
		
		public Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
	}

	public class STFAddonAssetExporter : ISTFAssetExporter
	{
		public string SerializeToJson(ISTFExportState State, ISTFAsset Asset)
		{
			var ret = new JObject
			{
				{"type", STFAddonAsset._TYPE},
				{"name", Asset.Name},
				{"version", Asset.Version},
				{"author", Asset.Author},
				{"url", Asset.URL},
				{"license", Asset.License},
				{"license_link", Asset.LicenseLink}
			};
			if(Asset.Preview) ret.Add("preview", SerdeUtil.SerializeResource(State, Asset.Preview));

			for(int i = 0; i < Asset.gameObject.transform.childCount; i++)
			{
				var child = Asset.gameObject.transform.GetChild(i);
				var childNodeInfo = child.GetComponents<ASTFNode>();
				foreach(var nodeInfo in childNodeInfo)
				{
					if(nodeInfo.Type != STFPatchNode._TYPE/* || nodeInfo.Type != STFAppendageNode._TYPE*/)
					{
						throw new Exception("Addon Asset can only containt root nodes of the types '" + STFPatchNode._TYPE + "'"/* or '" + STFAppendageNode._TYPE + "' !"*/);
					}
				}
			}
			ret.Add("root_node", SerdeUtil.SerializeNode(State, Asset.gameObject));

			return State.AddAsset(Asset, ret, Asset.Id);
		}
	}
	
	public class STFAddonAssetImporter : ISTFAssetImporter
	{
		public void ParseFromJson(ISTFImportState State, JObject JsonAsset, string Id)
		{
			try
			{
				var rootId = (string)JsonAsset["root_node"];
				var nodeJson = (JObject)State.JsonRoot["nodes"][rootId];

				var assetImportState = new STFAssetImportState(Id, State, State.Context);
				var nodeType = (string)nodeJson["type"] != null && ((string)nodeJson["type"]).Length > 0 ? (string)nodeJson["type"] : STFNode._TYPE;
				// Check node type validity
				var rootGo = State.Context.NodeImporters[nodeType].ParseFromJson(assetImportState, nodeJson, rootId);
				
				var asset = rootGo.AddComponent<STFAddonAsset>();
				asset.Id = Id;
				asset.Name = (string)JsonAsset["name"];
				asset.Version = (string)JsonAsset["version"];
				asset.Author = (string)JsonAsset["author"];
				asset.URL = (string)JsonAsset["url"];
				asset.License = (string)JsonAsset["license"];
				asset.LicenseLink = (string)JsonAsset["license_link"];

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

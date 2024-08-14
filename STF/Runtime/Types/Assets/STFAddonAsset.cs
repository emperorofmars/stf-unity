
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using STF.Serialisation;
using STF.Util;
using UnityEngine;

namespace STF.Types
{
	public class STFAddonAsset : ISTFAsset
	{
		public const string _TYPE = "STF.addon_asset";
		public override string Type => _TYPE;
		
		// Give information onto which asset this addon can be applied. Very temporary implementation, should be more thought out.
		// Possibilities: Restrict to ID(s), to type(s), check if a type/ID or something is present, etc...
		public List<string> TargetConstraints = new();
	}

	public class STFAddonAssetExporter : ISTFAssetExporter
	{
		public JObject SerializeToJson(STFExportState State, ISTFAsset Asset)
		{
			var (ret, rf) = Asset.SerializeDefaultValuesToJson(State);

			var rootNodes = new JArray();

			for(int i = 0; i < Asset.gameObject.transform.childCount; i++)
			{
				var child = Asset.gameObject.transform.GetChild(i);
				var childNodeInfo = child.GetComponents<ISTFNode>();
				foreach(var nodeInfo in childNodeInfo)
				{
					if(nodeInfo.Type != STFPatchNode._TYPE && nodeInfo.Type != STFAppendageNode._TYPE)
					{
						throw new Exception($"Addon Asset can only containt root nodes of the types '{ STFPatchNode._TYPE }' or '{ STFAppendageNode._TYPE }' !\nWrong node type: {nodeInfo.Type}");
					}
					else
					{
						rootNodes.Add(rf.NodeRef(ExportUtil.SerializeNode(State, child.gameObject)));
					}
				}
			}
			ret.Add("root_nodes", rootNodes);

			return ret;
		}
	}
	
	public class STFAddonAssetImporter : ISTFAssetImporter
	{
		public ISTFAsset ParseFromJson(STFImportState State, JObject JsonAsset)
		{
			GameObject rootGo = new GameObject();
			try
			{
				var rf = new RefDeserializer(JsonAsset);

				foreach(var nodeIdx in JsonAsset["root_nodes"])
				{
					var childGo = ImportUtil.ParseNode(State, rf.NodeRef(nodeIdx));
					childGo.transform.parent = rootGo.transform;
				}
				
				var asset = rootGo.AddComponent<STFAddonAsset>();
				asset.ParseDefaultValuesFromJson(State, JsonAsset, rf);
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

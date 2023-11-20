
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serde
{
	public class STFNode : ASTFNode
	{
		public static string _TYPE = "STF.node";
		public override string Type => _TYPE;
	}

	public class STFNodeExporter : ISTFNodeExporter
	{
		public JObject SerializeToJson(STFExportState State, GameObject Go)
		{
			throw new NotImplementedException();
		}
	}

	public class STFNodeImporter : ISTFNodeImporter
	{
		public GameObject ParseFromJson(ISTFAssetImportState State, JObject JsonAsset, string Id)
		{
			var ret = new GameObject();
			State.AddNode(ret, Id);
			
			var node = ret.AddComponent<STFNode>();
			node.NodeId = Id;
			node.name = (String)JsonAsset["name"];
			node.Origin = State.AssetInfo.assetId;

			TRSUtil.ParseTRS(ret, JsonAsset);
			STFNodeUtil.Parse(State, ret, JsonAsset);
			return ret;
		}
	}
}

#endif

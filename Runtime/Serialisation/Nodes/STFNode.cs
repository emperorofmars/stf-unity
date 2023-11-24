
using System;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFNode : ASTFNode
	{
		public const string _TYPE = "STF.node";
		public override string Type => _TYPE;
	}

	public class STFNodeExporter : ISTFNodeExporter
	{
		public string SerializeToJson(ISTFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFNode>();
			var ret = new JObject
			{
				{"type", STFNode._TYPE},
				{"name", Go.name},
				{"trs", TRSUtil.SerializeTRS(Go)},
				{"children", SerdeUtil.SerializeChildren(State, Go)},
				{"components", SerdeUtil.SerializeComponents(State, Go.GetComponents<Component>())}
			};

			return State.AddNode(Go, ret, node.NodeId);
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
			SerdeUtil.ParseNode(State, ret, JsonAsset);
			return ret;
		}
	}
}


#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
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
		public string SerializeToJson(ISTFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFNode>();
			var ret = new JObject
			{
				{"type", STFNode._TYPE},
				{"name", Go.name},
				{"trs", TRSUtil.SerializeTRS(Go)},
				{"children", STFSerdeUtil.SerializeChildren(State, Go)},
				{"components", STFSerdeUtil.SerializeComponents(State, Go.GetComponents<Component>(), new List<Type> {typeof(Transform), typeof(ISTFNode), typeof(STFNode), typeof(Animator), typeof(STFAsset)})}
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
			STFSerdeUtil.ParseNode(State, ret, JsonAsset);
			return ret;
		}
	}
}

#endif

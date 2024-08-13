
using System;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFNode : ISTFNode
	{
		public const string _TYPE = "STF.node";
		public override string Type => _TYPE;
	}

	public class STFNodeExporter : ASTFNodeExporter
	{
		public override string SerializeToJson(STFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFNode>();
			var ret = new JObject
			{
				{"type", STFNode._TYPE},
				{"name", Go.name},
				{"trs", TRSUtil.SerializeTRS(Go)},
				{"children", SerdeUtil.SerializeChildren(State, Go)},
				{"components", SerdeUtil.SerializeNodeComponents(State, Go.GetComponents<Component>())}
			};

			return State.AddNode(Go, ret, node.Id);
		}
	}

	public class STFNodeImporter : ASTFNodeImporter
	{

		public override GameObject ParseFromJson(STFImportState State, JObject JsonAsset, string Id)
		{
			var ret = new GameObject();
			var node = ret.AddComponent<STFNode>();
			State.AddNode(node, Id);

			node.Id = Id;
			node.name = (string)JsonAsset["name"];
			node.Origin = State.AssetId;

			TRSUtil.ParseTRS(ret, JsonAsset);
			SerdeUtil.ParseNode(State, ret, JsonAsset);
			return ret;
		}
	}
}

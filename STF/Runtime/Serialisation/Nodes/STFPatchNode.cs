
using System;
using Newtonsoft.Json.Linq;
using STF.Util;
using STF_Util;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFPatchNode : ISTFNode
	{
		public const string _TYPE = "STF.node_patch";
		public override string Type => _TYPE;

		[Id] public string TargetId;
	}

	public class STFPatchNodeExporter : ASTFNodeExporter
	{
		public override string SerializeToJson(ISTFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFPatchNode>();
			var ret = new JObject
			{
				{"type", STFPatchNode._TYPE},
				{"name", Go.name},
				{"trs", TRSUtil.SerializeTRS(Go)},
				{"children", SerdeUtil.SerializeChildren(State, Go)},
				{"components", SerdeUtil.SerializeNodeComponents(State, Go.GetComponents<Component>())},
				{"target", node.TargetId},
			};

			return State.AddNode(Go, ret, node.Id);
		}
	}

	public class STFPatchNodeImporter : ASTFNodeImporter
	{

		public override GameObject ParseFromJson(STFImportState State, JObject JsonAsset, string Id)
		{
			var ret = new GameObject();
			State.AddNode(ret, Id);
			
			var node = ret.AddComponent<STFPatchNode>();
			node.Id = Id;
			node.name = (string)JsonAsset["name"];
			node.Origin = State.AssetId;
			node.TargetId = (string)JsonAsset["target"];

			TRSUtil.ParseTRS(ret, JsonAsset);
			SerdeUtil.ParseNode(State, ret, JsonAsset);
			return ret;
		}
	}
}

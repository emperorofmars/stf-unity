
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using STF.Util;
using STF_Util;
using UnityEngine;

namespace STF.Types
{
	public class STFAppendageNode : ISTFNode
	{
		public const string _TYPE = "STF.node_appendage";
		public override string Type => _TYPE;

		[Id] public string TargetId;
	}

	public class STFAppendageNodeExporter : ASTFNodeExporter
	{
		public override string SerializeToJson(STFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFAppendageNode>();
			var ret = new JObject
			{
				{"type", STFAppendageNode._TYPE},
				{"name", Go.name},
				{"trs", TRSUtil.SerializeTRS(Go)},
				{"children", ExportUtil.SerializeNodeChildren(State, Go)},
				{"components", ExportUtil.SerializeNodeComponents(State, Go.GetComponents<Component>())},
				{"target", node.TargetId},
			};

			return State.AddNode(Go, ret, node.Id);
		}
	}

	public class STFAppendageNodeImporter : ASTFNodeImporter
	{

		public override GameObject ParseFromJson(STFImportState State, JObject JsonAsset, string Id)
		{
			var ret = new GameObject();
			var node = ret.AddComponent<STFAppendageNode>();
			State.AddNode(node);

			node.Id = Id;
			node.name = (string)JsonAsset["name"];
			node.Origin = State.AssetId;
			node.TargetId = (string)JsonAsset["target"];

			TRSUtil.ParseTRS(ret, JsonAsset);
			ImportUtil.ParseNodeChildrenAndComponents(State, ret, JsonAsset);
			return ret;
		}
	}
}

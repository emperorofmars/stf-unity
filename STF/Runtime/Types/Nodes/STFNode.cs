
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using STF.Util;
using UnityEngine;

namespace STF.Types
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
				{"children", ExportUtil.SerializeNodeChildren(State, Go)},
				{"components", ExportUtil.SerializeNodeComponents(State, Go.GetComponents<Component>())}
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
			State.AddNode(node);

			node.Id = Id;
			node.name = (string)JsonAsset["name"];
			node.Origin = State.AssetId;

			TRSUtil.ParseTRS(ret, JsonAsset);
			ImportUtil.ParseNodeChildrenAndComponents(State, ret, JsonAsset);
			return ret;
		}
	}
}

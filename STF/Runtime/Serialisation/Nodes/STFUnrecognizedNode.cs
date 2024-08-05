
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFUnrecognizedNode : ISTFNode
	{
		public string _TYPE;
		public override string Type => _TYPE;

		[TextArea]
		public string PreservedJson;
		public List<Object> usedResources = new List<Object>();
		public List<(string, GameObject)> usedNodes = new List<(string, GameObject)>();
	}

	public static class STFUnrecognizedNodeExporter
	{
		public static string SerializeToJson(ISTFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFUnrecognizedNode>();
			var ret = new JObject
			{
				{"type", node._TYPE},
				{"name", Go.name},
				{"trs", TRSUtil.SerializeTRS(Go)},
				{"children", SerdeUtil.SerializeChildren(State, Go)},
				{"components", SerdeUtil.SerializeNodeComponents(State, Go.GetComponents<Component>())}
			};
			ret.Merge(JObject.Parse(node.PreservedJson));
			foreach(var usedResource in node.usedResources) SerdeUtil.SerializeResource(State, usedResource);
			foreach(var usedNode in node.usedNodes) SerdeUtil.SerializeNode(State, usedNode.Item2);

			return State.AddNode(Go, ret, node.Id);
		}
	}

	public static class STFUnrecognizedNodeImporter
	{
		public static GameObject ParseFromJson(ISTFImportState State, JObject JsonAsset, string Id)
		{
			var ret = new GameObject();
			State.AddNode(ret, Id);
			
			var node = ret.AddComponent<STFUnrecognizedNode>();
			node.Id = Id;
			node.name = (string)JsonAsset["name"];
			node.Origin = State.Asset.Id;
			
			node._TYPE = (string)JsonAsset["type"];
			node.PreservedJson = JsonAsset.ToString();
			if(JsonAsset["used_resources"] != null) foreach(string resourceId in JsonAsset["used_resources"])
			{
				node.usedResources.Add(State.Resources[resourceId]);
			}
			if(JsonAsset["used_nodes"] != null) foreach(string nodeId in JsonAsset["used_nodes"])
			{
				node.usedNodes.Add((nodeId, State.Nodes.ContainsKey(nodeId) ? State.Nodes[nodeId] : null));
			}

			TRSUtil.ParseTRS(ret, JsonAsset);
			SerdeUtil.ParseNode(State, ret, JsonAsset);
			return ret;
		}
	}
}

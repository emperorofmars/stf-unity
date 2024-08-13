
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFUnrecognizedNode : ISTFNode
	{
		public string _Type;
		public override string Type => _Type;

		[TextArea]
		public string PreservedJson;
		public List<ISTFResource> ReferencedResources = new List<ISTFResource>();
		public List<ISTFNode> ReferencedNodes = new List<ISTFNode>();
	}

	public static class STFUnrecognizedNodeExporter
	{
		public static string SerializeToJson(STFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFUnrecognizedNode>();
			var ret = JObject.Parse(node.PreservedJson);
			foreach(var usedResource in node.ReferencedResources) ExportUtil.SerializeResource(State, usedResource);
			foreach(var usedNode in node.ReferencedNodes) ExportUtil.SerializeNode(State, usedNode);
			return State.AddNode(Go, ret, node.Id);
		}
	}

	public static class STFUnrecognizedNodeImporter
	{
		public static GameObject ParseFromJson(STFImportState State, JObject JsonAsset, string Id)
		{
			var ret = new GameObject();
			var node = ret.AddComponent<STFUnrecognizedNode>();
			State.AddNode(node);

			node.Id = Id;
			node.name = (string)JsonAsset["name"];
			node.Origin = State.AssetId;
			
			node._Type = (string)JsonAsset["type"];
			node.PreservedJson = JsonAsset.ToString();
			State.AddTask(new Task(() => {
				if(JsonAsset[STFKeywords.Keys.References] != null)
				{
					if(JsonAsset[STFKeywords.Keys.References][STFKeywords.ObjectType.Resources] != null) foreach(string resourceId in JsonAsset[STFKeywords.Keys.References][STFKeywords.ObjectType.Resources])
					{
						node.ReferencedResources.Add(State.Resources[resourceId]);
					}
					if(JsonAsset[STFKeywords.Keys.References][STFKeywords.ObjectType.Nodes] != null) foreach(string nodeId in JsonAsset[STFKeywords.Keys.References][STFKeywords.ObjectType.Nodes])
					{
						node.ReferencedNodes.Add(State.Nodes[nodeId]);
					}
				}
			}));

			TRSUtil.ParseTRS(ret, JsonAsset);
			ImportUtil.ParseNodeChildrenAndComponents(State, ret, JsonAsset);
			return ret;
		}
	}
}

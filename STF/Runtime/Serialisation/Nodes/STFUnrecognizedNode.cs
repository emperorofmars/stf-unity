
using System.Collections.Generic;
using System.Threading.Tasks;
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
		public List<Object> ReferencedResources = new List<Object>();
		public List<GameObject> ReferencedNodes = new List<GameObject>();
	}

	public static class STFUnrecognizedNodeExporter
	{
		public static string SerializeToJson(STFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFUnrecognizedNode>();
			var ret = JObject.Parse(node.PreservedJson);
			foreach(var usedResource in node.ReferencedResources) SerdeUtil.SerializeResource(State, usedResource);
			foreach(var usedNode in node.ReferencedNodes) SerdeUtil.SerializeNode(State, usedNode);
			return State.AddNode(Go, ret, node.Id);
		}
	}

	public static class STFUnrecognizedNodeImporter
	{
		public static GameObject ParseFromJson(STFImportState State, JObject JsonAsset, string Id)
		{
			var ret = new GameObject();
			State.AddNode(ret, Id);
			
			var node = ret.AddComponent<STFUnrecognizedNode>();
			node.Id = Id;
			node.name = (string)JsonAsset["name"];
			node.Origin = State.AssetId;
			
			node._TYPE = (string)JsonAsset["type"];
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
			SerdeUtil.ParseNode(State, ret, JsonAsset);
			return ret;
		}
	}
}

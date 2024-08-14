
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using STF.Util;
using UnityEngine;

namespace STF.Types
{
	public class STFUnrecognizedAsset : ISTFAsset
	{
		public string _Type;
		public override string Type => _Type;

		[TextArea]
		public string PreservedJson;
		public List<ISTFResource> ReferencedResources = new();
		public List<ISTFNode> ReferencedNodes = new();
	}

	public class STFUnrecognizedAssetExporter : ISTFAssetExporter
	{
		/*public string SerializeToJson(STFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFUnrecognizedNode>();
			var ret = JObject.Parse(node.PreservedJson);
			foreach(var usedResource in node.ReferencedResources) ExportUtil.SerializeResource(State, usedResource);
			foreach(var usedNode in node.ReferencedNodes) ExportUtil.SerializeNode(State, usedNode);
			return State.AddNode(Go, ret, node.Id);
		}*/

		public JObject SerializeToJson(STFExportState State, ISTFAsset Asset)
		{
			throw new System.NotImplementedException();
		}
	}

	public class STFUnrecognizedAssetImporter : ISTFAssetImporter
	{
		/*public GameObject ParseFromJson(STFImportState State, JObject JsonAsset, string Id)
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
		}*/

		public ISTFAsset ParseFromJson(STFImportState State, JObject JsonAsset)
		{
			throw new System.NotImplementedException();
		}
	}
}

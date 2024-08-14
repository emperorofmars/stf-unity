
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
		public JObject SerializeToJson(STFExportState State, ISTFAsset Asset)
		{
			//var (ret, rf) = Asset.SerializeDefaultValuesToJson(State);
			var unrecognized = (STFUnrecognizedAsset)Asset;
			var ret = JObject.Parse(unrecognized.PreservedJson);
			
			foreach(var usedResource in unrecognized.ReferencedResources) ExportUtil.SerializeResource(State, usedResource);
			foreach(var usedNode in unrecognized.ReferencedNodes) ExportUtil.SerializeNode(State, usedNode);

			return ret;
		}
	}

	public class STFUnrecognizedAssetImporter : ISTFAssetImporter
	{
		public ISTFAsset ParseFromJson(STFImportState State, JObject JsonAsset)
		{
			var rootGo = new GameObject();
			var asset = rootGo.AddComponent<STFUnrecognizedAsset>();
			var rf = new RefDeserializer(JsonAsset);
			asset.ParseDefaultValuesFromJson(State, JsonAsset, rf);
			
			asset._Type = (string)JsonAsset["type"];
			asset.PreservedJson = JsonAsset.ToString();
			State.AddTask(new Task(() => {
				if(JsonAsset[STFKeywords.Keys.References] != null)
				{
					if(JsonAsset[STFKeywords.Keys.References][STFKeywords.ObjectType.Resources] != null) foreach(string resourceId in JsonAsset[STFKeywords.Keys.References][STFKeywords.ObjectType.Resources])
					{
						asset.ReferencedResources.Add(State.Resources[resourceId]);
					}
					if(JsonAsset[STFKeywords.Keys.References][STFKeywords.ObjectType.Nodes] != null) foreach(string nodeId in JsonAsset[STFKeywords.Keys.References][STFKeywords.ObjectType.Nodes])
					{
						asset.ReferencedNodes.Add(State.Nodes[nodeId]);
					}
				}
			}));

			return asset;
		}
	}
}


#if UNITY_EDITOR

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serde
{
	public class STFUnrecognizedNodeComponent : ASTFNodeComponent
	{
		public string _TYPE;
		public override string Type => _TYPE;

		[TextArea]
		public string PreservedJson;
		public List<Object> usedResources = new List<Object>();
		public List<(string, GameObject)> usedNodes = new List<(string, GameObject)>();
	}

	public class STFUnrecognizedNodeComponentExporter
	{
		public static (string, JObject) SerializeToJson(ISTFExportState State, Component Component)
		{
			var c = (STFUnrecognizedNodeComponent)Component;
			var ret = JObject.Parse(c.PreservedJson);
			//ASTFNodeComponentExporter.SerializeRelationships(c, ret);
			foreach(var usedResource in c.usedResources) STFSerdeUtil.SerializeResource(State, usedResource);
			foreach(var usedNode in c.usedNodes) STFSerdeUtil.SerializeNode(State, usedNode.Item2);
			return (c.Id, ret);
		}
	}

	public class STFUnrecognizedNodeComponentImporter
	{
		public static void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<STFUnrecognizedNodeComponent>();
			c.Id = Id;
			//ASTFNodeComponentImporter.ParseRelationships(Json, c);
			c._TYPE = (string)Json["type"];
			c.PreservedJson = Json.ToString();
			if(Json["used_resources"] != null) foreach(string resourceId in Json["used_resources"])
			{
				c.usedResources.Add(State.Resources[resourceId]);
			}
			if(Json["used_nodes"] != null) foreach(string nodeId in Json["used_nodes"])
			{
				c.usedNodes.Add((nodeId, State.Nodes.ContainsKey(nodeId) ? State.Nodes[nodeId] : null));
			}
		}
	}
}

#endif

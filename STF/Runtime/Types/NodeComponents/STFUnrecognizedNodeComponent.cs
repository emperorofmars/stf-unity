
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using STF.Util;
using UnityEngine;

namespace STF.Types
{
	public class STFUnrecognizedNodeComponent : ISTFNodeComponent
	{
		public string _TYPE;
		public override string Type => _TYPE;

		[TextArea]
		public string PreservedJson;
		public List<ISTFResource> ReferencedResources = new();
		public List<ISTFNode> ReferencedNodes = new();
		public List<ISTFNodeComponent> ReferencedNodeComponentss = new();
	}

	public class STFUnrecognizedNodeComponentExporter : ISTFNodeComponentExporter
	{
		public string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty)
		{
			return UnityProperty;
		}

		public (string, JObject) SerializeToJson(STFExportState State, Component Component)
		{
			var c = (STFUnrecognizedNodeComponent)Component;
			var ret = JObject.Parse(c.PreservedJson);
			foreach(var usedResource in c.ReferencedResources) ExportUtil.SerializeResource(State, usedResource);
			foreach(var usedNode in c.ReferencedNodes) ExportUtil.SerializeNode(State, usedNode);
			return (c.Id, ret);
		}
	}

	public class STFUnrecognizedNodeComponentImporter : ISTFNodeComponentImporter
	{
		public string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty)
		{
			return STFProperty;
		}

		public void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go)
		{
			Debug.LogWarning($"Unrecognized node component type: {Json[STFKeywords.Keys.Type]}");

			var c = Go.AddComponent<STFUnrecognizedNodeComponent>();
			c.Id = Id;
			//ASTFNodeComponentImporter.ParseRelationships(Json, c);
			c._TYPE = (string)Json["type"];
			c.PreservedJson = Json.ToString();
			State.AddTask(new Task(() => {
				if(Json[STFKeywords.Keys.References] != null)
				{
					if(Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Resources] != null) foreach(string resourceId in Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Resources])
					{
						c.ReferencedResources.Add(State.Resources[resourceId]);
					}
					if(Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Nodes] != null) foreach(string nodeId in Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Nodes])
					{
						c.ReferencedNodes.Add(State.Nodes[nodeId]);
					}
					if(Json[STFKeywords.Keys.References][STFKeywords.ObjectType.NodeComponents] != null) foreach(string nodeComponentId in Json[STFKeywords.Keys.References][STFKeywords.ObjectType.NodeComponents])
					{
						c.ReferencedNodeComponentss.Add(State.NodeComponents[nodeComponentId]);
					}
				}
			}));
			State.AddNodeComponent(c);
		}
	}
}

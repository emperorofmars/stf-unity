
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFUnrecognizedNodeComponent : ISTFNodeComponent
	{
		public string _TYPE;
		public override string Type => _TYPE;

		[TextArea]
		public string PreservedJson;
		public List<Object> ReferencedResources = new List<Object>();
		public List<GameObject> ReferencedNodes = new List<GameObject>();
		public List<Component> ReferencedNodeComponentss = new List<Component>();
	}

	public class STFUnrecognizedNodeComponentExporter
	{
		public static (string, JObject) SerializeToJson(STFExportState State, Component Component)
		{
			var c = (STFUnrecognizedNodeComponent)Component;
			var ret = JObject.Parse(c.PreservedJson);
			//ASTFNodeComponentExporter.SerializeRelationships(c, ret);
			foreach(var usedResource in c.ReferencedResources) SerdeUtil.SerializeResource(State, usedResource);
			foreach(var usedNode in c.ReferencedNodes) SerdeUtil.SerializeNode(State, usedNode);
			// no need to explicitely export node components
			return (c.Id, ret);
		}
	}

	public class STFUnrecognizedNodeComponentImporter
	{
		public static void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go)
		{
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
			State.AddNodeComponent(c, Id);
		}
	}
}

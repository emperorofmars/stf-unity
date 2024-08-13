
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using STF.Util;
using STF.Serialisation;

namespace STF.Types
{
	public class STFBuffer : ScriptableObject
	{
		public string Id;
		[HideInInspector] public byte[] Data;
	}

	public class STFUnrecognizedResource : ISTFResource
	{
		public string _Type;
		public override string Type => _Type;
		[TextArea] public string PreservedJson;
		public List<ISTFResource> ReferencedResources = new List<ISTFResource>();
		public List<ISTFResourceComponent> ReferencedResourceComponents = new List<ISTFResourceComponent>();
		public List<ISTFNode> ReferencedNodes = new List<ISTFNode>();
		public List<STFBuffer> PreservedBuffers = new List<STFBuffer>();
	}

	public class STFUnrecognizedResourceExporter
	{
		public static string SerializeToJson(STFExportState State, Object Resource)
		{
			var r = (STFUnrecognizedResource)Resource;
			foreach(var usedResource in r.ReferencedResources) ExportUtil.SerializeResource(State, usedResource);
			foreach(var referencedResourceComponent in r.ReferencedResourceComponents) ExportUtil.SerializeResourceComponent(State, referencedResourceComponent);
			foreach(var usedNode in r.ReferencedNodes) ExportUtil.SerializeNode(State, usedNode);
			foreach(var usedbuffer in r.PreservedBuffers) State.AddBuffer(usedbuffer.Data, usedbuffer.Id);

			return State.AddResource(r, JObject.Parse(r.PreservedJson), r.Id);
		}
	}

	public class STFUnrecognizedResourceImporter
	{
		public static void ParseFromJson(STFImportState State, JObject Json, string Id)
		{
			var ret = ScriptableObject.CreateInstance<STFUnrecognizedResource>();
			ret.Id = Id;
			ret.Name = (string)Json["name"];
			ret.name = ret.Name + "_" + Id;
			ret._Type = (string)Json["type"];
			ret.PreservedJson = Json.ToString();
			
			State.AddPostprocessTask(new Task(() => {
				if(Json[STFKeywords.Keys.References] != null)
				{
					if(Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Resources] != null) foreach(string resourceId in Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Resources])
					{
						ret.ReferencedResources.Add(State.Resources[resourceId]);
					}
					if(Json[STFKeywords.Keys.References][STFKeywords.ObjectType.ResourceComponents] != null) foreach(string resourceComponentId in Json[STFKeywords.Keys.References][STFKeywords.ObjectType.ResourceComponents])
					{
						ret.ReferencedResourceComponents.Add(State.ResourceComponents[resourceComponentId]);
					}
					if(Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Nodes] != null) foreach(string nodeId in Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Nodes])
					{
						if(State.Nodes.ContainsKey(nodeId))
						{
							ret.ReferencedNodes.Add(State.Nodes[nodeId]);
						}
						else
						{
							/*var type = (string)State.JsonRoot[STFKeywords.ObjectType.Nodes][nodeId][STFKeywords.Keys.Type];
							if(type == null || type.Length == 0) type = STFNode._TYPE;
							var go = State.Context.NodeImporters[type].ParseFromJson(State, (JObject)State.JsonRoot[STFKeywords.ObjectType.Nodes][nodeId], nodeId);
							go.name = (string)State.JsonRoot[STFKeywords.ObjectType.Nodes][nodeId]["name"];
							State.UnityContext.SaveResource(go, ret, nodeId);*/
						}

						ret.ReferencedNodes.Add(State.Nodes[nodeId]);
					}
					if(Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Buffers] != null) foreach(string bufferId in Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Buffers])
					{
						var buffer = ScriptableObject.CreateInstance<STFBuffer>();
						buffer.Id = bufferId;
						buffer.Data = State.Buffers[bufferId];
						ret.PreservedBuffers.Add(buffer);
					}
				}
			}));
			State.AddResource(ret);
			ImportUtil.ParseResourceComponents(State, ret, Json);
		}
	}
}
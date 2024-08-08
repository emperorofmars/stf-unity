
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace STF.Serialisation
{
	public class STFBuffer : ScriptableObject
	{
		public string Id;
		[HideInInspector]
		public byte[] Data;
	}

	public class STFUnrecognizedResource : ISTFResource
	{
		[TextArea]
		public string PreservedJson;
		public List<STFBuffer> ReferencedBuffers = new List<STFBuffer>();
		public List<Object> ReferencedResources = new List<Object>();
		public List<GameObject> ReferencedNodes = new List<GameObject>();
	}

	public class STFUnrecognizedResourceExporter
	{
		public static string SerializeToJson(STFExportState State, UnityEngine.Object Resource)
		{
			var r = (STFUnrecognizedResource)Resource;
			foreach(var usedResource in r.ReferencedResources) SerdeUtil.SerializeResource(State, usedResource);
			foreach(var usedNode in r.ReferencedNodes) SerdeUtil.SerializeNode(State, usedNode);
			foreach(var usedbuffer in r.ReferencedBuffers) State.AddBuffer(usedbuffer.Data, usedbuffer.Id);

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
			ret.name = ret.Name;
			ret.PreservedJson = Json.ToString();

			
			State.AddPostprocessTask(new Task(() => {
				if(Json[STFKeywords.Keys.References] != null)
				{
					if(Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Resources] != null) foreach(string resourceId in Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Resources])
					{
						ret.ReferencedResources.Add(State.Resources[resourceId]);
					}
					if(Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Nodes] != null) foreach(string nodeId in Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Nodes])
					{
						if(State.Nodes.ContainsKey(nodeId))
						{
							ret.ReferencedNodes.Add(State.Nodes[nodeId]);
						}
						else
						{
							var type = (string)State.JsonRoot[STFKeywords.ObjectType.Nodes][nodeId][STFKeywords.Keys.Type];
							if(type == null || type.Length == 0) type = STFNode._TYPE;
							var go = State.Context.NodeImporters[type].ParseFromJson(State, (JObject)State.JsonRoot[STFKeywords.ObjectType.Nodes][nodeId], nodeId);
							go.name = (string)State.JsonRoot[STFKeywords.ObjectType.Nodes][nodeId]["name"];
							State.UnityContext.SaveResource(go, "Asset", nodeId);
						}

						ret.ReferencedNodes.Add(State.Nodes[nodeId]);
					}
					if(Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Buffers] != null) foreach(string bufferId in Json[STFKeywords.Keys.References][STFKeywords.ObjectType.Buffers])
					{
						var buffer = ScriptableObject.CreateInstance<STFBuffer>();
						buffer.Id = bufferId;
						buffer.Data = State.Buffers[bufferId];
						State.UnityContext.SaveResource(buffer, "Asset", bufferId);
						ret.ReferencedBuffers.Add(buffer);
					}
				}
			}));
			SerdeUtil.ParseResourceComponents(State, ret, Json);
			State.UnityContext.SaveResource(ret, "Asset", Id);
		}
	}
}

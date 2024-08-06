
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
		public List<STFBuffer> UsedBuffers = new List<STFBuffer>();
		public List<UnityEngine.Object> UsedResources = new List<Object>();
		public List<(string, GameObject)> UsedNodes = new List<(string, GameObject)>();
	}

	public class STFUnrecognizedResourceExporter
	{
		public static string SerializeToJson(STFExportState State, UnityEngine.Object Resource)
		{
			var r = (STFUnrecognizedResource)Resource;
			foreach(var resourceId in r.UsedResources) SerdeUtil.SerializeResource(State, resourceId);
			foreach(var usedNode in r.UsedNodes) SerdeUtil.SerializeNode(State, usedNode.Item2);
			foreach(var usedbuffer in r.UsedBuffers) State.AddBuffer(usedbuffer.Data, usedbuffer.Id);

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

			if(Json["used_buffers"] != null) foreach(string bufferId in Json["used_buffers"])
			{
				var buffer = ScriptableObject.CreateInstance<STFBuffer>();
				buffer.Id = bufferId;
				buffer.Data = State.Buffers[bufferId];
				State.UnityContext.SaveResource(buffer, "Asset", bufferId);
				ret.UsedBuffers.Add(buffer);
			}
			if(Json["used_resources"] != null) foreach(string resourceId in Json["used_resources"])
			{
				State.AddTask(new Task(() => {
					ret.UsedResources.Add(State.Resources[resourceId]);
				}));
			}
			if(Json["used_nodes"] != null) foreach(string nodeId in Json["used_nodes"])
			{
				var type = (string)State.JsonRoot["nodes"][nodeId]["type"];
				if(type == null || type.Length == 0) type = STFNode._TYPE;
				var go = State.Context.NodeImporters[type].ParseFromJson(State, (JObject)State.JsonRoot["nodes"][nodeId], nodeId);
				go.name = (string)State.JsonRoot["nodes"][nodeId]["name"];
				State.UnityContext.SaveResource(go, "Asset", nodeId);
				ret.UsedNodes.Add((nodeId, go));
			}
			SerdeUtil.ParseResourceComponents(State, ret, Json);
			State.UnityContext.SaveResource(ret, "Asset", Id);
		}
	}
}

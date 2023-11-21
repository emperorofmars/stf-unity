
#if UNITY_EDITOR

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using STF.IdComponents;

namespace STF.Serde
{
	public class STFBuffer : ScriptableObject
	{
		public string Id;
		[HideInInspector]
		public byte[] Data;
	}

	public class STFUnrecognizedResource : ScriptableObject
	{
		public string Id;
		[TextArea]
		public string PreservedJson;
		public List<STFBuffer> UsedBuffers;
		public List<UnityEngine.Object> UsedResources;
		public List<(string, GameObject)> UsedNodes = new List<(string, GameObject)>();
	}

	public class STFUnrecognizedResourceExporter
	{
		public static string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource)
		{
			var r = (STFUnrecognizedResource)Resource;

			//STFSerdeUtil.SerializeResource()
			return State.AddResource(r, JObject.Parse(r.PreservedJson), r.Id);
		}
	}

	public class STFUnrecognizedResourceImporter
	{
		public static void ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var ret = ScriptableObject.CreateInstance<STFUnrecognizedResource>();
			ret.Id = Id;
			ret.name = (string)Json["name"];
			ret.PreservedJson = Json.ToString();

			if(Json["used_buffers"] != null) foreach(string bufferId in Json["used_buffers"])
			{
				var buffer = ScriptableObject.CreateInstance<STFBuffer>();
				buffer.Id = bufferId;
				buffer.Data = State.Buffers[bufferId];
				State.SaveResource(buffer, "Asset", bufferId);
				ret.UsedBuffers.Add(buffer);
			}
			if(Json["used_resources"] != null) foreach(string resourceId in Json["used_resources"])
			{
				State.AddTask(new Task(() => {
					ret.UsedResources.Add(State.Resources[resourceId]);
				}));
			}
			var tmpAssetInfo = new STFAssetInfo();
			var assetImportState = new STFAssetImportState(tmpAssetInfo, State, State.Context);
			if(Json["used_nodes"] != null) foreach(string nodeId in Json["used_nodes"])
			{
				var type = (string)State.JsonRoot["nodes"][nodeId]["type"];
				if(type == null || type.Length == 0) type = STFNode._TYPE;
				var go = assetImportState.Context.NodeImporters[type].ParseFromJson(assetImportState, (JObject)State.JsonRoot["nodes"][nodeId], nodeId);
				go.name = (string)State.JsonRoot["nodes"][nodeId]["name"];
				State.SaveResource(go, "Asset", nodeId);
				ret.UsedNodes.Add((nodeId, go));
			}
			State.SaveResource(ret, "Asset", Id);
		}
	}
}

#endif

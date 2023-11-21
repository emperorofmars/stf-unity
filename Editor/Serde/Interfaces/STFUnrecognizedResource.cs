
#if UNITY_EDITOR

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

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
		public string PreservedJson;
		public List<STFBuffer> ReferencedBuffers;
		public List<UnityEngine.Object> ReferencedResources;
		public List<GameObject> ReferencedNodes;
	}

	public class STFUnrecognizedResourceExporter
	{
		public static string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource)
		{
			var r = ScriptableObject.CreateInstance<STFUnrecognizedResource>();
			
			return State.AddResource(r, null, r.Id);
		}
	}

	public class STFUnrecognizedResourceImporter
	{
		public static void ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var ret = ScriptableObject.CreateInstance<STFUnrecognizedResource>();
			ret.Id = Id;
			ret.PreservedJson = Json.ToString();

			State.SaveResource(ret, "Asset", Id);
		}
	}
}

#endif


using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serde
{
	public interface ISTFResourceExporter
	{
		List<GameObject> GatherUsedNodes(Object resource);
		//List<KeyValuePair<Object, Dictionary<string, System.Object>>> GatherResources(Object resource);
		List<Object> GatherUsedResources(Object resource);
		List<string> GatherUsedBuffers(Object resource);
		JToken SerializeToJson(STFExportState state, Object resource);
		string ConvertPropertyPath(string UnityProperty);
	}

	public interface ISTFResourceImporter
	{
		Object ParseFromJson(STFImportState state, JObject json, string id);
		string ConvertPropertyPath(string STFProperty);
	}
}

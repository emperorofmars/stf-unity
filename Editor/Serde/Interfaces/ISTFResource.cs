
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serde
{
	public interface ISTFResourceExporter
	{
		List<GameObject> GatherUsedNodes(Object Resource);
		//List<KeyValuePair<Object, Dictionary<string, System.Object>>> GatherResources(Object resource);
		List<Object> GatherUsedResources(Object Resource);
		List<string> GatherUsedBuffers(Object Resource);
		JToken SerializeToJson(STFExportState State, Object Resource);
		string ConvertPropertyPath(string UnityProperty);
	}

	public interface ISTFResourceImporter
	{
		Object ParseFromJson(STFImportState State, JObject Json, string Id);
		string ConvertPropertyPath(string STFProperty);
	}
}

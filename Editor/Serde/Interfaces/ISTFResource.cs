
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serde
{
	public interface ISTFResource
	{
		string Id {get; set;}
		string ResourceLocation {get; set;}
		string Name {get; set;}
	}

	public abstract class ASTFResource : ScriptableObject, ISTFResource
	{
		public string _id = System.Guid.NewGuid().ToString();
		public string Id {get => _id; set => _id = value;}

		public string _resourceLocation;
		public string ResourceLocation {get => _resourceLocation; set => _resourceLocation = value;}

		public string _name;
		public string Name {get => _name; set => _name = value;}
	}

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
		Object ParseFromJson(ISTFImportState State, JObject Json, string Id);
		string ConvertPropertyPath(string STFProperty);
	}
}

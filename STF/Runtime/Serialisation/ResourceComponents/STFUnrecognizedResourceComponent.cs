using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFUnrecognizedResourceComponent : ISTFResourceComponent
	{
		public string _Type;
		public override string Type => _Type;

		public string Json;
		public List<Object> UsedResources = new List<Object>();
	}
	
	public static class STFUnrecognizedResourceComponentExporter
	{
		public static string ConvertPropertyPath(string UnityProperty)
		{
			return UnityProperty;
		}

		public static (string Id, JObject JsonComponent) SerializeToJson(ISTFExportState State, ISTFResourceComponent Component)
		{
			return (Component.Id, JObject.Parse(((STFUnrecognizedResourceComponent)Component).Json));
		}
	}
	
	public static class STFUnrecognizedResourceComponentImporter
	{
		public static string ConvertPropertyPath(string STFProperty)
		{
			return STFProperty;
		}

		public static void ParseFromJson(ISTFImportState State, JObject Json, string Id, ISTFResource Resource)
		{
			var ret = ScriptableObject.CreateInstance<STFUnrecognizedResourceComponent>();
			ret.Id = Id;
			ret.Json = Json.ToString();
			ret.UsedResources = Json["used_resources"].ToObject<List<string>>().Select((string rid) => State.Resources[rid]).ToList();
			ret.Resource = Resource;
			Resource.Components.Add(ret);
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	[System.Serializable]
	public class STFUnrecognizedResourceComponent : ASTFResourceComponent
	{
		public string _Type;
		public override string Type => _Type;

		public Object _Resource;
		public override Object Resource => _Resource;

		public string Json;
		public List<Object> UsedResources = new List<Object>();
	}
	
	public class STFUnrecognizedResourceComponentExporter : ISTFResourceComponentExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new System.NotImplementedException();
		}

		public (string Id, JObject JsonComponent) SerializeToJson(ISTFExportState State, ISTFResourceComponent Component)
		{
			// handle resources
			return (Component.Id, JObject.Parse(((STFUnrecognizedResourceComponent)Component).Json));
		}

		public (string Json, List<ResourceIdPair> ResourceReferences) SerializeForUnity(ISTFResourceComponent Component)
		{
			return(((STFUnrecognizedResourceComponent)Component).Json, ((STFUnrecognizedResourceComponent)Component).UsedResources.Select(r => new ResourceIdPair{Id=null, Resource = r}).ToList());
		}
	}
	
	public class STFUnrecognizedResourceComponentImporter : ISTFResourceComponentImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id, ISTFResource Resource)
		{
			var ret = new STFUnrecognizedResourceComponent();
			ret.Id = Id;
			ret.Json = Json.ToString();
			ret.UsedResources = Json["used_resources"].ToObject<List<string>>().Select((string rid) => State.Resources[rid]).ToList();
			Resource.Components.Add(ret);
		}

		public ISTFResourceComponent DeserializeForUnity(string Json, string Id, List<ResourceIdPair> ResourceReferences)
		{
			var ret = new STFUnrecognizedResourceComponent();
			ret.Id = Id;
			ret.Json = Json;
			ret.UsedResources = ResourceReferences.Select(r => r.Resource).ToList();
			return ret;
		}
	}
}
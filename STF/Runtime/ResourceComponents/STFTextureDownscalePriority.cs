using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	[System.Serializable]
	public class STFTextureDownscalePriority : ISTFResourceComponent
	{
		public const string _TYPE = "STF.texture.downscale_priprity";
		public override string Type => _TYPE;

		public int DownscalePriority = 0;
	}
	
	public class STFTextureDownscalePriorityExporter : ISTFResourceComponentExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new System.NotImplementedException();
		}

		public (string Id, JObject JsonComponent) SerializeToJson(ISTFExportState State, ISTFResourceComponent Component)
		{
			var ret = new JObject {
				{"type", STFTextureDownscalePriority._TYPE},
				{"downscale_priority", ((STFTextureDownscalePriority)Component).DownscalePriority}
			};
			return (Component.Id, ret);
		}

		public (string Json, List<ResourceIdPair> ResourceReferences) SerializeForUnity(ISTFResourceComponent Component)
		{
			return(new JObject {
				{"type", STFTextureDownscalePriority._TYPE},
				{"downscale_priority", ((STFTextureDownscalePriority)Component).DownscalePriority}
			}.ToString(), null);
		}
	}
	
	public class STFTextureDownscalePriorityImporter : ISTFResourceComponentImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id, ISTFResource Resource)
		{
			var ret = new STFTextureDownscalePriority {
				Id = Id,
				DownscalePriority = (int)Json["downscale_priority"]
			};
			Resource.Components.Add(ret);
		}

		public ISTFResourceComponent DeserializeForUnity(string Json, string Id, List<ResourceIdPair> ResourceReferences)
		{
			var parsedJson = JObject.Parse(Json);
			return new STFTextureDownscalePriority{_Id = Id, DownscalePriority = (int)parsedJson["downscale_priority"]};
		}
	}
}
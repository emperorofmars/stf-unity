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
	}
	
	public class STFTextureDownscalePriorityImporter : ISTFResourceComponentImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id, ISTFResource Resource)
		{
			var ret = ScriptableObject.CreateInstance<STFTextureDownscalePriority>();
			ret.Id = Id;
			ret.DownscalePriority = (int)Json["downscale_priority"];
			State.AddResourceComponent(ret, Resource, Id);
		}
	}
}
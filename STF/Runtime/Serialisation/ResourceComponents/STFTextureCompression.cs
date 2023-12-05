using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	[System.Serializable]
	public class STFTextureCompression : ISTFResourceComponent
	{
		public const string _TYPE = "STF.texture.compression";
		public override string Type => _TYPE;

		public string Compression = "BC7";
	}
	
	public class STFTextureCompressionExporter : ISTFResourceComponentExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new System.NotImplementedException();
		}

		public (string Id, JObject JsonComponent) SerializeToJson(ISTFExportState State, ISTFResourceComponent Component)
		{
			var ret = new JObject {
				{"type", STFTextureCompression._TYPE},
				{"compression", ((STFTextureCompression)Component).Compression}
			};
			return (Component.Id, ret);
		}
	}
	
	public class STFTextureCompressionImporter : ISTFResourceComponentImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id, ISTFResource Resource)
		{
			var ret = ScriptableObject.CreateInstance<STFTextureCompression>();
			ret.Id = Id;
			ret.Compression = (string)Json["compression"];
			State.AddResourceComponent(ret, Resource, Id);
		}
	}
}
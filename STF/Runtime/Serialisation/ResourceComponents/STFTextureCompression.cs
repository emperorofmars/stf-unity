using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	[CreateAssetMenu(fileName = "STFTextureCompression", menuName = "STF/Resource Components/Texture Comporession", order = 1)]
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

		public (string Id, JObject JsonComponent) SerializeToJson(STFExportState State, ISTFResourceComponent Component)
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

		public void ParseFromJson(STFImportState State, JObject Json, string Id, ISTFResource Resource)
		{
			var ret = ScriptableObject.CreateInstance<STFTextureCompression>();
			ret.Id = Id;
			ret.Compression = (string)Json["compression"];
			State.AddResourceComponent(ret, Resource, Id);
		}
	}
}
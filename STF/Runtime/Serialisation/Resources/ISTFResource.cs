
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF_Util;
using UnityEngine;

namespace STF.Serialisation
{
	public abstract class ISTFResource : ScriptableObject
	{
		[Id] public string Id = System.Guid.NewGuid().ToString();
		public string Name;
		
		public UnityEngine.Object Resource;

		public string ResourceLocation;
		
		public List<ISTFResourceComponent> Components = new List<ISTFResourceComponent>();

	}

	public interface ISTFResourceExporter
	{
		string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null);
		string ConvertPropertyPath(ISTFExportState State, UnityEngine.Object Resource, string UnityProperty);
	}

	public interface ISTFResourceImporter
	{
		void ParseFromJson(ISTFImportState State, JObject Json, string Id);
		string ConvertPropertyPath(ISTFImportState State, UnityEngine.Object Resource, string STFProperty);
	}
}

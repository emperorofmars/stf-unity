
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF_Util;
using UnityEngine;

namespace STF.Serialisation
{
	public abstract class ISTFResource : ScriptableObject
	{
		public abstract string Type { get; }
		[Id] public string Id = System.Guid.NewGuid().ToString();
		public string Name;
		public Object Resource;
		public List<ISTFResourceComponent> Components = new List<ISTFResourceComponent>();
		public ISTFResource Fallback;
	}

	public interface ISTFResourceExporter
	{
		string SerializeToJson(STFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null);
		string ConvertPropertyPath(STFExportState State, UnityEngine.Object Resource, string UnityProperty);
	}

	public interface ISTFResourceImporter
	{
		void ParseFromJson(STFImportState State, JObject Json, string Id);
		string ConvertPropertyPath(STFImportState State, UnityEngine.Object Resource, string STFProperty);
	}
}

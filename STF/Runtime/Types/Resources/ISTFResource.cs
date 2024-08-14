
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using STF_Util;
using UnityEngine;

namespace STF.Types
{
	public abstract class ISTFResource : ScriptableObject, ISTFType
	{
		public abstract string Type { get; }
		public string Id { get => _Id; set => _Id = value; }
		[Id] public string _Id = System.Guid.NewGuid().ToString();
		public string STFName { get => _STFName; set => _STFName = value; }
		public string _STFName;
		public bool Degraded => _Degraded;
		bool _Degraded = false;
		
		public Object Resource;
		public readonly List<ISTFResourceComponent> Components = new();
		public ResourceReference Fallback = new();
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

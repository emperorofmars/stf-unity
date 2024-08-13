
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
		public string Name { get => _Name; set => _Name = value; }
		public string _Name;
		
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

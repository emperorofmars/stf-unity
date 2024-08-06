
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
		public Object Resource;
		public List<ISTFResourceComponent> Components = new List<ISTFResourceComponent>();

	}

	public interface ISTFResourceExporter
	{
		string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null);
		string ConvertPropertyPath(ISTFExportState State, UnityEngine.Object Resource, string UnityProperty);
	}

	public interface ISTFResourceImporter
	{
		void ParseFromJson(STFImportState State, JObject Json, string Id);
		string ConvertPropertyPath(STFImportState State, UnityEngine.Object Resource, string STFProperty);
	}
}

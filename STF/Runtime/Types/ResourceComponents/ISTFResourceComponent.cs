
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using STF.Util;
using UnityEngine;

namespace STF.Types
{
	public class ResourceIdPair { public string Id; public Object Resource;}

	public abstract class ISTFResourceComponent : ScriptableObject, ISTFType
	{
		public abstract string Type { get; }
		public string Id { get => _Id; set => _Id = value; }
		[Id] public string _Id = System.Guid.NewGuid().ToString();
		public string STFName { get => _STFName; set => _STFName = value; }
		public string _STFName;
		public bool Degraded => _Degraded;
		bool _Degraded = false;
		
		public List<string> Targets = new();
		[HideInInspector] public ResourceReference Resource = new();
	}
	
	public interface ISTFResourceComponentExporter
	{
		string ConvertPropertyPath(string UnityProperty);
		(string Id, JObject JsonComponent) SerializeToJson(STFExportState State, ISTFResourceComponent Component);
	}
	
	public interface ISTFResourceComponentImporter
	{
		string ConvertPropertyPath(string STFProperty);
		void ParseFromJson(STFImportState State, JObject Json, string Id, ISTFResource Resource);
	}
}

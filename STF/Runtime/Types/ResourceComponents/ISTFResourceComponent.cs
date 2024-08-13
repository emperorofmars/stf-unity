
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using STF.Types;
using STF_Util;
using UnityEngine;

namespace STF.Types
{
	public class ResourceIdPair { public string Id; public UnityEngine.Object Resource;}

	public abstract class ISTFResourceComponent : ScriptableObject, ISTFType
	{
		public abstract string Type { get; }
		public string Id { get => _Id; set => _Id = value; }
		[Id] public string _Id = System.Guid.NewGuid().ToString();
		public string Name { get => _Name; set => _Name = value; }
		public string _Name;
		
		public List<string> Targets = new List<string>();
		[HideInInspector] public ISTFResource Resource;
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

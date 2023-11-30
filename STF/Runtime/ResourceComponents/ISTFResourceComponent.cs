
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public class ResourceIdPair { public string Id; public UnityEngine.Object Resource;}
	public interface ISTFResourceComponent
	{
		string Id {get; set;}
		string Type {get;}
		List<string> Targets {get; set;}
		UnityEngine.Object Resource {get;}
	}

	[Serializable]
	public abstract class ASTFResourceComponent : ISTFResourceComponent
	{
		public string _Id = Guid.NewGuid().ToString();
		public string Id {get => _Id; set => _Id = value;}
		public abstract string Type { get; }

		public List<string> _targets = new List<string>();
		public List<string> Targets {get => _targets; set => _targets = value;}

		public abstract UnityEngine.Object Resource {get;}
	}
	
	public interface ISTFResourceComponentExporter
	{
		string ConvertPropertyPath(string UnityProperty);
		(string Id, JObject JsonComponent) SerializeToJson(ISTFExportState State, ISTFResourceComponent Component);
		
		(string Json, List<ResourceIdPair> ResourceReferences) SerializeForUnity(ISTFResourceComponent Component);
	}
	
	public interface ISTFResourceComponentImporter
	{
		string ConvertPropertyPath(string STFProperty);
		void ParseFromJson(ISTFImportState State, JObject Json, string Id, ISTFResource Resource);

		ISTFResourceComponent DeserializeForUnity(string Json, string Id, List<ResourceIdPair> ResourceReferences);
	}
}

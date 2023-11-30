
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
	}

	public abstract class ASTFResourceComponent : ScriptableObject, ISTFResourceComponent
	{
		public string _Id = Guid.NewGuid().ToString();
		public string Id {get => _Id; set => _Id = value;}
		public abstract string Type { get; }
		public string _ParentNodeId;
		public string ParentNodeId {get => _ParentNodeId; set => _ParentNodeId = value;}

		public List<string> _extends = new List<string>();
		public List<string> Extends {get => _extends; set => _extends = value;}

		public List<string> _overrides = new List<string>();
		public List<string> Overrides {get => _overrides; set => _overrides = value;}

		public List<string> _targets = new List<string>();
		public List<string> Targets {get => _targets; set => _targets = value;}
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
		void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, ISTFResource Resource);
		ISTFResourceComponent DeserializeForUnity(string Json, List<ResourceIdPair> ResourceReferences);
	}
}


using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public class ResourceIdPair { public string Id; public UnityEngine.Object Resource;}

	public abstract class ISTFResourceComponent : ScriptableObject
	{
		public string _Id = Guid.NewGuid().ToString();
		public string Id {get => _Id; set => _Id = value;}
		public abstract string Type { get; }

		public List<string> _targets = new List<string>();
		public List<string> Targets {get => _targets; set => _targets = value;}

		[HideInInspector] public ISTFResource Resource;
	}
	
	public interface ISTFResourceComponentExporter
	{
		string ConvertPropertyPath(string UnityProperty);
		(string Id, JObject JsonComponent) SerializeToJson(ISTFExportState State, ISTFResourceComponent Component);
	}
	
	public interface ISTFResourceComponentImporter
	{
		string ConvertPropertyPath(string STFProperty);
		void ParseFromJson(ISTFImportState State, JObject Json, string Id, ISTFResource Resource);
	}
}

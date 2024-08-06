
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public class ResourceIdPair { public string Id; public UnityEngine.Object Resource;}

	public abstract class ISTFResourceComponent : ScriptableObject
	{
		public abstract string Type {get;}
		public string Id = Guid.NewGuid().ToString();
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


using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serde
{
	public interface ISTFNodeComponent
	{
		string Id {get; set;}
		string Type {get;}
		List<string> Extends {get; set;}
		List<string> Overrides {get; set;}
		List<string> Targets {get; set;}
	}

	public abstract class ASTFNodeComponent : MonoBehaviour, ISTFNodeComponent
	{
		public string _id = Guid.NewGuid().ToString();
		public string Id {get => _id; set => _id = value;}
		public abstract string Type { get; }

		public List<string> _extends = new List<string>();
		public List<string> Extends {get => _extends; set => _extends = value;}

		public List<string> _overrides = new List<string>();
		public List<string> Overrides {get => _overrides; set => _overrides = value;}

		public List<string> _targets = new List<string>();
		public List<string> Targets {get => _targets; set => _targets = value;}
	}
	
	public interface ISTFNodeComponentExporter
	{
		string ConvertPropertyPath(string UnityProperty);
		(string, JObject) SerializeToJson(ISTFExportState State, Component Component);
	}
	
	public interface ISTFNodeComponentImporter
	{
		string ConvertPropertyPath(string STFProperty);
		void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go);
	}
	
	public abstract class ASTFNodeComponentExporter : ISTFNodeComponentExporter
	{
		public abstract string ConvertPropertyPath(string UnityProperty);
		abstract public (string, JObject) SerializeToJson(ISTFExportState State, Component Component);

		public static void SerializeRelationships(ISTFNodeComponent Component, JObject Json)
		{
			if(Component.Extends != null && Component.Extends.Count > 0) Json.Add("extends", new JArray(Component.Extends));
			if(Component.Overrides != null && Component.Overrides.Count > 0) Json.Add("overrides", new JArray(Component.Overrides));
			if(Component.Targets != null && Component.Targets.Count > 0) Json.Add("targets", new JArray(Component.Targets));
		}
	}
	
	public abstract class ASTFNodeComponentImporter : ISTFNodeComponentImporter
	{
		public abstract string ConvertPropertyPath(string STFProperty);
		abstract public void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go);

		public static void ParseRelationships(JObject Json, ISTFNodeComponent Component)
		{
			if(Json["extends"] != null) Component.Extends = Json["extends"].ToObject<List<string>>();
			if(Json["overrides"] != null) Component.Overrides = Json["overrides"].ToObject<List<string>>();
			if(Json["targets"] != null) Component.Targets = Json["targets"].ToObject<List<string>>();
		}
	}
}

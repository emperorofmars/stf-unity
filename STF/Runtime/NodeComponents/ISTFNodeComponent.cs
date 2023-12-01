
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF_Util;
using UnityEngine;

namespace STF.Serialisation
{
	public interface ISTFNodeComponent
	{
		string Id {get; set;}
		string Type {get;}
		string ParentNodeId {get; set;}
		List<string> Extends {get; set;}
		List<string> Overrides {get; set;}
		List<string> Targets {get; set;}
		Component OwnedUnityComponent {get; set;}
	}

	public abstract class ASTFNodeComponent : MonoBehaviour, ISTFNodeComponent
	{
		[Id] public string _Id = Guid.NewGuid().ToString();
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
		
		public Component _OwnedUnityComponent;
		public Component OwnedUnityComponent {get => _OwnedUnityComponent; set => _OwnedUnityComponent = value;}
	}
	
	public interface ISTFNodeComponentExporter
	{
		string ConvertPropertyPath(string UnityProperty);
		(string Id, JObject JsonComponent) SerializeToJson(ISTFExportState State, Component Component);
	}
	
	public interface ISTFNodeComponentImporter
	{
		string ConvertPropertyPath(string STFProperty);
		void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go);
	}
	
	public abstract class ASTFNodeComponentExporter : ISTFNodeComponentExporter
	{
		public abstract string ConvertPropertyPath(string UnityProperty);
		abstract public (string Id, JObject JsonComponent) SerializeToJson(ISTFExportState State, Component Component);

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

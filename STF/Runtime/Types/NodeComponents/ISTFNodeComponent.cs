
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using STF_Util;
using UnityEngine;

namespace STF.Types
{
	public abstract class ISTFNodeComponent : MonoBehaviour, ISTFType
	{
		public abstract string Type { get; }
		public string Id { get => _Id; set => _Id = value; }
		[Id] public string _Id = System.Guid.NewGuid().ToString();
		public string STFName { get => _STFName; set => _STFName = value; }
		public string _STFName;
		public bool Degraded => _Degraded;
		bool _Degraded = false;

		public string ParentNodeId;
		public List<string> Extends = new();
		public List<string> Overrides = new();
		public List<string> Targets = new();
		
		public Component OwnedUnityComponent;
	}
	
	public interface ISTFNodeComponentExporter
	{
		string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty);
		(string Id, JObject JsonComponent) SerializeToJson(STFExportState State, Component Component);
	}
	
	public interface ISTFNodeComponentImporter
	{
		string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty);
		void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go);
	}
	
	public abstract class ASTFNodeComponentExporter : ISTFNodeComponentExporter
	{
		public abstract string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty);
		abstract public (string Id, JObject JsonComponent) SerializeToJson(STFExportState State, Component Component);

		public static void SerializeRelationships(ISTFNodeComponent Component, JObject Json)
		{
			if(Component?.Extends != null && Component.Extends.Count > 0) Json.Add("extends", new JArray(Component.Extends));
			if(Component?.Overrides != null && Component.Overrides.Count > 0) Json.Add("overrides", new JArray(Component.Overrides));
			if(Component?.Targets != null && Component.Targets.Count > 0) Json.Add("targets", new JArray(Component.Targets));
		}
	}
	
	public abstract class ASTFNodeComponentImporter : ISTFNodeComponentImporter
	{
		public abstract string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty);
		abstract public void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go);

		public static void ParseRelationships(JObject Json, ISTFNodeComponent Component)
		{
			if(Json["extends"] != null) Component.Extends = Json["extends"].ToObject<List<string>>();
			if(Json["overrides"] != null) Component.Overrides = Json["overrides"].ToObject<List<string>>();
			if(Json["targets"] != null) Component.Targets = Json["targets"].ToObject<List<string>>();
		}
	}
}

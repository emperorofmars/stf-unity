
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serde
{
	public interface ISTFComponent
	{
		string Id {get; set;}
		List<string> Extends {get; set;}
		List<string> Overrides {get; set;}
		List<string> Targets {get; set;}
	}

	public abstract class ASTFComponent : MonoBehaviour, ISTFComponent
	{
		public string _id = Guid.NewGuid().ToString();
		public string Id {get => _id; set => _id = value;}

		public List<string> _extends = new List<string>();
		public List<string> Extends {get => _extends; set => _extends = value;}

		public List<string> _overrides = new List<string>();
		public List<string> Overrides {get => _overrides; set => _overrides = value;}

		public List<string> _targets = new List<string>();
		public List<string> Targets {get => _targets; set => _targets = value;}
	}
	
	public interface ISTFComponentExporter
	{
		List<GameObject> GatherNodes(Component Component);

		List<KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>> GatherResources(Component Component);

		JToken SerializeToJson(STFExportState State, Component Component);
	}
	
	public interface ISTFComponentImporter
	{
		void ParseFromJson(STFImportState State, JToken Json, string Id, GameObject Go);
	}
	
	public abstract class ASTFComponentExporter : ISTFComponentExporter
	{
		virtual public List<GameObject> GatherNodes(Component Component)
		{
			return null;
		}

		virtual public List<KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>> GatherResources(Component Component)
		{
			return null;
		}

		abstract public JToken SerializeToJson(STFExportState State, Component Component);

		protected void SerializeRelationships(ISTFComponent Component, JObject Json)
		{
			if(Component.Extends != null && Component.Extends.Count > 0) Json.Add("extends", new JArray(Component.Extends));
			if(Component.Overrides != null && Component.Overrides.Count > 0) Json.Add("overrides", new JArray(Component.Overrides));
			if(Component.Targets != null && Component.Targets.Count > 0) Json.Add("targets", new JArray(Component.Targets));
		}
	}
	
	public abstract class ASTFComponentImporter : ISTFComponentImporter
	{
		abstract public void ParseFromJson(STFImportState State, JToken Json, string Id, GameObject Go);

		protected void ParseRelationships(JToken Json, ISTFComponent Component)
		{
			if(Json["extends"] != null) Component.Extends = Json["extends"].ToObject<List<string>>();
			if(Json["overrides"] != null) Component.Overrides = Json["overrides"].ToObject<List<string>>();
			if(Json["targets"] != null) Component.Targets = Json["targets"].ToObject<List<string>>();
		}
	}
}


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
		List<GameObject> GatherNodes(Component component);

		List<KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>> GatherResources(Component component);

		JToken SerializeToJson(STFExportState state, Component component);
	}
	
	public interface ISTFComponentImporter
	{
		void ParseFromJson(STFImportState state, JToken json, string id, GameObject go);
	}
	
	public abstract class ASTFComponentExporter : ISTFComponentExporter
	{
		virtual public List<GameObject> GatherNodes(Component component)
		{
			return null;
		}

		virtual public List<KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>> GatherResources(Component component)
		{
			return null;
		}

		abstract public JToken SerializeToJson(STFExportState state, Component component);

		protected void SerializeRelationships(ISTFComponent component, JObject json)
		{
			if(component.Extends != null && component.Extends.Count > 0) json.Add("extends", new JArray(component.Extends));
			if(component.Overrides != null && component.Overrides.Count > 0) json.Add("overrides", new JArray(component.Overrides));
			if(component.Targets != null && component.Targets.Count > 0) json.Add("targets", new JArray(component.Targets));
		}
	}
	
	public abstract class ASTFComponentImporter : ISTFComponentImporter
	{
		abstract public void ParseFromJson(STFImportState state, JToken json, string id, GameObject go);

		protected void ParseRelationships(JToken json, ISTFComponent component)
		{
			if(json["extends"] != null) component.Extends = json["extends"].ToObject<List<string>>();
			if(json["overrides"] != null) component.Overrides = json["overrides"].ToObject<List<string>>();
			if(json["targets"] != null) component.Targets = json["targets"].ToObject<List<string>>();
		}
	}
}


using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFComponent
	{
		string id {get; set;}
		List<string> extends {get; set;}
		List<string> overrides {get; set;}
		List<string> targets {get; set;}
		List<string> resources_used {get; set;}
	}
	public abstract class ASTFComponent : MonoBehaviour, ISTFComponent
	{
		public string _id = Guid.NewGuid().ToString();
		public string id {get => _id; set => _id = value;}
		public List<string> _extends;
		public List<string> extends {get => _extends; set => _extends = value;}
		public List<string> _overrides;
		public List<string> overrides {get => _overrides; set => _overrides = value;}
		public List<string> _targets;
		public List<string> targets {get => _targets; set => _targets = value;}
		public List<string> resources_used {get => _targets; set => _targets = value;}
	}
	
	public abstract class ASTFComponentExporter
	{
		virtual public List<GameObject> GatherNodes(Component component)
		{
			return null;
		}

		virtual public List<KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>> GatherResources(Component component)
		{
			return null;
		}

		abstract public JToken SerializeToJson(ISTFExporter state, Component component);

		protected void SerializeRelationships(ISTFComponent component, JObject json)
		{
			if(component.extends != null && component.extends.Count > 0) json.Add("extends", new JArray(component.extends));
			if(component.overrides != null && component.overrides.Count > 0) json.Add("overrides", new JArray(component.overrides));
			if(component.targets != null && component.targets.Count > 0) json.Add("targets", new JArray(component.targets));
		}
	}
	
	public abstract class ASTFComponentImporter
	{
		abstract public void ParseFromJson(ISTFImporter state, ISTFAsset asset, JToken json, string id, GameObject go);

		protected void ParseRelationships(JToken json, ISTFComponent component)
		{
			if(json["extends"] != null) component.extends = json["extends"].ToObject<List<string>>();
			if(json["overrides"] != null) component.overrides = json["overrides"].ToObject<List<string>>();
			if(json["targets"] != null) component.targets = json["targets"].ToObject<List<string>>();
			if(json["resources_used"] != null) component.resources_used = json["resources_used"].ToObject<List<string>>();
		}
	}
}

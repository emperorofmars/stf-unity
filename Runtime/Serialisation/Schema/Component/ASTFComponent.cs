
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{
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
		}
	}
}

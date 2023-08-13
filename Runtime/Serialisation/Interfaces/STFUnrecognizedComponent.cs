
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;

namespace stf.Components
{
	// If a component is encountered which has no registered importer, this will be used to hold its data and the resource ID's it references.
	// TODO: Actually list all referenced resources in every component. Also create a object to hold an unrecognized resource, and its buffers.

	public class STFUnrecognizedComponent : MonoBehaviour, ISTFComponent
	{
		public string id {get; set;}
		public List<string> extends {get; set;}
		public List<string> overrides {get; set;}
		public List<string> targets {get; set;}
		public List<Object> resources_used = new List<Object>();
		public string type;

		[Multiline]
		public string json;

		public JToken SerializeToJson(ISTFExporter state)
		{
			var ret = (JObject)JToken.Parse(json);
			var resources_used_Ids = new JArray();

			if(ret.ContainsKey("resources_used")) ret.Remove("resources_used");
			((JObject)ret).Add("resources_used", resources_used_Ids);
			foreach(var resource in resources_used)
			{
				var id = state.GetResourceId(resource);
				if(id == null) foreach(var meta in state.GetMetas())
				{
					id = meta.resourceInfo.Find(r => r.resource == resource || r.originalResource == resource)?.id;
					if(id != null) break;
				}
				if(id != null) resources_used_Ids.Add(id);
				else Debug.LogWarning($"Resource from unrecognized component missing!: {resource}");
			}
			return ret;
		}

		public void ParseFromJson(ISTFImporter state, JToken json)
		{
			this.type = (string)json["type"];
			this.json = ((JObject)json).ToString();
			var resources_used_Ids = json["resources_used"]?.ToObject<List<string>>();
			if(resources_used_Ids != null) foreach(var id in resources_used_Ids)
			{
				resources_used.Add(state.GetResource(id));
			}
		}
	}
}

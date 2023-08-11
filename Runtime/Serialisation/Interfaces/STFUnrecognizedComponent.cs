
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
		public List<string> resources_used {get; set;}
		public string type;

		[Multiline]
		public string json;

		public JToken SerializeToJson(ISTFExporter state)
		{
			return new JRaw(json);
		}

		public void ParseFromJson(ISTFImporter state, JToken json)
		{
			this.type = (string)json["type"];
			this.json = ((JObject)json).ToString();
		}
	}
}

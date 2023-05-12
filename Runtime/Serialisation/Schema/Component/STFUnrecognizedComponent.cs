
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;

namespace stf.Components
{
	public class STFUnrecognizedComponent : MonoBehaviour, ISTFComponent
	{
		public string id {get; set;}
		public List<string> extends {get; set;}
		public List<string> overrides {get; set;}
		public List<string> targets {get; set;}
		public string type;

		[Multiline]
		public string json;

		public JToken serializeToJson(ISTFExporter state)
		{
			return new JRaw(json);
		}

		public void parseFromJson(ISTFImporter state, JToken json)
		{
			this.name = id + (string)json["name"];
			this.type = (string)json["type"];
			this.json = ((JObject)json).ToString();
		}
	}
}


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
		public List<string> resources_used {get; set;}

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

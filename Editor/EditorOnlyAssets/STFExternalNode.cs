
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace stf.serialisation
{
	public class STFExternalNodeExporter
	{
		public static JObject serializeToJson(GameObject go, ISTFExporter state)
		{
			var ret = new JObject();
			ret.Add("name", go.name);
			ret.Add("type", "external");
			ret.Add("path", Utils.getPath(go.transform));
			return ret;
		}
	}

	public class STFExternalNodeImporter : ISTFNodeImporter
	{
		GameObject root;
		public STFExternalNodeImporter(GameObject root)
		{
			this.root = root;
		}
		public GameObject parseFromJson(ISTFImporter state, JToken json, JObject jsonRoot, out List<string> nodesToParse)
		{
			var go = root.transform.Find((string)json["path"]).gameObject;
			go.name = (string)json["name"];
			nodesToParse = null;
			return go;
		}
	}
}

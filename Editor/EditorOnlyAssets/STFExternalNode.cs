
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace stf.serialisation
{
	public class STFExternalNodeExporter : ASTFNodeExporter
	{
		override public JToken serializeToJson(GameObject go, ISTFExporter state)
		{
			var ret = (JObject)base.serializeToJson(go, state);
			ret.Add("type", "external");
			ret.Add("path", Utils.getPath(go.transform));
			return ret;
		}
	}

	public class STFExternalNodeImporter : ASTFNodeImporter
	{
		GameObject root;
		public STFExternalNodeImporter(GameObject root)
		{
			this.root = root;
		}
		override public GameObject parseFromJson(ISTFImporter state, JToken json)
		{
			var go = root.transform.Find((string)json["path"]).gameObject;
			go.name = (string)json["name"];
			return go;
		}
	}
}

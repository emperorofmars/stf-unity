
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace stf.serialisation
{
	/*public abstract class ASTFNodeExporter
	{
		virtual public JToken serializeToJson(GameObject go, ISTFExporter state)
		{
			var ret = new JObject() {
				{"name", go.name}
			};
			return ret;
		}

		public List<GameObject> gatherNodes(GameObject go)
		{
			return null;
		}

		public List<UnityEngine.Object> gatherResources(GameObject go)
		{
			return null;
		}
	}*/

	public abstract class ASTFNodeImporter
	{
		virtual public GameObject parseFromJson(ISTFImporter state, JToken json)
		{
			var go = new GameObject();
			go.name = (string)json["name"];
			return go;
		}
	}
}

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public abstract class ASTFResourceExporter
	{
		virtual public List<GameObject> GatherNodes(UnityEngine.Object resource)
		{
			return null;
		}

		virtual public List<KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>> GatherResources(UnityEngine.Object resource)
		{
			return null;
		}

		abstract public JToken SerializeToJson(ISTFExporter state, UnityEngine.Object resource);
	}

	public abstract class ASTFResourceImporter
	{
		abstract public UnityEngine.Object ParseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot);
	}
}

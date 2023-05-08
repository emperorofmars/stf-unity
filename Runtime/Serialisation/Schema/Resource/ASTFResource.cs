
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public abstract class ASTFResourceExporter
	{
		virtual public List<GameObject> gatherNodes(UnityEngine.Object resource)
		{
			return null;
		}

		virtual public List<UnityEngine.Object> gatherResources(UnityEngine.Object resource)
		{
			return null;
		}

		abstract public JToken serializeToJson(ISTFExporter state, UnityEngine.Object resource);
	}

	public abstract class ASTFResourceImporter
	{
		abstract public UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot);
	}
}

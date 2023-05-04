
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public abstract class ASTFComponentExporter
	{
		virtual public List<GameObject> gatherNodes(Component component)
		{
			return null;
		}

		virtual public List<UnityEngine.Object> gatherResources(Component component)
		{
			return null;
		}

		abstract public JToken serializeToJson(ISTFExporter state, Component component);
	}
	
	public abstract class ASTFComponentImporter
	{
		abstract public void parseFromJson(ISTFImporter state, JToken json, string id, GameObject go);
	}
}

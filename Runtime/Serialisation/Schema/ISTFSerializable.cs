using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFSerializable
	{
		List<GameObject> gatherNodes();
		List<Object> gatherResources();
		JToken serializeToJson(ISTFExporter state);
		void parseFromJson(ISTFImporter state, JToken json);
	}
}


using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace stf.serialisation
{
	public interface ISTFNodeImporter
	{
		GameObject parseFromJson(ISTFImporter state, JToken json, JObject jsonRoot, out List<string> nodesToParse);
	}
}
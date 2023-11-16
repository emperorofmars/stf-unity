
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace stf.serialisation
{
	public interface ISTFNodeImporter
	{
		GameObject ParseFromJson(ISTFImporter state, JToken json, JObject jsonRoot, ISTFAsset asset, ISTFAssetImporter assetContext, out List<string> nodesToParse);
	}
}
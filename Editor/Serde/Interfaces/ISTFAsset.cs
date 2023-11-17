
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.IdComponents;

namespace STF.Serde
{
	public interface ISTFAssetExporter
	{
		JToken SerializeToJson(STFExportState state);
	}
	
	public interface ISTFAssetImporter
	{
		STFAsset ParseFromJson(STFImportState state, JToken jsonAsset, string id, JObject jsonRoot);
		void convertNode(STFImportState state, string nodeId, JObject jsonRoot, STFAsset asset);
	}
}

#endif

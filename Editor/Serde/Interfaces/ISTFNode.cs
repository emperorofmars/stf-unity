
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.IdComponents;

namespace STF.Serde
{
	public interface ISTFNodeExporter
	{
		JToken SerializeToJson(STFExportState state, GameObject go);
	}

	public interface ISTFNodeImporter
	{
		GameObject ParseFromJson(STFImportState state, JToken jsonAsset, string id, JObject jsonRoot);
	}
}

#endif

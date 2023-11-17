
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
		JObject SerializeToJson(STFExportState State, System.Object Asset);
	}
	
	public interface ISTFAssetImporter
	{
		UnityEngine.Object ParseFromJson(STFImportState State, JObject JsonAsset, string Id, JObject JsonRoot);
	}
}

#endif

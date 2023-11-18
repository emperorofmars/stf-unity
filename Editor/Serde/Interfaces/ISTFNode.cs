
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
		//JObject SerializeToJson(ISTFAssetExportState State, GameObject Go);
	}

	public interface ISTFNodeImporter
	{
		GameObject ParseFromJson(ISTFAssetImportState State, JObject JsonAsset, string Id);
	}
}

#endif

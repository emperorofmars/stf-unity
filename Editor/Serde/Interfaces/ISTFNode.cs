
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
		JToken SerializeToJson(STFExportState State, GameObject Go);
	}

	public interface ISTFNodeImporter
	{
		GameObject ParseFromJson(STFImportState State, JToken JsonAsset, string Id);
	}
}

#endif

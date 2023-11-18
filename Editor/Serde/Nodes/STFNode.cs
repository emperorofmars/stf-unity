
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serde
{
	public class STFNode : MonoBehaviour
	{
		public string NodeId = Guid.NewGuid().ToString();
		public string Type = "STF.Node";
		public string Origin;
	}

	public class STFNodeExporter : ISTFNodeExporter
	{
		public JObject SerializeToJson(STFExportState State, GameObject Go)
		{
			throw new NotImplementedException();
		}
	}

	public class STFNodeImporter : ISTFNodeImporter
	{
		public static string _TYPE = "STF.Node";
		public GameObject ParseFromJson(ISTFAssetImportState State, JObject JsonAsset, string Id)
		{
			var ret = new GameObject();
			var node = ret.AddComponent<STFNode>();
			node.NodeId = Id;
			node.name = (String)JsonAsset["name"];
			node.Origin = State.AssetInfo.assetId;
			//Components
			return ret;
		}
	}
}

#endif

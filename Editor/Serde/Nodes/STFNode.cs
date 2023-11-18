
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Util;
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
			State.AddNode(ret, Id);
			
			var node = ret.AddComponent<STFNode>();
			node.NodeId = Id;
			node.name = (String)JsonAsset["name"];
			node.Origin = State.AssetInfo.assetId;

			TRSUtil.ParseTRS(ret, JsonAsset);

			foreach(string childId in JsonAsset["children"])
			{
				var childJson = (JObject)State.JsonRoot["nodes"][childId];
				var type = (string)childJson["type"];
				if(type == null || type.Length == 0) type = STFNodeImporter._TYPE;
				if(State.Context.NodeImporters.ContainsKey(type))
				{
					Debug.Log($"Parsing Node: {type}");
					var childGo = State.Context.NodeImporters[type].ParseFromJson(State, childJson, childId);
					childGo.transform.SetParent(ret.transform);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Node: {type}");
					// Unrecognized Node
				}
			}

			//Components
			return ret;
		}
	}
}

#endif

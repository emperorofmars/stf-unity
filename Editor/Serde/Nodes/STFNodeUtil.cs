
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serde
{
	public class STFNodeUtil
	{
		public static void ParseChildren(ISTFAssetImportState State, GameObject Go, JObject JsonAsset)
		{
			foreach(string childId in JsonAsset["children"])
			{
				var childJson = (JObject)State.JsonRoot["nodes"][childId];
				var type = (string)childJson["type"];
				if(type == null || type.Length == 0) type = STFNode._TYPE;
				if(State.Context.NodeImporters.ContainsKey(type))
				{
					Debug.Log($"Parsing Node: {type}");
					var childGo = State.Context.NodeImporters[type].ParseFromJson(State, childJson, childId);
					childGo.transform.SetParent(Go.transform);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Node: {type}");
					// Unrecognized Node
				}
			}
		}

		public static void ParseComponents(ISTFAssetImportState State, GameObject Go, JObject JsonAsset)
		{
			if(JsonAsset["components"] == null) return;
			foreach(var entry in (JObject)JsonAsset["components"])
			{
				var type = (string)entry.Value["type"];
				if(type == null || type.Length == 0) type = STFNode._TYPE;
				if(State.Context.NodeComponentImporters.ContainsKey(type))
				{
					Debug.Log($"Parsing Component: {type}");
					State.Context.NodeComponentImporters[type].ParseFromJson(State, (JObject)entry.Value, entry.Key, Go);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Component: {type}");
					//
				}
			}
		}

		public static void Parse(ISTFAssetImportState State, GameObject Go, JObject JsonAsset)
		{
			ParseChildren(State, Go, JsonAsset);
			ParseComponents(State, Go, JsonAsset);
		}
	}
}

#endif


#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serde
{
	public class STFSerdeUtil
	{
		public static void ParseNodeChildren(ISTFAssetImportState State, GameObject Go, JObject JsonAsset)
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

		public static void ParseNodeComponents(ISTFAssetImportState State, GameObject Go, JObject JsonAsset)
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

		public static void ParseNode(ISTFAssetImportState State, GameObject Go, JObject JsonAsset)
		{
			ParseNodeChildren(State, Go, JsonAsset);
			ParseNodeComponents(State, Go, JsonAsset);
		}

		public static string SerializeNode(ISTFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<ISTFNode>();
			if(node != null && State.Context.NodeExporters.ContainsKey(node.Type))
			{
				Debug.Log($"Serializing Node: {node.Type}");
				return State.Context.NodeExporters[node.Type].SerializeToJson(State, Go);
			}
			else
			{
				Debug.LogWarning($"Unrecognized Node: {node.Type}");
				// Unrecognized Asset
				return null;
			}
		}

		public static KeyValuePair<string, JObject> SerializeComponent(ISTFExportState State, Component NodeComponent)
		{
			if(State.Context.NodeComponentExporters.ContainsKey(NodeComponent.GetType()))
			{
				Debug.Log($"Serializing NodeComponent: {NodeComponent.GetType()}");
				return State.Context.NodeComponentExporters[NodeComponent.GetType()].SerializeToJson(State, NodeComponent);
			}
			else
			{
				Debug.LogWarning($"Unrecognized NodeComponent: {NodeComponent.GetType()}");
				// Unrecognized Asset
				return new KeyValuePair<string, JObject>(null, null);
			}
		}

		public static string SerializeResource(ISTFExportState State, UnityEngine.Object Resource)
		{
			if(State.Context.ResourceExporters.ContainsKey(Resource.GetType()))
			{
				Debug.Log($"Serializing Resource: {Resource.GetType()}");
				return State.Context.ResourceExporters[Resource.GetType()].SerializeToJson(State, Resource);
			}
			else
			{
				Debug.LogWarning($"Unrecognized Resource: {Resource.GetType()}");
				// Unrecognized Asset
				return null;
			}
		}
	}
}

#endif

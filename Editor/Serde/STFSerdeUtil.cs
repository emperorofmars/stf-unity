
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using Codice.Client.Common.TreeGrouper;
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
					var childGo = State.Context.NodeImporters[type].ParseFromJson(State, childJson, childId);
					childGo.transform.SetParent(Go.transform);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Node: {type}");
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
					State.Context.NodeComponentImporters[type].ParseFromJson(State, (JObject)entry.Value, entry.Key, Go);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Component: {type}");
					STFUnrecognizedNodeComponentImporter.ParseFromJson(State, (JObject)entry.Value, entry.Key, Go);
				}
			}
		}

		public static void ParseNode(ISTFAssetImportState State, GameObject Go, JObject JsonAsset)
		{
			ParseNodeChildren(State, Go, JsonAsset);
			ParseNodeComponents(State, Go, JsonAsset);
		}

		public static JArray SerializeChildren(ISTFExportState State, GameObject Go)
		{
			var ret = new JArray();
			for(int childIdx = 0; childIdx < Go.transform.childCount; childIdx++)
			{
				var child = SerializeNode(State, Go.transform.GetChild(childIdx).gameObject);
				if(child != null) ret.Add(child);
				else Debug.LogWarning($"Skipping Unrecognized Unity Node: {Go.transform.GetChild(childIdx)}");
			}
			return ret;
		}

		public static string SerializeNode(ISTFExportState State, GameObject Go)
		{
			if(State.Nodes.ContainsKey(Go)) return State.Nodes[Go].Key;
			var node = Go.GetComponent<ISTFNode>();
			if(node != null && State.Context.NodeExporters.ContainsKey(node.Type))
			{
				return State.Context.NodeExporters[node.Type].SerializeToJson(State, Go);
			}
			else
			{
				Debug.LogWarning($"Unrecognized Node: {node.Type}");
				return null;
			}
		}

		public static JObject SerializeComponents(ISTFExportState State, Component[] NodeComponents)
		{
			var ret = new JObject();
			foreach(var component in NodeComponents)
			{
				if(State.Context.ExportExclusions.Find(e => component.GetType().IsSubclassOf(e) || component.GetType() == e) != null) continue;
				var serializedComponent = SerializeComponent(State, component);
				if(serializedComponent.Item1 != null) ret.Add(serializedComponent.Item1, serializedComponent.Item2);
				else Debug.LogWarning($"Skipping Unrecognized Unity Component: {component}");
			}
			return ret;
		}

		public static (string, JObject) SerializeComponent(ISTFExportState State, Component NodeComponent)
		{
			if(State.Context.NodeComponentExporters.ContainsKey(NodeComponent.GetType()))
			{
				return State.Context.NodeComponentExporters[NodeComponent.GetType()].SerializeToJson(State, NodeComponent);
			}
			else
			{
				return STFUnrecognizedNodeComponentExporter.SerializeToJson(State, NodeComponent);
			}
		}

		public static string SerializeResource(ISTFExportState State, UnityEngine.Object Resource)
		{
			if(State.Resources.ContainsKey(Resource)) return State.Resources[Resource].Key;
			if(State.Context.ResourceExporters.ContainsKey(Resource.GetType()))
			{
				return State.Context.ResourceExporters[Resource.GetType()].SerializeToJson(State, Resource);
			}
			else
			{
				Debug.LogWarning($"Unrecognized Resource: {Resource.GetType()}");
				return null;
			}
		}
	}
}

#endif

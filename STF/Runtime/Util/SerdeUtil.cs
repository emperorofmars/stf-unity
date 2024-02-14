
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public class SerdeUtil
	{
		public static void ParseNodeChildren(ISTFAssetImportState State, GameObject Go, JObject JsonAsset)
		{
			if(JsonAsset["children"] == null || JsonAsset["children"].Type == JTokenType.Null) return;
			foreach(string childId in JsonAsset["children"])
			{
				var childJson = (JObject)State.JsonRoot["nodes"][childId];
				var type = (string)childJson["type"];
				if(type == null || type.Length == 0) type = STFNode._TYPE;
				if(State.Context.NodeImporters.ContainsKey(type))
				{
					var childGo = State.Context.NodeImporters[type].ParseFromJson(State, childJson, childId);
					childGo.transform.SetParent(Go.transform, false);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Node: {type}");
					var childGo = STFUnrecognizedNodeImporter.ParseFromJson(State, childJson, childId);
					childGo.transform.SetParent(Go.transform, false);
				}
			}
		}

		public static void ParseNodeComponents(ISTFAssetImportState State, GameObject Go, JObject JsonAsset)
		{
			if(JsonAsset["components"] == null || JsonAsset["components"].Type == JTokenType.Null) return;
			foreach(var entry in (JObject)JsonAsset["components"])
			{
				var type = (string)entry.Value["type"];
				if(type != null && State.Context.NodeComponentImporters.ContainsKey(type))
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

		public static void ParseResourceComponents(ISTFImportState State, ISTFResource Resource, JObject JsonResource)
		{
			if(JsonResource["components"] == null || JsonResource["components"].Type == JTokenType.Null) return;
			foreach(var entry in (JObject)JsonResource["components"])
			{
				var type = (string)entry.Value["type"];
				if(type != null && State.Context.ResourceComponentImporters.ContainsKey(type))
				{
					State.Context.ResourceComponentImporters[type].ParseFromJson(State, (JObject)entry.Value, entry.Key, Resource);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Component: {type}");
					STFUnrecognizedResourceComponentImporter.ParseFromJson(State, (JObject)entry.Value, entry.Key, Resource);
				}
			}
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
			if(State.Nodes.ContainsKey(Go)) return State.Nodes[Go].Id;
			var node = Go.GetComponent<ISTFNode>();
			if(node != null && State.Context.NodeExporters.ContainsKey(node.Type))
			{
				return State.Context.NodeExporters[node.Type].SerializeToJson(State, Go);
			}
			else
			{
				Debug.LogWarning($"Unrecognized Node: {node.Type}");
				return STFUnrecognizedNodeExporter.SerializeToJson(State, Go);
			}
		}

		public static JObject SerializeNodeComponents(ISTFExportState State, Component[] NodeComponents)
		{
			var ret = new JObject();
			foreach(var component in NodeComponents)
			{
				if(State.Context.ExportExclusions.Find(e => component.GetType().IsSubclassOf(e) || component.GetType() == e) != null) continue;
				var serializedComponent = SerializeNodeComponent(State, component);
				if(serializedComponent.Item1 != null) ret.Add(serializedComponent.Item1, serializedComponent.Item2);
				else Debug.LogWarning($"Skipping Unrecognized Unity Component: {component}");
			}
			return ret;
		}

		public static (string, JObject) SerializeNodeComponent(ISTFExportState State, Component NodeComponent)
		{
			(string Id, JObject JsonComponent) ret;
			if(State.Context.NodeComponentExporters.ContainsKey(NodeComponent.GetType()))
			{
				ret = State.Context.NodeComponentExporters[NodeComponent.GetType()].SerializeToJson(State, NodeComponent);
			}
			else
			{
				Debug.LogWarning($"Unrecognized Node: {NodeComponent.GetType()}");
				ret = STFUnrecognizedNodeComponentExporter.SerializeToJson(State, NodeComponent);
			}
			State.AddComponent(NodeComponent, ret.JsonComponent, ret.Id);
			return ret;
		}

		public static string SerializeResource(ISTFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			if(State.Resources.ContainsKey(Resource)) return State.Resources[Resource].Id;
			else if(State.Context.ResourceExporters.ContainsKey(Resource.GetType()))
			{
				return State.Context.ResourceExporters[Resource.GetType()].SerializeToJson(State, Resource, Context);
			}
			else
			{
				Debug.LogWarning($"Unrecognized Resource: {Resource.GetType()}");
				return null;
			}
		}

		public static JObject SerializeResourceComponents(ISTFExportState State, ISTFResource Resource)
		{
			var ret = new JObject();
			if(Resource?.Components != null) foreach(var component in Resource.Components)
			{
				var serializedComponent = SerializeResourceComponent(State, component);
				if(serializedComponent.Item1 != null) ret.Add(serializedComponent.Item1, serializedComponent.Item2);
				else Debug.LogWarning($"Skipping Unrecognized Resource Component: {component}");
			}
			return ret;
		}

		public static (string, JObject) SerializeResourceComponent(ISTFExportState State, ISTFResourceComponent ResourceComponent)
		{
			if(State.Context.ResourceComponentExporters.ContainsKey(ResourceComponent.Type))
			{
				return State.Context.ResourceComponentExporters[ResourceComponent.Type].SerializeToJson(State, ResourceComponent);
			}
			else
			{
				Debug.LogWarning($"Unrecognized Resource Component: {ResourceComponent.Type}");
				return STFUnrecognizedResourceComponentExporter.SerializeToJson(State, ResourceComponent);
			}
		}
	}
}

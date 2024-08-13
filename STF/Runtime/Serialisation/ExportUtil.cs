
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public static class ExportUtil
	{
		public static JArray SerializeNodeChildren(STFExportState State, GameObject Go)
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

		public static string SerializeNode(STFExportState State, ISTFNode Node)
		{
			return SerializeNode(State, Node.gameObject);
		}

		public static string SerializeNode(STFExportState State, GameObject Go)
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

		public static JObject SerializeNodeComponents(STFExportState State, Component[] NodeComponents)
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

		public static (string, JObject) SerializeNodeComponent(STFExportState State, Component NodeComponent)
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
			State.AddNodeComponent(NodeComponent, ret.JsonComponent, ret.Id);
			return ret;
		}

		public static string SerializeResource(STFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
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

		public static JObject SerializeResourceComponents(STFExportState State, ISTFResource Resource)
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

		public static (string, JObject) SerializeResourceComponent(STFExportState State, ISTFResourceComponent ResourceComponent)
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


using Newtonsoft.Json.Linq;
using STF.Types;
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
			return State.Context.GetNodeExporter(node.Type).SerializeToJson(State, Go);
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
			(string Id, JObject JsonComponent) ret = State.Context.GetNodeComponentExporter(NodeComponent.GetType()).SerializeToJson(State, NodeComponent);
			State.AddNodeComponent(NodeComponent, ret.JsonComponent, ret.Id);
			return ret;
		}

		public static string SerializeResource(STFExportState State, Object Resource, Object Context = null)
		{
			if(State.Resources.ContainsKey(Resource)) return State.Resources[Resource].Id;
			else return State.Context.GetResourceExporter(Resource.GetType()).SerializeToJson(State, Resource, Context);
		}

		public static JObject SerializeResourceComponents(STFExportState State, ISTFResource Resource)
		{
			var ret = new JObject();
			if(Resource && Resource.Components != null) foreach(var component in Resource.Components)
			{
				var serializedComponent = SerializeResourceComponent(State, component);
				ret.Add(serializedComponent.Item1, serializedComponent.Item2);
			}
			return ret;
		}

		public static (string, JObject) SerializeResourceComponent(STFExportState State, ISTFResourceComponent ResourceComponent)
		{
			return State.Context.GetResourceComponentExporter(ResourceComponent.Type).SerializeToJson(State, ResourceComponent);
		}
	}
}

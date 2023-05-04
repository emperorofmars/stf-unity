
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace stf.serialisation
{
	public class STFExternalUnityAssetExporter : ISTFAssetExporter
	{
		public GameObject rootNode;
		private STFNodeExporter _nodeExporter = new STFNodeExporter();
		private STFExternalUnityResourceExporter _resourceExporter = new STFExternalUnityResourceExporter();

		public void Convert(ISTFExporter state)
		{
			if(rootNode == null) throw new Exception("Root node must not be null");

			var transforms = rootNode.GetComponentsInChildren<Transform>();
			foreach(var transform in transforms)
			{
				//check if node is already registered
				var nodeId = state.RegisterNode(transform.gameObject, _nodeExporter);
				var components = transform.GetComponents<Component>();
				foreach(var component in components)
				{
					if(state.GetContext().ComponentExporters.ContainsKey(component.GetType()))
					{
						var componentExporter = state.GetContext().ComponentExporters[component.GetType()];
						gatherResources(state, componentExporter.gatherResources(component));
						state.RegisterComponent(nodeId, component, componentExporter);
					}
				}
			}
		}

		private void gatherResources(ISTFExporter state, List<UnityEngine.Object> resources)
		{
			if(resources != null)
			{
				foreach(var resource in resources)
				{
					gatherResources(state, _resourceExporter.gatherResources(resource));
					state.RegisterResource(resource, _resourceExporter);
				}
			}
		}

		public JToken SerializeToJson(ISTFExporter state)
		{
			var ret = new JObject();
			ret.Add("type", "external");
			ret.Add("root_node", state.GetNodeId(rootNode));
			return ret;
		}
	}

	public class ExternalUnityAsset : ISTFAsset
	{
		ISTFImporter state;
		public string id;
		public string rootNodeId;

		public ExternalUnityAsset(ISTFImporter state, string id)
		{
			this.state = state;
			this.id = id;
		}

		public UnityEngine.Object GetAsset()
		{
			return state.GetNode(rootNodeId);
		}

		public Type GetAssetType()
		{
			return typeof(GameObject);
		}

		public string getId()
		{
			return id;
		}
	}

	public class ExternalUnityAssetImporter : ISTFAssetImporter
	{
		private STFNodeImporter _importer = new STFNodeImporter();

		public ISTFAsset ParseFromJson(ISTFImporter state, JToken jsonAsset, string id, JObject jsonRoot)
		{
			var ret = new ExternalUnityAsset(state, id);
			var rootNodeId = (string)jsonAsset["root_node"];
			ret.rootNodeId = rootNodeId;
			convertAssetNode(state, rootNodeId, jsonRoot);
			return ret;
		}

		private void convertAssetNode(ISTFImporter state, string nodeId, JObject jsonRoot)
		{
			var jsonNode = (JObject)jsonRoot["nodes"][nodeId];
			if((string)jsonNode["type"] != null && ((string)jsonNode["type"]).Length > 0 && (string)jsonNode["type"] != "default")
				throw new Exception("Nodetype '" + (string)jsonNode["type"] + "' is not supported");

			state.AddNode(nodeId, _importer.parseFromJson(state, jsonNode));
			
			if(jsonNode["children"] != null)
			{
				foreach(var child in (JArray)jsonNode["children"])
				{
					convertAssetNode(state, (string)child, jsonRoot);
				}
			}
		}
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public class Register_ExternalUnityAssetImporter
	{
		static Register_ExternalUnityAssetImporter()
		{
			STFRegistry.RegisterAssetImporter("external", new ExternalUnityAssetImporter());
		}
	}
#endif
}

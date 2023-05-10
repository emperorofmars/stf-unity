
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using stf.Components;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace stf.serialisation
{
	public class STFExternalUnityPatchAssetExporter : ISTFAssetExporter
	{
		public GameObject rootNode;
		private STFExternalNodeExporter _nodeExporter = new STFExternalNodeExporter();
		private STFExternalUnityResourceExporter _resourceExporter = new STFExternalUnityResourceExporter();

		private List<string> gatheredRootNodes = new List<string>();

		public void Convert(ISTFExporter state)
		{
			if(rootNode == null) throw new Exception("Root node must not be null");

			var components = rootNode.GetComponentsInChildren<Component>();

			foreach(var component in components)
			{
				if(component.GetType() == typeof(Transform)) continue;
				if(component.GetType() == typeof(STFUUID)) continue;
				if(component.GetType() == typeof(Animator)) continue;
				if(component.GetType() == typeof(SkinnedMeshRenderer)) continue;
				if(component.GetType() == typeof(MeshRenderer)) continue;
				if(component.GetType() == typeof(MeshFilter)) continue;

				if(component.GetType() == typeof(STFUnrecognizedComponent))
				{
					var nodeUuidComponent = component.gameObject.GetComponent<STFUUID>();
					var nodeId = nodeUuidComponent != null && nodeUuidComponent.id != null && nodeUuidComponent.id.Length > 0 ? nodeUuidComponent.id : Guid.NewGuid().ToString();
					state.RegisterNode(nodeId, STFExternalNodeExporter.serializeToJson(component.gameObject, state), component.gameObject);
					state.RegisterComponent(nodeId, component);
				}
				else if(state.GetContext().ComponentExporters.ContainsKey(component.GetType()))
				{
					var componentExporter = state.GetContext().ComponentExporters[component.GetType()];
					var nodeUuidComponent = component.gameObject.GetComponent<STFUUID>();
					var nodeId = nodeUuidComponent != null && nodeUuidComponent.id != null && nodeUuidComponent.id.Length > 0 ? nodeUuidComponent.id : Guid.NewGuid().ToString();
					state.RegisterNode(nodeId, STFExternalNodeExporter.serializeToJson(component.gameObject, state), component.gameObject);
					var gatheredNodes = componentExporter.gatherNodes(component);
					if(gatheredNodes != null)
					{
						foreach(var gatheredNode in gatheredNodes)
						{
							var gatheredNodeUuidComponent = component.gameObject.GetComponent<STFUUID>();
							var gatheredNodeId = nodeUuidComponent != null && gatheredNodeUuidComponent.id != null && gatheredNodeUuidComponent.id.Length > 0 ? gatheredNodeUuidComponent.id : Guid.NewGuid().ToString();
							state.RegisterNode(gatheredNodeId, STFExternalNodeExporter.serializeToJson(component.gameObject, state), component.gameObject);

							if(!gatheredRootNodes.Contains(gatheredNodeId)) gatheredRootNodes.Add(gatheredNodeId);
						}
					}
					gatherResources(state, componentExporter.gatherResources(component));
					state.RegisterComponent(nodeId, component);
					gatheredRootNodes.Add(nodeId);
				}
				else
				{
					Debug.LogWarning("Component not recognized, skipping: " + component.GetType() + " on " + component.gameObject.name);
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
			ret.Add("type", "external_unity_patch");
			ret.Add("unity_asset_path", PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(rootNode));
			ret.Add("unity_root_node_path", Utils.getPath(rootNode.transform));
			ret.Add("root_nodes", new JArray(gatheredRootNodes));
			return ret;
		}

		public string GetId(ISTFExporter state)
		{
			return Guid.NewGuid().ToString();
		}
	}

	public class ExternalUnityPatchAsset : ISTFAsset
	{
		ISTFImporter state;
		public string id;
		public GameObject unityRoot;

		public ExternalUnityPatchAsset(ISTFImporter state, string id)
		{
			this.state = state;
			this.id = id;
		}

		public UnityEngine.Object GetAsset()
		{
			return unityRoot;
		}

		public string GetSTFAssetType()
		{
			return "unity_asset_path";
		}

		public Type GetUnityAssetType()
		{
			return typeof(GameObject);
		}

		public string getId()
		{
			return id;
		}
	}
	
	public class STFExternalUnityPatchAssetImporter : ISTFAssetImporter
	{
		private STFExternalNodeImporter _importer;

		public ISTFAsset ParseFromJson(ISTFImporter state, JToken jsonAsset, string id, JObject jsonRoot)
		{
			var ret = new ExternalUnityPatchAsset(state, id);
			var rootNodeIds = jsonAsset["root_nodes"].ToObject<List<string>>();
			var unityRootAssetPath = (string)jsonAsset["unity_asset_path"];
			var mainAsset = AssetDatabase.LoadMainAssetAtPath(unityRootAssetPath);
			if(mainAsset.GetType() != typeof(GameObject))
				throw new Exception("Referenced asset is not a GameObject");
			
			ret.unityRoot = UnityEngine.Object.Instantiate((GameObject)mainAsset);
			ret.unityRoot.name = ((GameObject)mainAsset).name;
			_importer = new STFExternalNodeImporter(ret.unityRoot);

			foreach(var rootNodeId in rootNodeIds) convertAssetNode(state, rootNodeId, jsonRoot);

			return ret;
		}

		private void convertAssetNode(ISTFImporter state, string nodeId, JObject jsonRoot)
		{
			var jsonNode = (JObject)jsonRoot["nodes"][nodeId];
			if((string)jsonNode["type"] != null && ((string)jsonNode["type"]).Length > 0 && (string)jsonNode["type"] != "external")
				throw new Exception("Nodetype '" + (string)jsonNode["type"] + "' is not supported");

			var nodesToParse = new List<string>();
			state.AddNode(nodeId, _importer.parseFromJson(state, jsonNode, jsonRoot, out nodesToParse));
			
			if(nodesToParse != null)
			{
				foreach(var childId in nodesToParse)
				{
					convertAssetNode(state, childId, jsonRoot);
				}
			}
		}
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public class Register_STFExternalUnityPatchAssetImporter
	{
		static Register_STFExternalUnityPatchAssetImporter()
		{
			STFRegistry.RegisterAssetImporter("external_unity_patch", new STFExternalUnityPatchAssetImporter());
		}
	}
#endif
}

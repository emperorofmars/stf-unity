
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	public class STFAddonAssetExporter : ISTFAssetExporter
	{
		public GameObject rootNode;
		public string id;
		public string name;

		public void Convert(ISTFExporter state)
		{
			if(rootNode == null) throw new Exception("Root node must not be null");

			var assetInfo = rootNode.GetComponent<STFAssetInfo>();
			if(assetInfo != null)
			{
				id = assetInfo.assetId != null && assetInfo.assetId.Length > 0 ? assetInfo.assetId : Guid.NewGuid().ToString();
				name = assetInfo.assetName != null && assetInfo.assetName.Length > 0 ? assetInfo.assetName : rootNode.name;
				if(assetInfo.originalMetaInformation != null) state.AddMeta(assetInfo.originalMetaInformation);
			} else
			{
				id = Guid.NewGuid().ToString();
				name = rootNode.name;
			}

			var armatureState = new STFAssetArmatureHandler();
			armatureState.GatherArmatures(state, rootNode.GetComponentsInChildren<SkinnedMeshRenderer>(), rootNode);

			var transforms = rootNode.GetComponentsInChildren<Transform>();
			foreach(var transform in transforms)
			{
				if(transform == rootNode.transform) continue;
				var go = transform.gameObject;
				var nodeUuidComponent = go.GetComponent<STFUUID>();
				var nodeId = nodeUuidComponent != null && nodeUuidComponent.id != null && nodeUuidComponent.id.Length > 0 ? nodeUuidComponent.id : Guid.NewGuid().ToString();
				if(transform.parent == rootNode.transform)
				{
					var appendageComponent = go.GetComponent<STFAppendageNode>();
					var patchComponent = go.GetComponent<STFPatchNode>();
					if(appendageComponent)
					{
						state.RegisterNode(nodeId, STFAppendageNodeExporter.serializeToJson(go, state), go);
					}
					else if(patchComponent)
					{
						state.RegisterNode(nodeId, STFPatchNodeExporter.serializeToJson(go, state), go);
					}
					else
					{
						throw new Exception("Addon Assets must have patch or appendage root nodes");
					}
				}
				else
				{
					if(armatureState.HandleBoneInstance(state, transform) == false)
					{
						state.RegisterNode(nodeId, STFNodeExporter.serializeToJson(go, state), go);
					}
				}
			}

			foreach(var transform in transforms)
			{
				if(transform == rootNode.transform) continue;

				var nodeId = state.GetNodeId(transform.gameObject);
				var components = transform.GetComponents<Component>();
				foreach(var component in components)
				{
					if(component.GetType() == typeof(Transform)) continue;
					if(component.GetType() == typeof(STFUUID)) continue;
					if(component.GetType() == typeof(Animator)) continue;
					if(component.GetType() == typeof(STFAssetInfo)) continue;
					if(component.GetType() == typeof(STFArmatureInstance)) continue;
					if(component.GetType() == typeof(STFAppendageNode)) continue;
					if(component.GetType() == typeof(STFPatchNode)) continue;

					if(component.GetType() == typeof(STFUnrecognizedComponent))
					{
						state.RegisterComponent(nodeId, component);
					}
					else if(state.GetContext().ComponentExporters.ContainsKey(component.GetType()))
					{
						var componentExporter = state.GetContext().ComponentExporters[component.GetType()];
						gatherResources(state, componentExporter.gatherResources(component));
						state.RegisterComponent(nodeId, component, componentExporter);
					}
					else
					{
						Debug.LogWarning("Component not recognized, skipping: " + component.GetType() + " on " + component.gameObject.name);
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
					if(!state.GetContext().ResourceExporters.ContainsKey(resource.GetType()))
						throw new Exception("Unsupported Resource Encountered: " + resource.GetType());
					gatherResources(state, state.GetContext().ResourceExporters[resource.GetType()].gatherResources(resource));
					state.RegisterResource(resource);
				}
			}
		}

		public JToken SerializeToJson(ISTFExporter state)
		{
			var ret = new JObject();
			ret.Add("type", "addon");
			if(name != null && name.Length > 0) ret.Add("name", name);

			var roots = new List<string>();
			for(int i = 0; i < rootNode.transform.childCount; i++)
			{
				roots.Add(state.GetNodeId(rootNode.transform.GetChild(i).gameObject));
			}
			ret.Add("root_nodes", new JArray(roots));
			return ret;
		}

		public string GetId(ISTFExporter state)
		{
			return id;
		}
	}

	public class STFAddonAsset : ISTFAsset
	{
		ISTFImporter state;
		public string id;
		public GameObject holder;
		public string assetName;

		public STFAddonAsset(ISTFImporter state, string id, string name)
		{
			this.state = state;
			this.id = id;
			this.assetName = name;
		}

		public UnityEngine.Object GetAsset()
		{
			return holder;
		}

		public string GetSTFAssetName()
		{
			return assetName;
		}

		public string GetSTFAssetType()
		{
			return "addon";
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

	public class STFAddonAssetImporter : ISTFAssetImporter
	{
		public ISTFAsset ParseFromJson(ISTFImporter state, JToken jsonAsset, string id, JObject jsonRoot)
		{
			var ret = new STFAddonAsset(state, id, (string)jsonAsset["name"]);

			var rootNodeIds = jsonAsset["root_nodes"].ToObject<List<string>>();
			ret.holder = new GameObject();
			ret.holder.name = (string)jsonAsset["name"];
			var assetInfo = ret.holder.AddComponent<STFAssetInfo>();
			assetInfo.assetId = id;
			assetInfo.assetType = "addon";
			assetInfo.assetName = (string)jsonAsset["name"];
			assetInfo.originalMetaInformation = state.GetMeta();

			foreach(var rootNodeId in rootNodeIds)
			{
				convertAssetNode(state, rootNodeId, jsonRoot);
				state.AddTask(new Task(() => {
					var node = state.GetNode(rootNodeId);
					node.transform.parent = ret.holder.transform;
				}));
			}

			return ret;
		}

		private void convertAssetNode(ISTFImporter state, string nodeId, JObject jsonRoot)
		{
			var jsonNode = (JObject)jsonRoot["nodes"][nodeId];
			var nodetype = (string)jsonNode["type"] == null || ((string)jsonNode["type"]).Length == 0 ? "default" : (string)jsonNode["type"];
			if(!state.GetContext().NodeImporters.ContainsKey(nodetype))
				throw new Exception($"Nodetype '{nodetype}' is not supported.");

			var nodesToParse = new List<string>();
			state.AddNode(nodeId, state.GetContext().NodeImporters[nodetype].parseFromJson(state, jsonNode, jsonRoot, out nodesToParse));
			
			if(nodesToParse != null)
			{
				foreach(var childId in nodesToParse)
				{
					convertAssetNode(state, childId, jsonRoot);
				}
			}
		}
	}
}

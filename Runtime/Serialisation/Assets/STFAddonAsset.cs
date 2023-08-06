
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	// Describes a scene which can be applied onto another by the STFAddonApplier. It's root nodes must be either of the STFAppendageNode or STFPatchNode type.

	public class STFAddonAssetExporter : ISTFAssetExporter
	{
		public static string _TYPE = "STF.addon";
		public GameObject rootNode;
		public string id;
		public string name;
		public string targetId;

		public void Convert(ISTFExporter state)
		{
			if(rootNode == null) throw new Exception("Root node must not be null");
			var rootNodeInstance = UnityEngine.Object.Instantiate(rootNode);
			rootNodeInstance.name = rootNode.name;
			state.AddTrashObject(rootNodeInstance);

			STFSetup.SetupAddonAssetInplace(rootNodeInstance);

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

			targetId = rootNode.GetComponent<STFAddonAssetInfo>()?.targetAssetId;

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
						state.RegisterNode(nodeId, STFAppendageNodeExporter.SerializeToJson(go, state), go);
					}
					else if(patchComponent)
					{
						state.RegisterNode(nodeId, STFPatchNodeExporter.SerializeToJson(go, state), go);
					}
					else
					{
						throw new Exception("Addon Assets must have patch or appendage root nodes");
					}
				}
				else
				{
					STFNodeHandler.RegisterNode(state, transform);
				}
			}

			foreach(var transform in transforms)
			{
				if(transform == rootNode.transform) continue;

				var nodeId = state.GetNodeId(transform.gameObject);
				var components = transform.GetComponents<Component>();
				foreach(var component in components)
				{
					// Do this type of shit more legit, with hot pluggable types to skip
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
						gatherResources(state, componentExporter.GatherResources(component));
						state.RegisterComponent(nodeId, component, componentExporter);
					}
					else
					{
						Debug.LogWarning("Component not recognized, skipping: " + component.GetType() + " on " + component.gameObject.name);
					}
				}
			}
		}

		private void gatherResources(ISTFExporter state, List<KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>> resources)
		{
			if(resources != null)
			{
				foreach(var resource in resources)
				{
					if(!state.GetContext().ResourceExporters.ContainsKey(resource.Key.GetType()))
						throw new Exception("Unsupported Resource Encountered: " + resource.Key.GetType());
					gatherResources(state, state.GetContext().ResourceExporters[resource.Key.GetType()].GatherResources(resource.Key));
					state.RegisterResource(resource.Key);
					if(resource.Value != null && resource.Value.Count > 0) foreach(var ctx in resource.Value) state.AddResourceContext(resource.Key, ctx.Key, ctx.Value);
				}
			}
		}

		public JToken SerializeToJson(ISTFExporter state)
		{
			var ret = new JObject();
			ret.Add("type", _TYPE);
			ret.Add("target_asset", targetId);
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
			return STFAddonAssetExporter._TYPE;
		}

		public Type GetUnityAssetType()
		{
			return typeof(GameObject);
		}

		public string getId()
		{
			return id;
		}
		
		public bool isNodeInAsset(string id)
		{
			return holder.GetComponentsInChildren<STFUUID>().FirstOrDefault(n => n.id == id) != null;
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
			assetInfo.assetType = STFAddonAssetExporter._TYPE;
			assetInfo.assetName = (string)jsonAsset["name"];
			assetInfo.originalMetaInformation = state.GetMeta();
			var addonAssetInfo = ret.holder.AddComponent<STFAddonAssetInfo>();
			addonAssetInfo.targetAssetId = (string)jsonAsset["target_asset"];

			foreach(var rootNodeId in rootNodeIds)
			{
				convertAssetNode(state, rootNodeId, jsonRoot, ret);
				state.AddTask(new Task(() => {
					var node = state.GetNode(rootNodeId);
					node.transform.parent = ret.holder.transform;
				}));
			}

			return ret;
		}

		private void convertAssetNode(ISTFImporter state, string nodeId, JObject jsonRoot, ISTFAsset asset)
		{
			var jsonNode = (JObject)jsonRoot["nodes"][nodeId];
			var nodetype = (string)jsonNode["type"] == null || ((string)jsonNode["type"]).Length == 0 ? "default" : (string)jsonNode["type"];
			if(!state.GetContext().NodeImporters.ContainsKey(nodetype))
				throw new Exception($"Nodetype '{nodetype}' is not supported.");

			var nodesToParse = new List<string>();
			state.AddNode(nodeId, state.GetContext().NodeImporters[nodetype].ParseFromJson(state, jsonNode, jsonRoot, out nodesToParse));
			
			if(nodesToParse != null)
			{
				foreach(var childId in nodesToParse)
				{
					convertAssetNode(state, childId, jsonRoot, asset);
				}
			}
			
			var go = state.GetNode(nodeId);
			if((JObject)jsonNode["components"] != null)
			{
				state.AddTask(new Task(() => {
					foreach(var jsonComponent in (JObject)jsonNode["components"])
					{
						if((string)jsonComponent.Value["type"] != null && state.GetContext().ComponentImporters.ContainsKey((string)jsonComponent.Value["type"]))
						{
							var componentImporter = state.GetContext().ComponentImporters[(string)jsonComponent.Value["type"]];
							componentImporter.ParseFromJson(state, asset, jsonComponent.Value, jsonComponent.Key, go);
						}
						else
						{
							var unrecognizedComponent = (STFUnrecognizedComponent)go.AddComponent<STFUnrecognizedComponent>();
							unrecognizedComponent.id = jsonComponent.Key;
							unrecognizedComponent.ParseFromJson(state, jsonComponent.Value);
						}
					}
				}));
			}
		}
	}
}

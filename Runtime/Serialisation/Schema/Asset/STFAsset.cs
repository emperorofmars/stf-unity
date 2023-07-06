
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	public class STFAssetExporter : ISTFAssetExporter
	{
		public GameObject rootNode;
		public string id;
		public string name;
		public string rootNodeId;

		public void Convert(ISTFExporter state)
		{
			if(rootNode == null) throw new Exception("Root node must not be null");
			var rootNodeInstance = UnityEngine.Object.Instantiate(rootNode);
			rootNodeInstance.name = rootNode.name;
			state.AddTrashObject(rootNodeInstance);

			// Fill in all Id's, find armatures
			STFSetup.SetupStandaloneAssetInplace(rootNodeInstance);

			var assetInfo = rootNodeInstance.GetComponent<STFAssetInfo>();
			id = assetInfo.assetId != null && assetInfo.assetId.Length > 0 ? assetInfo.assetId : Guid.NewGuid().ToString();
			name = assetInfo.assetName != null && assetInfo.assetName.Length > 0 ? assetInfo.assetName : rootNode.name;
			if(assetInfo.originalMetaInformation != null) state.AddMeta(assetInfo.originalMetaInformation);

			// TODO: handle node export by hot loaded components, determine which to use by explicit function
			
			var transforms = rootNodeInstance.GetComponentsInChildren<Transform>();
			foreach(var transform in transforms)
			{
				STFNodeHandler.RegisterNode(state, transform);
			}

			// Export Components and Resources
			foreach(var transform in transforms)
			{
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
			rootNodeId = state.GetNodeId(rootNodeInstance);
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
			ret.Add("type", "asset");
			if(name != null && name.Length > 0) ret.Add("name", name);
			ret.Add("root_node", rootNodeId);
			return ret;
		}

		public string GetId(ISTFExporter state)
		{
			return id;
		}
	}

	public class STFAsset : ISTFAsset
	{
		ISTFImporter state;
		public string id;
		public string rootNodeId;
		public string assetName;

		public STFAsset(ISTFImporter state, string id, string name)
		{
			this.state = state;
			this.id = id;
			this.assetName = name;
		}

		public UnityEngine.Object GetAsset()
		{
			return state.GetNode(rootNodeId);
		}

		public string GetSTFAssetName()
		{
			return assetName;
		}

		public string GetSTFAssetType()
		{
			return "asset";
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
			return state.GetNode(rootNodeId).GetComponentsInChildren<STFUUID>().FirstOrDefault(n => n.id == id) != null;
		}
	}

	public class STFAssetImporter : ISTFAssetImporter
	{
		public ISTFAsset ParseFromJson(ISTFImporter state, JToken jsonAsset, string id, JObject jsonRoot)
		{
			var ret = new STFAsset(state, id, (string)jsonAsset["name"]);

			var rootNodeId = (string)jsonAsset["root_node"];
			ret.rootNodeId = rootNodeId;
			convertAssetNode(state, rootNodeId, jsonRoot, ret);
			state.AddTask(new Task(() => {
				var assetInfo = state.GetNode(rootNodeId).AddComponent<STFAssetInfo>();
				assetInfo.assetId = id;
				assetInfo.assetType = "asset";
				assetInfo.assetName = (string)jsonAsset["name"];
				assetInfo.originalMetaInformation = state.GetMeta();
			}));
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

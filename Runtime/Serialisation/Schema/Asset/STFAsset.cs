
using System;
using System.Collections.Generic;
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

		public Dictionary<Transform, string> boneMappings = new Dictionary<Transform, string>();
		private Dictionary<Transform, STFArmatureResourceExporter> armatureInstances = new Dictionary<Transform, STFArmatureResourceExporter>();
		private Dictionary<Transform, Transform[]> armatureInstancesBoneInstances = new Dictionary<Transform, Transform[]>();

		public void Convert(ISTFExporter state)
		{
			if(rootNode == null) throw new Exception("Root node must not be null");

			var assetInfo = rootNode.GetComponent<STFAssetInfo>();
			if(assetInfo != null)
			{
				id = assetInfo.assetId != null && assetInfo.assetId.Length > 0 ? assetInfo.assetId : Guid.NewGuid().ToString();
				name = assetInfo.assetName != null && assetInfo.assetName.Length > 0 ? assetInfo.assetName : rootNode.name;
			} else
			{
				id = Guid.NewGuid().ToString();
				name = rootNode.name;
			}

			GatherArmatures(state, rootNode.GetComponentsInChildren<SkinnedMeshRenderer>());

			var transforms = rootNode.GetComponentsInChildren<Transform>();
			RegisterNodes(state, transforms);

			foreach(var transform in transforms)
			{
				var nodeId = state.GetNodeId(transform.gameObject);
				var components = transform.GetComponents<Component>();
				foreach(var component in components)
				{
					if(component.GetType() == typeof(Transform)) continue;
					if(component.GetType() == typeof(STFUUID)) continue;
					if(component.GetType() == typeof(Animator)) continue;

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

		private void GatherArmatures(ISTFExporter state, SkinnedMeshRenderer[] skinnedMeshRenderers)
		{
			var armatures = new Dictionary<Mesh, STFArmatureResourceExporter>();
			foreach(var smr in skinnedMeshRenderers)
			{
				// find if the same armature already exists
				foreach(var smr2 in skinnedMeshRenderers)
				{
					// TODO: try also comparing the bindpose values
					if(smr2.rootBone == smr.rootBone && armatures.ContainsKey(smr2.sharedMesh))
					{
						if(!armatures.ContainsKey(smr.sharedMesh))
						{
							armatures.Add(smr.sharedMesh, armatures[smr2.sharedMesh]);
							//armatures[smr2.sharedMesh].meshes.Add(smr.sharedMesh);
							state.RegisterSubresourceId(smr.sharedMesh, "armature", armatures[smr2.sharedMesh].id);

						}
					}
				}
				if(!armatures.ContainsKey(smr.sharedMesh))
				{
					var armature = new STFArmatureResourceExporter();
					armature.SetupFromSkinnedMeshRenderer(state, smr);
					armatures.Add(smr.sharedMesh, armature);
					//armature.meshes.Add(smr.sharedMesh);
					state.RegisterSubresourceId(smr.sharedMesh, "armature", armature.id);

					armatureInstances.Add(smr.rootBone.parent, armature);
				}
				for(int i = 0; i < smr.bones.Length; i++)
				{
					boneMappings.Add(smr.bones[i], armatures[smr.sharedMesh].boneIds[i]);
				}
				armatureInstancesBoneInstances.Add(smr.rootBone.parent, smr.bones);
			}
		}

		private void RegisterNodes(ISTFExporter state, Transform[] transforms)
		{
			foreach(var transform in transforms)
			{
				var go = transform.gameObject;
				var nodeUuidComponent = go.GetComponent<STFUUID>();
				var nodeId = nodeUuidComponent != null && nodeUuidComponent.id != null && nodeUuidComponent.id.Length > 0 ? nodeUuidComponent.id : Guid.NewGuid().ToString();
				
				if(!boneMappings.ContainsKey(go.transform))
				{
					if(!armatureInstances.ContainsKey(go.transform))
					{
						state.RegisterNode(nodeId, STFNodeExporter.serializeToJson(go, state), go);
					}
					else
					{
						var node = STFArmatureInstanceNodeExporter.serializeToJson(go, state, armatureInstances[go.transform].id, armatureInstancesBoneInstances[go.transform]);
						state.RegisterNode(nodeId, node, go);
					}
				}
				else
				{
					Transform armatureInstance = null;
					foreach(var armatureInstanceMapping in armatureInstancesBoneInstances)
					{
						foreach(var t in armatureInstanceMapping.Value)
						{
							if(t == go.transform)
							{
								armatureInstance = armatureInstanceMapping.Key;
								break;
							}
						}
						if(armatureInstance != null) break;
					}
					var node = STFBoneInstanceNodeExporter.serializeToJson(go, state, boneMappings[go.transform], armatureInstancesBoneInstances[armatureInstance]);
					state.RegisterNode(nodeId, node, go);
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
			ret.Add("type", "asset");
			if(name != null && name.Length > 0) ret.Add("name", name);
			ret.Add("root_node", state.GetNodeId(rootNode));
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
	}

	public class STFAssetImporter : ISTFAssetImporter
	{
		public ISTFAsset ParseFromJson(ISTFImporter state, JToken jsonAsset, string id, JObject jsonRoot)
		{
			var ret = new STFAsset(state, id, (string)jsonAsset["name"]);

			var rootNodeId = (string)jsonAsset["root_node"];
			ret.rootNodeId = rootNodeId;
			convertAssetNode(state, rootNodeId, jsonRoot);
			state.AddTask(new Task(() => {
				var assetInfo = state.GetNode(rootNodeId).AddComponent<STFAssetInfo>();
				assetInfo.assetId = id;
				assetInfo.assetType = "asset";
				assetInfo.assetName = (string)jsonAsset["name"];
			}));
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

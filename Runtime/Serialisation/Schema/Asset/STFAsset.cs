
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
		private STFNodeExporter _nodeExporter = new STFNodeExporter();
		public string id = Guid.NewGuid().ToString();

		public Dictionary<Transform, string> boneMappings = new Dictionary<Transform, string>();
		private Dictionary<Transform, STFArmatureResource> armatureInstances = new Dictionary<Transform, STFArmatureResource>();
		private Dictionary<Transform, Transform[]> armatureInstancesBoneInstances = new Dictionary<Transform, Transform[]>();

		public void Convert(ISTFExporter state)
		{
			if(rootNode == null) throw new Exception("Root node must not be null");

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
			// TODO: handle already existing armatures
			var armatures = new Dictionary<Mesh, STFArmatureResource>();
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
					var armature = new STFArmatureResource();
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
			/*state.AddTask(new Task(() => {
				foreach(var armature in armatures)
				{
					state.re
					armature.Key
				}
			}));*/
		}

		private void RegisterNodes(ISTFExporter state, Transform[] transforms)
		{
			foreach(var transform in transforms)
			{
				var go = transform.gameObject;
				string nodeId = null;
				if(!boneMappings.ContainsKey(go.transform))
				{
					if(!armatureInstances.ContainsKey(go.transform))
						nodeId = state.RegisterNode(go, _nodeExporter);
					else
					{
						var node = (JObject)_nodeExporter.serializeToJson(go, state);
						node.Add("type", "armature_instance");
						node.Add("armature", armatureInstances[go.transform].id);

						state.AddTask(new Task(() => {
							var boneInstanceIds = new List<string>();
							foreach(var boneInstance in armatureInstancesBoneInstances[go.transform])
								boneInstanceIds.Add(state.GetNodeId(boneInstance.gameObject));
							node.Add("bone_instances", new JArray(boneInstanceIds));
						}));

						var nodeUuidComponent = go.GetComponent<STFUUID>();
						nodeId = nodeUuidComponent != null && nodeUuidComponent.id != null && nodeUuidComponent.id.Length > 0 ? nodeUuidComponent.id : Guid.NewGuid().ToString();
						state.RegisterNode(nodeId, node, go);
					}
				}
				else
				{
					var node = (JObject)_nodeExporter.serializeBoneInstanceToJson(go, state, boneMappings[go.transform]);
					var nodeUuidComponent = go.GetComponent<STFUUID>();
					nodeId = nodeUuidComponent != null && nodeUuidComponent.id != null && nodeUuidComponent.id.Length > 0 ? nodeUuidComponent.id : Guid.NewGuid().ToString();
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

		public STFAsset(ISTFImporter state, string id)
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

	public class STFAssetImporter : ISTFAssetImporter
	{
		private STFNodeImporter _importer = new STFNodeImporter();

		public ISTFAsset ParseFromJson(ISTFImporter state, JToken jsonAsset, string id, JObject jsonRoot)
		{
			var ret = new STFAsset(state, id);
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
}

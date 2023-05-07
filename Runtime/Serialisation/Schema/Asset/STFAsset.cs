
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

		private Dictionary<Mesh, STFArmatureResource> armatures = new Dictionary<Mesh, STFArmatureResource>();

		public void Convert(ISTFExporter state)
		{
			if(rootNode == null) throw new Exception("Root node must not be null");

			// gather meshes and skeletons
			gatherArmatures(state, rootNode.GetComponentsInChildren<SkinnedMeshRenderer>());
			// register armature bones

			var transforms = rootNode.GetComponentsInChildren<Transform>();
			foreach(var transform in transforms)
			{
				//check if node is already registered
				var nodeId = state.RegisterNode(transform.gameObject, _nodeExporter);
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

		private void gatherArmatures(ISTFExporter state, SkinnedMeshRenderer[] skinnedMeshRenderers)
		{
			foreach(var smr in skinnedMeshRenderers)
			{
				if(!armatures.ContainsKey(smr.sharedMesh))
				{
					var armature = new STFArmatureResource();
					//armature.setup(state, smr.rootBone, smr.bones, smr.sharedMesh.bindposes);
					armature.setupFromSkinnedMeshRenderer(state, smr);
					armatures.Add(smr.sharedMesh, armature);
				}
			}

			/*var smrPerMesh = new Dictionary<Mesh, SkinnedMeshRenderer>();
			foreach(var smr in skinnedMeshRenderers)
			{
				// handle armature id from stfmeta
				var newArmature = new STFArmatureResource();
				newArmature.armatureRootTransform = smr.rootBone.parent;
				newArmature.rootBone = smr.rootBone;
				newArmature.bones = smr.bones;
				if(!state.HasArmature(smr.sharedMesh))
				{
					state.SetArmature(smr.sharedMesh, newArmature);
					foreach(var bone in smr.bones) state.RegisterArmatureNode(newArmature, bone.gameObject);
				}
				else if(state.GetArmature(smr.sharedMesh).calculateArmatureDeviation(smr.sharedMesh, smr.transform) > newArmature.calculateArmatureDeviation(smr.sharedMesh, smr.transform))
				{
					var armature = state.GetArmature(smr.sharedMesh);
					armature.armatureRootTransform = smr.rootBone.parent;
					armature.rootBone = smr.rootBone;
					armature.bones = smr.bones;
					state.SetArmature(smr.sharedMesh, armature);
					foreach(var bone in smr.bones) state.RegisterArmatureNode(armature, bone.gameObject);
				}
				else
				{
					var armature = state.GetArmature(smr.sharedMesh);
					foreach(var bone in smr.bones) state.RegisterArmatureNode(armature, bone.gameObject);
				}
			}*/
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

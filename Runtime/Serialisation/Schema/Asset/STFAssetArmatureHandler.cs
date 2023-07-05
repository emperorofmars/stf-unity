
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	public class STFAssetArmatureHandler
	{
		public Dictionary<Transform, string> boneMappings = new Dictionary<Transform, string>();
		public Dictionary<Transform, STFArmatureResourceExporter> armatureInstances = new Dictionary<Transform, STFArmatureResourceExporter>();
		public Dictionary<Transform, Transform[]> armatureInstancesBoneInstances = new Dictionary<Transform, Transform[]>();

		public void GatherArmatures(ISTFExporter state, SkinnedMeshRenderer[] skinnedMeshRenderers, GameObject rootNode)
		{
			var armatures = new Dictionary<Mesh, STFArmatureResourceExporter>();
			var tree = rootNode.GetComponentsInChildren<Transform>();

			foreach(var smr in skinnedMeshRenderers)
			{
				// Not in tree
				if(tree.FirstOrDefault(t => t == smr.rootBone) == null)
				{
					var externalArmatureInstance = smr.rootBone?.parent?.GetComponent<STFArmatureInstance>();
					if(externalArmatureInstance)
					{
						state.AddResourceContext(smr.sharedMesh, "armature", externalArmatureInstance.GetComponent<STFUUID>().id);
					}
					else if(smr.gameObject.GetComponent<STFSkinnedMeshRendererAddon>() != null)
					{
						var smrAddon = smr.gameObject.GetComponent<STFSkinnedMeshRendererAddon>();
						state.AddResourceContext(smr.sharedMesh, "armature", smrAddon.ArmatureInstanceId);
					}
					else
					{
						throw new Exception("External Armature not present!");
					}
					continue;
				}
				
				// find if the same armature already exists
				foreach(var smr2 in skinnedMeshRenderers)
				{
					if(smr2.rootBone == smr.rootBone && armatures.ContainsKey(smr2.sharedMesh))
					{
						if(!armatures.ContainsKey(smr.sharedMesh))
						{
							armatures.Add(smr.sharedMesh, armatures[smr2.sharedMesh]);
							state.AddResourceContext(smr.sharedMesh, "armature", armatures[smr2.sharedMesh].id);
						}
					}
				}
				if(!armatures.ContainsKey(smr.sharedMesh))
				{
					var armature = new STFArmatureResourceExporter();
					armature.SetupFromSkinnedMeshRenderer(state, smr);
					armatures.Add(smr.sharedMesh, armature);
					state.AddResourceContext(smr.sharedMesh, "armature", armature.id);

					armatureInstances.Add(smr.rootBone.parent, armature);
					for(int i = 0; i < smr.bones.Length; i++)
					{
						boneMappings.Add(smr.bones[i], armatures[smr.sharedMesh].boneIds[i]);
					}
					armatureInstancesBoneInstances.Add(smr.rootBone.parent, smr.bones);
				}
			}
		}

		public bool HandleBoneInstance(ISTFExporter state, Transform transform)
		{
			var go = transform.gameObject;
			var nodeUuidComponent = go.GetComponent<STFUUID>();
			var nodeId = nodeUuidComponent != null && nodeUuidComponent.id != null && nodeUuidComponent.id.Length > 0 ? nodeUuidComponent.id : Guid.NewGuid().ToString();
			
			if(!boneMappings.ContainsKey(go.transform))
			{
				if(!armatureInstances.ContainsKey(go.transform))
				{
					// Normal Node
					//state.RegisterNode(nodeId, STFNodeExporter.serializeToJson(go, state), go);
					return false;
				}
				else
				{
					// Armature Instance Node
					var node = STFArmatureInstanceNodeExporter.SerializeToJson(go, state, armatureInstances[go.transform].id, armatureInstancesBoneInstances[go.transform]);
					state.RegisterNode(nodeId, node, go);
					return true;
				}
			}
			else
			{
				// Bone Instance Node
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
				var node = STFBoneInstanceNodeExporter.SerializeToJson(go, state, boneMappings[go.transform], armatureInstancesBoneInstances[armatureInstance]);
				state.RegisterNode(nodeId, node, go);
				return true;
			}
		}
	}
}

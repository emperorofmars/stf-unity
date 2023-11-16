
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	// Determine which node type is being parsed.
	// TODO: Completely redo how nodes are exported. Register node-exporters in the registry along with a function for each to determine the priority of handling a unity Gameobject instead of this.

	public static class STFNodeHandler
	{
		public static void RegisterNode(ISTFExporter state, Transform transform)
		{
			var go = transform.gameObject;
			var uuidComponent = go.GetComponent<STFUUID>();
			var armatureInstance = go.GetComponent<STFArmatureInstance>();
			var nodeId = uuidComponent != null && uuidComponent.id != null && uuidComponent.id.Length > 0 ? uuidComponent.id : Guid.NewGuid().ToString();

			// Armature Instance Node
			if(armatureInstance != null)
			{
				var boneInstances = armatureInstance.bones.Select(b => b.transform).ToArray();
				state.RegisterResource(armatureInstance.armature);
				
				var node = STFArmatureInstanceNodeExporter.SerializeToJson(go, state, state.GetResourceId(armatureInstance.armature), boneInstances);
				state.RegisterNode(nodeId, node, go);
				foreach(var bone in armatureInstance.bones)
				{
					var boneIdComponent = bone.GetComponent<STFUUID>();
					var boneNode = STFBoneInstanceNodeExporter.SerializeToJson(bone.gameObject, state, boneIdComponent.boneId, boneInstances);
					state.RegisterNode(boneIdComponent.id, boneNode, bone.gameObject);
				}
				return;
			}
			// Bone Instance Node
			else if(!String.IsNullOrWhiteSpace(uuidComponent.boneId))
			{
				// skip, will be registered by with the armature instance above
				return;
			}
			else
			{
				// Normal Node
				state.RegisterNode(nodeId, STFNodeExporter.SerializeToJson(go, state), go);
				return;
			}
		}
	}
}


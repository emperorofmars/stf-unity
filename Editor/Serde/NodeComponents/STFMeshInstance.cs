using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serde
{
	public class STFMeshInstance : ASTFNodeComponent
	{
		public STFArmatureInstanceNode ArmatureInstance;
		public string ArmatureInstanceId;
	}

	public class STFMeshInstanceExporter : ASTFNodeComponentExporter
	{
		override public JObject SerializeToJson(STFExportState state, Component component)
		{
			SkinnedMeshRenderer c = (SkinnedMeshRenderer)component;
			var ret = new JObject();
			/*ret.Add("type", "STF.mesh_instance");
			ret.Add("mesh", state.GetResourceId(c.sharedMesh));

			var smrAddon = c.gameObject.GetComponent<STFSkinnedMeshRendererAddon>();
			if(smrAddon) ret.Add("armature_instance", smrAddon.ArmatureInstanceId);
			else ret.Add("armature_instance", state.GetNodeId(c.rootBone.parent.gameObject));

			ret.Add("materials", new JArray(c.sharedMaterials.Select(m => m != null ? state.GetResourceId(m) : null)));
			ret.Add("morphtarget_values", new JArray(Enumerable.Range(0, c.sharedMesh.blendShapeCount).Select(i => c.GetBlendShapeWeight(i))));
			
			ret.Add("resources_used", new JArray(state.GetResourceId(c.sharedMesh), ret["armature_instance"]));
			((JArray)ret["resources_used"]).Merge(ret["morphtarget_values"]);*/
			return ret;
		}
	}

	public class STFMeshInstanceImporter : ASTFNodeComponentImporter
	{
		public static string _TYPE = "STF.mesh_instance";

		public override void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<SkinnedMeshRenderer>();
			var meshInstanceComponent = Go.AddComponent<STFMeshInstance>();
			meshInstanceComponent.Id = Id;
			//state.AddComponent(id, c);

			Debug.Log(Json);

			var meta = (STFMesh)State.Resources[(string)Json["mesh"]];
			var resource = meta.Resource;
			if(resource.GetType() == typeof(Mesh))
			{
				c.sharedMesh = (Mesh)resource;
			}
			else if(resource.GetType() == typeof(GameObject))
			{
				var renderer = ((GameObject)resource).GetComponent<SkinnedMeshRenderer>();
				c.sharedMesh = renderer.sharedMesh;
			}

			if((string)Json["armature_instance"] != null)
			{
				var armatureInstanceNode = State.Nodes[(string)Json["armature_instance"]];
				if(armatureInstanceNode != null)
				{
					var armatureInstance = armatureInstanceNode.GetComponent<STFArmatureInstanceNode>();
					meshInstanceComponent.ArmatureInstance = armatureInstance;
					c.rootBone = armatureInstance.root.transform;
					c.bones = armatureInstance.bones.Select(b => b.transform).ToArray();
					c.updateWhenOffscreen = true;
				}
				else
				{
					meshInstanceComponent.ArmatureInstanceId = (string)Json["armature_instance"];
				}
			}

			/*var materials = new Material[c.sharedMesh.subMeshCount];
			for(int i = 0; i < materials.Length; i++)
			{
				if(Json["materials"][i] != null) materials[i] = (Material)State.Resources[(string)Json["materials"][i]];
			}*/
			if(c.sharedMesh.blendShapeCount > 0 && Json["morphtarget_values"] != null)
			{
				for(int i = 0; i < c.sharedMesh.blendShapeCount; i++)
				{
					c.SetBlendShapeWeight(i, (float)Json["morphtarget_values"][i]);
				}

			}
			//c.sharedMaterials = materials;
			c.localBounds = c.sharedMesh.bounds;
		}
	}
}

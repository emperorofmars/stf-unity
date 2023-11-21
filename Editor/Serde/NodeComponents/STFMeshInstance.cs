
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serde
{
	public class STFMeshInstance : ASTFNodeComponent
	{
		public static string _TYPE = "STF.mesh_instance";
		public STFArmatureInstanceNode ArmatureInstance;
		public string ArmatureInstanceId;
	}

	public class STFMeshInstanceExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public override KeyValuePair<string, JObject> SerializeToJson(ISTFExportState State, Component component)
		{
			SkinnedMeshRenderer c = (SkinnedMeshRenderer)component;
			var meshId = STFSerdeUtil.SerializeResource(State, c.sharedMesh);

			var ret = new JObject {
				{"type", STFMeshInstance._TYPE},
				{"mesh", meshId}
			};

			var meshInstance = c.gameObject.GetComponent<STFMeshInstance>();
			if(meshInstance.ArmatureInstanceId != null && meshInstance.ArmatureInstanceId.Length > 0) ret.Add("armature_instance", meshInstance.ArmatureInstanceId);
			else ret.Add("armature_instance", State.Nodes[c.rootBone.parent.gameObject].Key);

			//ret.Add("materials", new JArray(c.sharedMaterials.Select(m => m != null ? State.Resources[m].Key : null)));
			ret.Add("morphtarget_values", new JArray(Enumerable.Range(0, c.sharedMesh.blendShapeCount).Select(i => c.GetBlendShapeWeight(i))));
			
			ret.Add("resources_used", new JArray(meshId, ret["armature_instance"])); // add materials
			//((JArray)ret["resources_used"]).Merge(ret["morphtarget_values"]);
			return new KeyValuePair<string, JObject>(meshInstance.Id, ret);
		}
	}

	public class STFMeshInstanceImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public override void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<SkinnedMeshRenderer>();
			var meshInstanceComponent = Go.AddComponent<STFMeshInstance>();
			meshInstanceComponent.Id = Id;
			State.AddComponent(meshInstanceComponent, Id);

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

#endif

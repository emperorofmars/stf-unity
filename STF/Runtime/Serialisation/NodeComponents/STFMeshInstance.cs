
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFMeshInstance : ISTFNodeComponent
	{
		public const string _TYPE = "STF.mesh_instance";
		public override string Type => _TYPE;
		public STFArmatureInstanceNode ArmatureInstance;
		public string ArmatureInstanceId;
		public List<MTF.Material> Materials = new List<MTF.Material>();
	}

	public class STFMeshInstanceExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(ISTFExportState State, Component Component, string UnityProperty)
		{
			if(UnityProperty.StartsWith("blendShape"))
			{
				return "blendshape." + UnityProperty.Split('.')[1];
			}
			else if(UnityProperty.StartsWith("material"))
			{
				if(Component is SkinnedMeshRenderer)
				{
					return State.Context.ResourceExporters[typeof(MTF.Material)].ConvertPropertyPath(State, ((SkinnedMeshRenderer)Component).material, UnityProperty.Split('.')[1]);
				}
				else if(Component is STFMeshInstance)
				{
					return State.Context.ResourceExporters[typeof(MTF.Material)].ConvertPropertyPath(State, ((STFMeshInstance)Component).Materials[0], UnityProperty.Split('.')[1]);
				}
			}
			throw new Exception("Unrecognized animation property: " + UnityProperty);
		}

		public override (string, JObject) SerializeToJson(ISTFExportState State, Component component)
		{
			SkinnedMeshRenderer c = (SkinnedMeshRenderer)component;
			var meshId = SerdeUtil.SerializeResource(State, c.sharedMesh);

			var ret = new JObject {
				{"type", STFMeshInstance._TYPE},
				{"mesh", meshId}
			};

			var meshInstance = c.gameObject.GetComponent<STFMeshInstance>();
			SerializeRelationships(meshInstance, ret);
			
			if(meshInstance.ArmatureInstanceId != null && meshInstance.ArmatureInstanceId.Length > 0) ret.Add("armature_instance", meshInstance.ArmatureInstanceId);
			else ret.Add("armature_instance", State.Nodes[c.rootBone.parent.gameObject].Id);

			var materials = new JArray();
			for(int matIdx = 0; matIdx < c.sharedMaterials.Length; matIdx++)
			{
				if(meshInstance.Materials.Count > matIdx && meshInstance.Materials[matIdx] != null)
				{
					materials.Add(SerdeUtil.SerializeResource(State, meshInstance.Materials[matIdx]));
				}
				else
				{
					materials.Add(SerdeUtil.SerializeResource(State, c.sharedMaterials[matIdx]));
				}
			}
			
			ret.Add("materials", materials);
			ret.Add("morphtarget_values", new JArray(Enumerable.Range(0, c.sharedMesh.blendShapeCount).Select(i => c.GetBlendShapeWeight(i))));

			var resourcesUsed = new JArray(meshId, ret["armature_instance"]);
			foreach(var m in ret["materials"]) if(m != null) resourcesUsed.Add(m);
			ret.Add("resources_used", resourcesUsed);
			return (meshInstance.Id, ret);
		}
	}

	public class STFMeshInstanceImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(ISTFImportState State, Component Component, string STFProperty)
		{
			if(STFProperty.StartsWith("blendshape"))
			{
				return "blendShape." + STFProperty.Split('.')[1];
			}
			throw new Exception("Unrecognized animation property: " + STFProperty);
		}

		public override void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go)
		{
			// handle non skinned meshes as well
			var c = Go.AddComponent<SkinnedMeshRenderer>();
			var meshInstanceComponent = Go.AddComponent<STFMeshInstance>();
			ParseRelationships(Json, meshInstanceComponent);
			meshInstanceComponent.Id = Id;
			meshInstanceComponent.OwnedUnityComponent = c;
			State.AddComponent(meshInstanceComponent, Id);

			var meta = (STFMesh)State.Resources[(string)Json["mesh"]];
			var resource = meta.Resource;
			c.sharedMesh = (Mesh)resource;

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

			var materials = new UnityEngine.Material[c.sharedMesh.subMeshCount];
			meshInstanceComponent.Materials = new List<MTF.Material>(new MTF.Material[c.sharedMesh.subMeshCount]);
			for(int i = 0; i < materials.Length; i++)
			{
				try{
					if((string)Json["materials"][i] != null && State.Resources.ContainsKey((string)Json["materials"][i]))
					{
						var mtfMaterial = (MTF.Material)State.Resources[(string)Json["materials"][i]];
						meshInstanceComponent.Materials[i] = mtfMaterial;
						materials[i] = mtfMaterial?.ConvertedMaterial;
					}
					else
					{
						Debug.LogWarning("Material Import Error, Falling back to default material.");
						var mtfMaterial = MTF.Material.CreateDefaultMaterial();
						meshInstanceComponent.Materials[i] = mtfMaterial;
						// actually convert
						materials[i] = mtfMaterial?.ConvertedMaterial;
					}
				}
				catch(Exception)
				{
					Debug.LogWarning("Material Import Error, Skipping.");
				}
			}
			if(c.sharedMesh.blendShapeCount > 0 && Json["morphtarget_values"] != null)
			{
				for(int i = 0; i < c.sharedMesh.blendShapeCount; i++)
				{
					c.SetBlendShapeWeight(i, (float)Json["morphtarget_values"][i]);
				}

			}
			c.sharedMaterials = materials;
			c.localBounds = c.sharedMesh.bounds;
		}
	}
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STF.Addon;
using STF.ApplicationConversion;
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
				return "blendshape." + UnityProperty.Substring(UnityProperty.IndexOf('.') + 1);
			}
			else if(UnityProperty.StartsWith("material")) // figure out which material at which index
			{
				var materialProperty = UnityProperty.Substring(UnityProperty.IndexOf('.') + 1);
				//var matIdx = int.Parse(UnityProperty.Split(':')[1].Split('.')[0]);
				if(Component is Renderer)
				{
					// handle material index somehow
					return "material." + State.Context.ResourceExporters[typeof(Material)].ConvertPropertyPath(State, ((Renderer)Component).sharedMaterial, materialProperty);
				}
				else if(Component is STFMeshInstance)
				{
					return "material." + State.Context.ResourceExporters[typeof(MTF.Material)].ConvertPropertyPath(State, ((STFMeshInstance)Component).Materials[0], materialProperty);
				}
			}
			throw new Exception("Unrecognized animation property: " + UnityProperty);
		}

		public override (string, JObject) SerializeToJson(ISTFExportState State, Component Component)
		{
			Renderer renderer = Component as Renderer;
			Mesh mesh;
			if(Component is SkinnedMeshRenderer) mesh = (renderer as SkinnedMeshRenderer).sharedMesh;
			else mesh = (renderer as MeshRenderer).gameObject.GetComponent<MeshFilter>().sharedMesh;
			
			var meshId = SerdeUtil.SerializeResource(State, mesh);

			var ret = new JObject {
				{"type", STFMeshInstance._TYPE},
				{"mesh", meshId}
			};

			var meshInstance = Component.gameObject.GetComponent<STFMeshInstance>();
			SerializeRelationships(meshInstance, ret);

			if(Component is SkinnedMeshRenderer)
			{
				if(meshInstance.ArmatureInstanceId != null && meshInstance.ArmatureInstanceId.Length > 0) ret.Add("armature_instance", meshInstance.ArmatureInstanceId);
				else ret.Add("armature_instance", (renderer as SkinnedMeshRenderer).rootBone.parent.GetComponent<STFArmatureInstanceNode>()?.Id);
			}

			var materials = new JArray();
			for(int matIdx = 0; matIdx < renderer.sharedMaterials.Length; matIdx++)
			{
				if(meshInstance != null && meshInstance.Materials.Count > matIdx && meshInstance.Materials[matIdx] != null)
				{
					materials.Add(SerdeUtil.SerializeResource(State, meshInstance.Materials[matIdx]));
				}
				else
				{
					materials.Add(SerdeUtil.SerializeResource(State, renderer.sharedMaterials[matIdx]));
				}
			}
			
			ret.Add("materials", materials);
			ret.Add("morphtarget_values", new JArray(Enumerable.Range(0, mesh.blendShapeCount).Select(i => Component is SkinnedMeshRenderer ? (renderer as SkinnedMeshRenderer).GetBlendShapeWeight(i) : 0)));

			var resourcesUsed = new JArray(meshId, ret["armature_instance"]);
			foreach(var m in ret["materials"]) if(m != null) resourcesUsed.Add(m);
			ret.Add("resources_used", resourcesUsed);
			return (meshInstance != null ? meshInstance.Id : Guid.NewGuid().ToString(), ret);
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
			else if(STFProperty.StartsWith("material"))
			{
				var matIdx = int.Parse(STFProperty.Split(':')[1].Split('.')[0]);

				return "material." + State.Context.ResourceImporters[MTFMaterialImporter._TYPE].ConvertPropertyPath(State, ((STFMeshInstance)Component).Materials[matIdx], STFProperty.Substring(STFProperty.IndexOf('.') + 1));
			}
			throw new Exception("Unrecognized animation property: " + STFProperty);
		}

		public override void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go)
		{
			var meta = (STFMesh)State.Resources[(string)Json["mesh"]];
			var meshInstanceComponent = Go.AddComponent<STFMeshInstance>();
			meshInstanceComponent.Id = Id;
			ParseRelationships(Json, meshInstanceComponent);
			State.AddComponent(meshInstanceComponent, Id);

			Mesh mesh = (Mesh)meta.Resource;
			Renderer renderer;

			if(meta.ArmatureId != null && meta.ArmatureId.Length > 0)
			{
				var c = Go.AddComponent<SkinnedMeshRenderer>();
				renderer = c;
				meshInstanceComponent.OwnedUnityComponent = c;

				c.sharedMesh = mesh;

				if((string)Json["armature_instance"] != null)
				{
					if(State.Nodes.ContainsKey((string)Json["armature_instance"]))
					{
						var armatureInstanceNode = State.Nodes[(string)Json["armature_instance"]];
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
				
				if(c.sharedMesh.blendShapeCount > 0 && Json["morphtarget_values"] != null)
				{
					for(int i = 0; i < c.sharedMesh.blendShapeCount; i++)
					{
						c.SetBlendShapeWeight(i, (float)Json["morphtarget_values"][i]);
					}

				}
				c.localBounds = c.sharedMesh.bounds;
			}
			else // Non skinned mesh
			{
				var c = Go.AddComponent<MeshRenderer>();
				renderer = c;
				var meshFilter = Go.AddComponent<MeshFilter>();
				meshInstanceComponent.OwnedUnityComponent = c;

				meshFilter.sharedMesh = mesh;
			}

			var materials = new UnityEngine.Material[mesh.subMeshCount];
			meshInstanceComponent.Materials = new List<MTF.Material>(new MTF.Material[mesh.subMeshCount]);
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
						materials[i] = mtfMaterial?.ConvertedMaterial;
					}
				}
				catch(Exception)
				{
					Debug.LogWarning("Material Import Error, Skipping.");
				}
			}
			renderer.sharedMaterials = materials;
		}
	}

	public class STFMeshInstanceAddonApplier : ISTFNodeComponentAddonApplier
	{
		public override void Apply(ISTFAddonApplierContext Context, GameObject Target, Component SourceComponent)
		{
			base.Apply(Context, Target, SourceComponent);
			var meshInstance = Target.GetComponent<STFMeshInstance>();
			var smr = SourceComponent as SkinnedMeshRenderer;

			var armatureInstanceNode = Context.Root.GetComponentsInChildren<STFArmatureInstanceNode>().FirstOrDefault(c => c.Id == meshInstance?.ArmatureInstanceId);
			if(armatureInstanceNode != null)
			{
				Debug.Log("Setting up Armature");

				smr.sharedMesh.bindposes = armatureInstanceNode.armature.Bindposes;

				smr.rootBone = armatureInstanceNode.root.transform;
				smr.bones = armatureInstanceNode.bones.Select(b => b.transform).ToArray();
			}
			else
			{
				throw new Exception("Invalid armature instance");
			}
		}
	}


	public class STFMeshInstanceApplicationConverter : ISTFNodeComponentApplicationConverter
	{
		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
		}

		public string ConvertPropertyPath(ISTFApplicationConvertState State, Component Component, string STFProperty)
		{
			// Significantly unfuck this	
			if(STFProperty.StartsWith("material"))
			{
				var matIdx = int.Parse(STFProperty.Split(':')[1].Split('.')[0]);
				var meshInstance = Component.gameObject.GetComponent<STFMeshInstance>();
				return State.ConverterContext.Resource[typeof(MTF.Material)].ConvertPropertyPath(State, meshInstance.Materials[matIdx], STFProperty.Substring(STFProperty.IndexOf('.') + 1));
			}
			else
			{
				return STFProperty;
			}
		}

		public void ConvertResources(ISTFApplicationConvertState State, Component Component)
		{
			// Maybe add the material ??
		}
	}
}

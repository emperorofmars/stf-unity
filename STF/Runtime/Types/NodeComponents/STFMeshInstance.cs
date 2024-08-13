
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STF.Addon;
using STF.ApplicationConversion;
using STF.Serialisation;
using STF.Util;
using UnityEngine;

namespace STF.Types
{
	public class STFMeshInstance : ISTFNodeComponent
	{
		public const string _TYPE = "STF.mesh_instance";
		public override string Type => _TYPE;
		public NodeReference<STFArmatureInstanceNode> ArmatureInstance = new();
		public List<ResourceReference<MTFMaterial>> Materials = new();
	}

	public class STFMeshInstanceExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty)
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
					// handle material index somehow at some point despite unity
					return "material." + State.Context.ResourceExporters[typeof(Material)].ConvertPropertyPath(State, ((Renderer)Component).sharedMaterial, materialProperty);
				}
				else if(Component is STFMeshInstance)
				{
					return "material." + State.Context.ResourceExporters[typeof(MTF.Material)].ConvertPropertyPath(State, ((STFMeshInstance)Component).Materials[0], materialProperty);
				}
			}
			throw new Exception("Unrecognized animation property: " + UnityProperty);
		}

		public override (string, JObject) SerializeToJson(STFExportState State, Component Component)
		{
			Renderer renderer = Component as Renderer;
			Mesh mesh;
			if(Component is SkinnedMeshRenderer) mesh = (renderer as SkinnedMeshRenderer).sharedMesh;
			else mesh = (renderer as MeshRenderer).gameObject.GetComponent<MeshFilter>().sharedMesh;

			var ret = new JObject {
				{"type", STFMeshInstance._TYPE},
			};
			var rf = new RefSerializer(ret);
			
			ret.Add("mesh", rf.ResourceRef(ExportUtil.SerializeResource(State, mesh)));

			var meshInstance = Component.gameObject.GetComponent<STFMeshInstance>();
			SerializeRelationships(meshInstance, ret);

			if(Component is SkinnedMeshRenderer)
			{
				if(meshInstance.ArmatureInstance.IsValid()) ret.Add("armature_instance", rf.NodeRef(meshInstance.ArmatureInstance.Id));
				else ret.Add("armature_instance", rf.NodeRef((renderer as SkinnedMeshRenderer).rootBone.parent.GetComponent<STFArmatureInstanceNode>()?.Id));
			}

			var materials = new JArray();
			for(int matIdx = 0; matIdx < renderer.sharedMaterials.Length; matIdx++)
			{
				if(meshInstance.Materials.Count > matIdx && meshInstance.Materials[matIdx] != null)
				{
					materials.Add(rf.ResourceRef(ExportUtil.SerializeResource(State, meshInstance.Materials[matIdx])));
				}
				else if(renderer.sharedMaterials[matIdx] != null)
				{
					materials.Add(rf.ResourceRef(ExportUtil.SerializeResource(State, renderer.sharedMaterials[matIdx])));
				}
				else
				{
					materials.Add(null);
				}
			}
			ret.Add("materials", materials);
			ret.Add("morphtarget_values", new JArray(Enumerable.Range(0, mesh.blendShapeCount).Select(i => Component is SkinnedMeshRenderer ? (renderer as SkinnedMeshRenderer).GetBlendShapeWeight(i) : 0)));

			return (meshInstance.Id, ret);
		}
	}

	public class STFMeshInstanceImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty)
		{
			if(STFProperty.StartsWith("blendshape"))
			{
				return "blendShape." + STFProperty.Split('.')[1];
			}
			else if(STFProperty.StartsWith("material"))
			{
				var matIdx = int.Parse(STFProperty.Split(':')[1].Split('.')[0]);

				return "material." + State.Context.ResourceImporters[MTFMaterial._TYPE].ConvertPropertyPath(State, ((STFMeshInstance)Component).Materials[matIdx], STFProperty.Substring(STFProperty.IndexOf('.') + 1));
			}
			throw new Exception("Unrecognized animation property: " + STFProperty);
		}

		public override void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go)
		{
			var rf = new RefDeserializer(Json);

			var meta = (STFMesh)State.Resources[rf.ResourceRef(Json["mesh"])];
			var meshInstanceComponent = Go.AddComponent<STFMeshInstance>();
			meshInstanceComponent.Id = Id;
			ParseRelationships(Json, meshInstanceComponent);
			State.AddNodeComponent(meshInstanceComponent);

			Mesh mesh = (Mesh)meta.Resource;
			Renderer renderer;

			State.AddTask(new Task(() => {
				if(!string.IsNullOrWhiteSpace(meta.Armature.Id))
				{
					var c = Go.AddComponent<SkinnedMeshRenderer>();
					renderer = c;
					meshInstanceComponent.OwnedUnityComponent = c;

					c.sharedMesh = mesh;

					if(Json.ContainsKey("armature_instance"))
					{
						var armatureInstanceId = rf.NodeRef(Json["armature_instance"]);
						if(State.Nodes.ContainsKey(armatureInstanceId))
						{
							var armatureInstanceNode = State.Nodes[armatureInstanceId];
							var armatureInstance = armatureInstanceNode.GetComponent<STFArmatureInstanceNode>();
							meshInstanceComponent.ArmatureInstance = armatureInstance;
							c.rootBone = armatureInstance.Root.transform;
							c.bones = armatureInstance.Bones.Select(b => b.transform).ToArray();
							c.updateWhenOffscreen = true;
						}
						else
						{
							meshInstanceComponent.ArmatureInstance = armatureInstanceId;
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

				var materials = new Material[mesh.subMeshCount];
				meshInstanceComponent.Materials = new List<ResourceReference<MTFMaterial>>(new ResourceReference<MTFMaterial>[mesh.subMeshCount]);
				for(int i = 0; i < materials.Length; i++)
				{
					//try{
						if(Json["materials"].Count() > i && Json["materials"][i] != null && Json["materials"][i].Type != JTokenType.Null && State.Resources.ContainsKey(rf.ResourceRef(Json["materials"][i])))
						{
							var mtfMaterial = (MTFMaterial)State.Resources[rf.ResourceRef(Json["materials"][i])];
							meshInstanceComponent.Materials[i] = mtfMaterial;
							materials[i] = (mtfMaterial.Resource as MTF.Material)?.ConvertedMaterial;
						}
						else
						{
							Debug.LogWarning("Material Import Error, Falling back to default material.");
							var mtfMaterial = MTF.Material.CreateDefaultMaterial();
							var stfMtfMaterial = ScriptableObject.CreateInstance<MTFMaterial>();
							stfMtfMaterial.Resource = mtfMaterial;
							meshInstanceComponent.Materials[i] = stfMtfMaterial;
							materials[i] = mtfMaterial.ConvertedMaterial;
						}
					/*}
					catch(Exception e)
					{
						Debug.LogWarning("Material Import Error, Skipping.", e);
					}*/
				}
				renderer.sharedMaterials = materials;
			}));
		}
	}

	public class STFMeshInstanceAddonApplier : ISTFNodeComponentAddonApplier
	{
		public override void Apply(ISTFAddonApplierContext Context, GameObject Target, Component SourceComponent)
		{
			base.Apply(Context, Target, SourceComponent);
			var meshInstance = SourceComponent.GetComponent<STFMeshInstance>();
			var smr = SourceComponent as SkinnedMeshRenderer;

			var armatureInstanceNode = Context.Root.GetComponentsInChildren<STFArmatureInstanceNode>().FirstOrDefault(c => c.Id == meshInstance.ArmatureInstance.Id);
			if(armatureInstanceNode != null)
			{
				smr.sharedMesh.bindposes = armatureInstanceNode.Armature.GetRef<STFArmature>().Bindposes;

				smr.rootBone = armatureInstanceNode.Root.transform;
				smr.bones = armatureInstanceNode.Bones.Select(b => b.transform).ToArray();
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

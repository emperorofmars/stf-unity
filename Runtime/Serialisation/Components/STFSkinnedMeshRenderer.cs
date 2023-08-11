using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;

namespace stf.Components
{
	public class STFMeshInstanceImporter : ASTFComponentImporter
	{
		public static string _TYPE = "STF.mesh_instance";
		override public void ParseFromJson(ISTFImporter state, ISTFAsset asset, JToken json, string id, GameObject go)
		{
			var c = go.AddComponent<SkinnedMeshRenderer>();
			var uuidComponent = go.GetComponent<STFUUID>();
			uuidComponent.componentIds.Add(new STFUUID.ComponentIdMapping{component = c, id = id});
			state.AddComponent(id, c);

			var resource = state.GetResource((string)json["mesh"]);
			if(resource.GetType() == typeof(Mesh))
			{
				c.sharedMesh = (Mesh)resource;
			}
			else if(resource.GetType() == typeof(GameObject))
			{
				var renderer = ((GameObject)resource).GetComponent<SkinnedMeshRenderer>();
				c.sharedMesh = renderer.sharedMesh;
			}

			if((string)json["armature_instance"] != null)
			{
				var armatureInstanceNode = state.GetNode((string)json["armature_instance"]);

				if(armatureInstanceNode != null && asset.isNodeInAsset((string)json["armature_instance"]))
				{
					var armatureInstance = armatureInstanceNode.GetComponent<STFArmatureInstance>();
					c.rootBone = armatureInstance.root.transform;
					c.bones = armatureInstance.bones.Select(b => b.transform).ToArray();
					c.updateWhenOffscreen = true;
				}
				else
				{
					var addonDef = go.AddComponent<STFSkinnedMeshRendererAddon>();
					addonDef.ArmatureInstanceId = (string)json["armature_instance"];
				}
			}

			var materials = new Material[c.sharedMesh.subMeshCount];
			for(int i = 0; i < materials.Length; i++)
			{
				if(json["materials"][i] != null) materials[i] = (Material)state.GetResource((string)json["materials"][i]);
			}
			if(c.sharedMesh.blendShapeCount > 0 && json["morphtarget_values"] != null)
			{
				for(int i = 0; i < c.sharedMesh.blendShapeCount; i++)
				{
					c.SetBlendShapeWeight(i, (float)json["morphtarget_values"][i]);
				}

			}
			c.sharedMaterials = materials;
			c.localBounds = c.sharedMesh.bounds;
		}
	}

	public class STFMeshInstanceExporter : ASTFComponentExporter
	{

		override public List<KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>> GatherResources(Component component)
		{
			SkinnedMeshRenderer c = (SkinnedMeshRenderer)component;
			var ret = new List<KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>>();
			if(c.GetComponent<STFSkinnedMeshRendererAddon>() == null)
			{
				ret.Add(new KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>> (c.rootBone.parent.GetComponent<STFArmatureInstance>().armature, null));
				ret.Add(new KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>> (c.sharedMesh, new Dictionary<string, object>{{"armature", c.rootBone.parent.GetComponent<STFArmatureInstance>().armature}}));
			}
			else
			{
				ret.Add(new KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>> (c.sharedMesh, new Dictionary<string, object>{{"armature_id", c.GetComponent<STFSkinnedMeshRendererAddon>().ArmatureInstanceId}}));
			}
			foreach(var material in c.sharedMaterials) if(material != null) ret.Add(new KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>> (material, null));
			return ret;
		}

		override public JToken SerializeToJson(ISTFExporter state, Component component)
		{
			SkinnedMeshRenderer c = (SkinnedMeshRenderer)component;
			var ret = new JObject();
			ret.Add("type", "STF.mesh_instance");
			ret.Add("mesh", state.GetResourceId(c.sharedMesh));

			var smrAddon = c.gameObject.GetComponent<STFSkinnedMeshRendererAddon>();
			if(smrAddon) ret.Add("armature_instance", smrAddon.ArmatureInstanceId);
			else ret.Add("armature_instance", state.GetNodeId(c.rootBone.parent.gameObject));

			ret.Add("materials", new JArray(c.sharedMaterials.Select(m => m != null ? state.GetResourceId(m) : null)));
			ret.Add("morphtarget_values", new JArray(Enumerable.Range(0, c.sharedMesh.blendShapeCount).Select(i => c.GetBlendShapeWeight(i))));
			
			ret.Add("resources_used", new JArray(state.GetResourceId(c.sharedMesh), ret["armature_instance"]));
			((JArray)ret["resources_used"]).Merge(ret["morphtarget_values"]);
			return ret;
		}
	}

	public class STFMeshInstanceAddonApplier : ISTFAddonTrigger
	{
		public void Apply(Component triggerComponent, GameObject root)
		{
			GameObject go = triggerComponent.gameObject;
			var smr = go.GetComponent<SkinnedMeshRenderer>();
			var addonDef = (STFSkinnedMeshRendererAddon)triggerComponent;
			
			var armatureInstanceNode = root.GetComponentsInChildren<STFUUID>().FirstOrDefault(c => c.id == addonDef.ArmatureInstanceId);
			if(armatureInstanceNode != null)
			{
				var armatureInstance = armatureInstanceNode.GetComponent<STFArmatureInstance>();
				smr.sharedMesh.bindposes = armatureInstance.armature.bindposes;

				smr.rootBone = armatureInstance.root.transform;
				smr.bones = armatureInstance.bones.Select(b => b.transform).ToArray();
				smr.updateWhenOffscreen = true;
			}
			else
			{
				throw new Exception("Invalid armature instance");
			}
		}
	}

	public class SkinnedMeshRendererAnimationPathTranslator : ISTFAnimationPathTranslator
	{
		public string ToSTF(string property)
		{
			if(property.StartsWith("blendShape"))
			{
				return "blendshape." + property.Split('.')[1];
			}
			throw new Exception("Unrecognized animation property: " + property);
		}

		public string ToUnity(string property)
		{
			if(property.StartsWith("blendshape"))
			{
				return "blendShape." + property.Split('.')[1];
			}
			throw new Exception("Unrecognized animation property: " + property);
		}
	}
}

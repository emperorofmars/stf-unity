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
		override public void parseFromJson(ISTFImporter state, JToken json, string id, GameObject go)
		{
			var c = go.AddComponent<SkinnedMeshRenderer>();
			var uuidComponent = go.GetComponent<STFUUID>();
			uuidComponent.componentIds.Add(c, id);
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
				var armatureInstanceId = (string)json["armature_instance"];
				var armatureInstanceNode = state.GetNode(armatureInstanceId);
				var armatureInstance = armatureInstanceNode.GetComponent<STFArmatureInstance>();
				c.rootBone = armatureInstance.root.transform;
				c.bones = armatureInstance.bones.Select(b => b.transform).ToArray();
			}

			var materials = new Material[c.sharedMesh.subMeshCount];
			for(int i = 0; i < materials.Length; i++)
			{
				if(json["materials"][i] != null) materials[i] = (Material)state.GetResource((string)json["materials"][i]);
			}
			c.sharedMaterials = materials;
			c.localBounds = c.sharedMesh.bounds;
		}
	}

	public class STFMeshInstanceExporter : ASTFComponentExporter
	{

		override public List<UnityEngine.Object> gatherResources(Component component)
		{
			SkinnedMeshRenderer c = (SkinnedMeshRenderer)component;
			var ret = new List<UnityEngine.Object>();
			ret.Add(c.sharedMesh);
			foreach(var material in c.sharedMaterials) if(material != null) ret.Add(material);
			return ret;
		}

		override public JToken serializeToJson(ISTFExporter state, Component component)
		{
			SkinnedMeshRenderer c = (SkinnedMeshRenderer)component;
			var ret = new JObject();
			ret.Add("type", "STF.mesh_instance");
			ret.Add("mesh", state.GetResourceId(c.sharedMesh));
			ret.Add("armature_instance", state.GetNodeId(c.rootBone.parent.gameObject));
			ret.Add("materials", new JArray(c.sharedMaterials.Select(m => m != null ? state.GetResourceId(m) : null)));
			return ret;
		}
	}
}

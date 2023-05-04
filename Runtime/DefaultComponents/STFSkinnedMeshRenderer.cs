using System;
using System.Collections.Generic;
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

			var skeleton = json["skeleton"].ToObject<List<string>>();
			var bones = new Transform[skeleton.Count];
			for(int i = 0; i < skeleton.Count; i++)
			{
				var node = state.GetNode(skeleton[i]);
				bones[i] = node.transform;
			}
			c.bones = bones;
			c.rootBone = state.GetNode((string)json["skeleton_root"]).transform;

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

			c.materials = new Material[c.sharedMesh.subMeshCount];
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
			return ret;
		}

		override public JToken serializeToJson(ISTFExporter state, Component component)
		{
			SkinnedMeshRenderer c = (SkinnedMeshRenderer)component;
			var ret = new JObject();
			ret.Add("type", "STF.mesh_instance");
			ret.Add("mesh", state.GetResourceId(c.sharedMesh));

			var skeleton = new JArray();
			foreach(var bone in c.bones)
			{
				skeleton.Add(state.GetNodeId(bone.gameObject));
			}
			ret.Add("skeleton", skeleton);
			ret.Add("skeleton_root", state.GetNodeId(c.rootBone.gameObject));
			return ret;
		}
	}
}

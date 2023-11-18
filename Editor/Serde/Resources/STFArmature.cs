

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using UnityEditor;
using UnityEngine;

namespace STF.Serde
{
	public class STFArmature : MonoBehaviour
	{
		public string armatureId;
		public string armatureName;
		public Transform root;
		public List<Transform> bones = new List<Transform>();
		public Matrix4x4[] bindposes;
	}

	public class STFArmatureExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public List<string> GatherUsedBuffers(UnityEngine.Object Resource)
		{
			throw new NotImplementedException();
		}

		public List<GameObject> GatherUsedNodes(UnityEngine.Object Resource)
		{
			throw new NotImplementedException();
		}

		public List<UnityEngine.Object> GatherUsedResources(UnityEngine.Object Resource)
		{
			throw new NotImplementedException();
		}

		public JToken SerializeToJson(STFExportState state, UnityEngine.Object resource)
		{
			var armature = (STFArmature)resource;
			
			var ret = new JObject();
			ret.Add("type", STFArmatureImporter._TYPE);
			ret.Add("name", armature.armatureName);
			ret.Add("root", armature.root.GetComponent<STFNode>().NodeId);

			var boneIds = new List<string>();
			foreach(var bone in armature.bones)
			{
				var boneJson = new JObject();
				boneJson.Add("name", bone.name);
				boneJson.Add("type", "bone");

				boneJson.Add("trs", new JArray() {
					new JArray() {bone.localPosition.x, bone.localPosition.y, bone.localPosition.z},
					new JArray() {bone.localRotation.x, bone.localRotation.y, bone.localRotation.z, bone.localRotation.w},
					new JArray() {bone.localScale.x, bone.localScale.y, bone.localScale.z}
				});
				var childIds = new String[bone.childCount];
				for(int childIdx = 0; childIdx < bone.childCount; childIdx++)
				{
					childIds[childIdx] = bone.GetChild(childIdx).GetComponent<STFNode>().NodeId;
				}
				boneJson.Add("children", new JArray(childIds));

				var boneId = bone.GetComponent<STFNode>().NodeId;
				boneIds.Add(boneId);
				state.AddNode(bone.gameObject, boneJson, boneId);
			}
			ret.Add("bones", new JArray(boneIds));
			state.AddResource(armature, ret, armature.armatureId);
			return ret;
		}
	}

	public class STFArmatureImporter : ISTFResourceImporter
	{
		public static string _TYPE = "STF.armature";

		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public UnityEngine.Object ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var go = new GameObject();
			State.AddTrash(go);
			var armature = go.AddComponent<STFArmature>();
			
			armature.armatureId = Id;
			armature.name = (string)Json["name"];
			armature.armatureName = (string)Json["name"];

			if(Json["trs"] != null)
			{
				armature.transform.localPosition = new Vector3((float)Json["trs"][0][0], (float)Json["trs"][0][1], (float)Json["trs"][0][2]);
				armature.transform.localRotation = new Quaternion((float)Json["trs"][1][0], (float)Json["trs"][1][1], (float)Json["trs"][1][2], (float)Json["trs"][1][3]);
				armature.transform.localScale = new Vector3((float)Json["trs"][2][0], (float)Json["trs"][2][1], (float)Json["trs"][2][2]);
			}

			var boneIds = Json["bones"].ToObject<List<string>>();

			var tasks = new List<Task>();
			for(int i = 0; i < boneIds.Count; i++)
			{
				var boneNodeJson = State.JsonRoot["nodes"][boneIds[i]];
				var boneGO = new GameObject();
				State.AddTrash(boneGO);
				var bone = boneGO.AddComponent<STFNode>();
				
				bone.NodeId = boneIds[i];
				bone.name = (string)boneNodeJson["name"];
				bone.Origin = Id;

				bone.transform.localPosition = new Vector3((float)boneNodeJson["trs"][0][0], (float)boneNodeJson["trs"][0][1], (float)boneNodeJson["trs"][0][2]);
				bone.transform.localRotation = new Quaternion((float)boneNodeJson["trs"][1][0], (float)boneNodeJson["trs"][1][1], (float)boneNodeJson["trs"][1][2], (float)boneNodeJson["trs"][1][3]);
				bone.transform.localScale = new Vector3((float)boneNodeJson["trs"][2][0], (float)boneNodeJson["trs"][2][1], (float)boneNodeJson["trs"][2][2]);

				armature.bones.Add(bone.transform);
				
				foreach(string childId in boneNodeJson["children"]?.ToObject<List<string>>())
				{
					tasks.Add(new Task(() => {
						var childBone = armature.bones.Find(b => b.GetComponent<STFNode>().NodeId == childId);
						childBone.SetParent(bone.transform, false);
					}));
				}
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
				if(task.Exception != null) throw task.Exception;
			}
			var root = armature.bones.Find(b => b.GetComponent<STFNode>().NodeId == (string)Json["root"]);
			root.SetParent(armature.transform, false);
			armature.root = root;

			armature.bindposes = new Matrix4x4[boneIds.Count];
			for(int i = 0; i < boneIds.Count; i++)
			{
				armature.bindposes[i] = armature.bones[i].worldToLocalMatrix;
			}

			var ResourceLocation = Path.Combine(State.GetResourceLocation(), armature.armatureName + "_" + armature.armatureId + ".Prefab");
			PrefabUtility.SaveAsPrefabAsset(go, ResourceLocation);
			State.AddResource(armature, armature.armatureId);

			return armature;
		}
	}
}

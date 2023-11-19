
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using STF.Util;
using UnityEditor;
using UnityEngine;

namespace STF.Serde
{
	public class STFArmature : ASTFResource
	{
		public Matrix4x4[] Bindposes;
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
			/*var armature = (STFArmatureNodeInfo)resource;
			
			var ret = new JObject();
			ret.Add("type", STFArmatureImporter._TYPE);
			ret.Add("name", armature.ArmatureName);
			ret.Add("root", armature.Root.GetComponent<STFNode>().NodeId);

			var boneIds = new List<string>();
			foreach(var bone in armature.Bones)
			{
				var boneJson = new JObject();
				boneJson.Add("name", bone.name);
				boneJson.Add("type", "bone");

				boneJson.Add("trs", new JArray() {
					new JArray() {bone.transform.localPosition.x, bone.transform.localPosition.y, bone.transform.localPosition.z},
					new JArray() {bone.transform.localRotation.x, bone.transform.localRotation.y, bone.transform.localRotation.z, bone.transform.localRotation.w},
					new JArray() {bone.transform.localScale.x, bone.transform.localScale.y, bone.transform.localScale.z}
				});

				var childIds = new String[bone.transform.childCount];
				for(int childIdx = 0; childIdx < bone.transform.childCount; childIdx++)
				{
					childIds[childIdx] = bone.transform.GetChild(childIdx).GetComponent<STFNode>().NodeId;
				}
				boneJson.Add("children", new JArray(childIds));

				var boneId = bone.GetComponent<STFNode>().NodeId;
				boneIds.Add(boneId);
				state.AddNode(bone.gameObject, boneJson, boneId);
			}
			ret.Add("bones", new JArray(boneIds));
			ret.Add("used_nodes", new JArray(boneIds));
			state.AddResource(armature, ret, armature.ArmatureId);
			return ret;*/

			return null;
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
			var armature = go.AddComponent<STFArmatureNodeInfo>();
			
			armature.ArmatureId = Id;
			armature.name = (string)Json["name"];
			armature.ArmatureName = (string)Json["name"];

			if(Json["trs"] != null) TRSUtil.ParseTRS(armature.gameObject, Json);

			var boneIds = Json["bones"].ToObject<List<string>>();

			var tasks = new List<Task>();
			for(int i = 0; i < boneIds.Count; i++)
			{
				var boneNodeJson = (JObject)State.JsonRoot["nodes"][boneIds[i]];
				var boneGO = new GameObject();
				State.AddTrash(boneGO);

				var bone = boneGO.AddComponent<STFNode>();
				bone.Type = "STF.bone";
				bone.NodeId = boneIds[i];
				bone.name = (string)boneNodeJson["name"];
				bone.Origin = Id;
				TRSUtil.ParseTRS(bone.gameObject, boneNodeJson);

				armature.Bones.Add(bone.gameObject);
				
				foreach(string childId in boneNodeJson["children"]?.ToObject<List<string>>())
				{
					tasks.Add(new Task(() => {
						var childBone = armature.Bones.Find(b => b.GetComponent<STFNode>().NodeId == childId);
						childBone.transform.SetParent(bone.transform, false);
					}));
				}
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
				if(task.Exception != null) throw task.Exception;
			}
			var root = armature.Bones.Find(b => b.GetComponent<STFNode>().NodeId == (string)Json["root"]);
			root.transform.SetParent(armature.transform, false);
			armature.Root = root;

			var bindposes = new Matrix4x4[boneIds.Count];
			for(int i = 0; i < boneIds.Count; i++)
			{
				bindposes[i] = armature.Bones[i].transform.worldToLocalMatrix;
			}

			var ResourceLocation = Path.Combine(State.GetResourceLocation(), armature.ArmatureName + "_" + armature.ArmatureId + ".Prefab");
			var saved = PrefabUtility.SaveAsPrefabAsset(go, ResourceLocation);

			var ret = ScriptableObject.CreateInstance<STFArmature>();
			ret.Id = Id;
			ret.Name = armature.name;
			ret.ResourceLocation = ResourceLocation;
			ret.Resource = saved;
			ret.Bindposes = bindposes;
			
			State.AddResource(ret, armature.ArmatureId);
			AssetDatabase.CreateAsset(ret, Path.ChangeExtension(ret.ResourceLocation, "Asset"));
			AssetDatabase.Refresh();

			return ret;
		}
	}
}

#endif

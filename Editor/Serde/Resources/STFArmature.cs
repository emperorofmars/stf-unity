
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

		public string SerializeToJson(ISTFExportState state, UnityEngine.Object resource)
		{
			var meta = (STFArmature)resource;
			var armatureGo = (GameObject)meta.Resource;
			var armature = armatureGo.GetComponent<STFArmatureNodeInfo>();
			
			var ret = new JObject();
			ret.Add("type", STFArmatureImporter._TYPE);
			ret.Add("name", armature.ArmatureName);
			ret.Add("root", armature.Root.GetComponent<STFBoneNode>().NodeId);

			var boneIds = new List<string>();
			foreach(var bone in armature.Bones)
			{
				var boneJson = new JObject
				{
					{"name", bone.name},
					{"type", STFBoneNode._TYPE},
					{"trs", TRSUtil.SerializeTRS(bone)},
				};

				var childIds = new string[bone.transform.childCount];
				for(int childIdx = 0; childIdx < bone.transform.childCount; childIdx++)
				{
					childIds[childIdx] = bone.transform.GetChild(childIdx).GetComponent<STFBoneNode>().NodeId;
				}
				boneJson.Add("children", new JArray(childIds));

				var boneId = bone.GetComponent<STFBoneNode>().NodeId;
				boneIds.Add(boneId);
				state.AddNode(bone.gameObject, boneJson, boneId);
			}
			ret.Add("bones", new JArray(boneIds));
			
			ret.Add("used_nodes", new JArray(boneIds));
			return state.AddResource(armature, ret, armature.ArmatureId);
		}
	}

	public class STFArmatureImporter : ISTFResourceImporter
	{
		public static string _TYPE = "STF.armature";

		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var go = new GameObject();
			State.AddTrash(go);
			var armatureInfo = go.AddComponent<STFArmatureNodeInfo>();
			
			var meta = ScriptableObject.CreateInstance<STFArmature>();
			meta.Id = Id;
			meta.Name = (string)Json["name"];
			go.name = meta.Name;

			armatureInfo.ArmatureId = Id;
			armatureInfo.ArmatureName = meta.Name;

			if(Json["trs"] != null) TRSUtil.ParseTRS(go, Json);

			var boneIds = Json["bones"].ToObject<List<string>>();

			var tasks = new List<Task>();
			for(int i = 0; i < boneIds.Count; i++)
			{
				var boneNodeJson = (JObject)State.JsonRoot["nodes"][boneIds[i]];
				var boneGO = new GameObject();
				State.AddTrash(boneGO);

				var bone = boneGO.AddComponent<STFBoneNode>();
				bone.NodeId = boneIds[i];
				bone.name = (string)boneNodeJson["name"];
				bone.Origin = Id;
				TRSUtil.ParseTRS(bone.gameObject, boneNodeJson);

				armatureInfo.Bones.Add(bone.gameObject);
				
				foreach(string childId in boneNodeJson["children"]?.ToObject<List<string>>())
				{
					tasks.Add(new Task(() => {
						var childBone = armatureInfo.Bones.Find(b => b.GetComponent<STFBoneNode>().NodeId == childId);
						childBone.transform.SetParent(bone.transform, false);
					}));
				}
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
				if(task.Exception != null) throw task.Exception;
			}
			var root = armatureInfo.Bones.Find(b => b.GetComponent<STFBoneNode>().NodeId == (string)Json["root"]);
			root.transform.SetParent(go.transform, false);
			armatureInfo.Root = root;

			var bindposes = new Matrix4x4[boneIds.Count];
			for(int i = 0; i < boneIds.Count; i++)
			{
				bindposes[i] = armatureInfo.Bones[i].transform.worldToLocalMatrix;
			}
			meta.Bindposes = bindposes;

			State.SaveResource(go, meta, Id);
			return;
		}
	}
}

#endif

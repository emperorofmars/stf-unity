
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFArmature : ISTFResource
	{
		public Matrix4x4[] Bindposes;
	}

	public class STFArmatureExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(STFExportState State, UnityEngine.Object Resource, string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(STFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			var meta = (STFArmature)Resource;
			var armatureGo = (GameObject)meta.Resource;
			var armature = armatureGo.GetComponent<STFArmatureNodeInfo>();
			
			var ret = new JObject {
				{"type", STFArmatureImporter._TYPE},
				{"name", armature.ArmatureName},
			};

			var rf = new RefSerializer(ret);
			ret.Add("root", rf.NodeRef(armature.Root.GetComponent<STFBoneNode>().Id));

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
					childIds[childIdx] = bone.transform.GetChild(childIdx).GetComponent<STFBoneNode>().Id;
				}
				boneJson.Add("children", new JArray(childIds));

				var boneId = bone.GetComponent<STFBoneNode>().Id;
				boneIds.Add(boneId);
				State.AddNode(bone.gameObject, boneJson, boneId);
				rf.NodeRef(boneId);
			}
			//ret.Add("bones", new JArray(boneIds));
			
			ret.Add("components", SerdeUtil.SerializeResourceComponents(State, meta));
			//ret.Add("used_nodes", new JArray(boneIds));
			return State.AddResource(armature, ret, armature.ArmatureId);
		}
	}

	public class STFArmatureImporter : ISTFResourceImporter
	{
		public const string _TYPE = "STF.armature";

		public string ConvertPropertyPath(STFImportState State, UnityEngine.Object Resource, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(STFImportState State, JObject Json, string Id)
		{
			var go = new GameObject();
			State.AddTrash(go);
			var armatureInfo = go.AddComponent<STFArmatureNodeInfo>();
			
			var meta = ScriptableObject.CreateInstance<STFArmature>();
			meta.Id = Id;
			meta.Name = (string)Json["name"];
			meta.name = meta.Name;
			go.name = meta.Name;

			armatureInfo.ArmatureId = Id;
			armatureInfo.ArmatureName = meta.Name;

			if(Json["trs"] != null) TRSUtil.ParseTRS(go, Json);

			var rf = new RefDeserializer(Json);
			var boneIds = rf.NodeRefs();

			var tasks = new List<Task>();
			for(int i = 0; i < boneIds.Count; i++)
			{
				var boneNodeJson = (JObject)State.JsonRoot["nodes"][boneIds[i]];
				var boneGO = new GameObject();
				State.AddTrash(boneGO);

				var bone = boneGO.AddComponent<STFBoneNode>();
				bone.Id = boneIds[i];
				bone.name = (string)boneNodeJson["name"];
				bone.Origin = Id;
				TRSUtil.ParseTRS(bone.gameObject, boneNodeJson);

				armatureInfo.Bones.Add(bone.gameObject);
				
				foreach(string childId in boneNodeJson["children"]?.ToObject<List<string>>())
				{
					tasks.Add(new Task(() => {
						var childBone = armatureInfo.Bones.Find(b => b.GetComponent<STFBoneNode>().Id == childId);
						childBone.transform.SetParent(bone.transform, false);
					}));
				}
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
				if(task.Exception != null) throw task.Exception;
			}
			var root = armatureInfo.Bones.Find(b => b.GetComponent<STFBoneNode>().Id == rf.NodeRef(Json["root"]));
			root.transform.SetParent(go.transform, false);
			armatureInfo.Root = root;

			var bindposes = new Matrix4x4[boneIds.Count];
			for(int i = 0; i < boneIds.Count; i++)
			{
				bindposes[i] = armatureInfo.Bones[i].transform.worldToLocalMatrix;
			}
			meta.Bindposes = bindposes;

			State.UnityContext.SaveResource(go, meta, Id);
			SerdeUtil.ParseResourceComponents(State, meta, Json);
			return;
		}
	}
}

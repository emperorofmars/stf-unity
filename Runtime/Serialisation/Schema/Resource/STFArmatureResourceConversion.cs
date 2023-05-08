
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace stf.serialisation
{
	public class STFArmatureResourceExporter
	{
		public string id;
		public string rootBoneId;
		public List<string> boneIds = new List<string>();
		//public List<Mesh> meshes = new List<Mesh>();

		public void SetupFromSkinnedMeshRenderer(ISTFExporter state, SkinnedMeshRenderer smr)
		{
			var rootBone = smr.rootBone;
			var bones = smr.bones;
			var bindposes = smr.sharedMesh.bindposes;

			// get armature from stored resource
			if(smr.rootBone.parent != null && smr.rootBone.parent.GetComponent<STFArmatureInstance>() != null)
			{
				Debug.Log("PARSING FROM RESOURCE");
				var armatureInstance = smr.rootBone.parent.GetComponent<STFArmatureInstance>();
				rootBoneId = armatureInstance.armature.rootId;
				this.id = armatureInstance.armature.id;
				
				var armatureJson = new JObject();
				armatureJson.Add("type", STFArmatureImporter._TYPE);
				if(rootBone.parent != null)
				{
					armatureJson.Add("name", rootBone.parent.name);
					if(armatureInstance.armature.armaturePosition != null && armatureInstance.armature.armatureRotation != null && armatureInstance.armature.armatureScale != null)
					{
						armatureJson.Add("trs", new JArray() {
							new JArray() {armatureInstance.armature.armaturePosition.x, armatureInstance.armature.armaturePosition.y, armatureInstance.armature.armaturePosition.z},
							new JArray() {armatureInstance.armature.armatureRotation.x, armatureInstance.armature.armatureRotation.y, armatureInstance.armature.armatureRotation.z, armatureInstance.armature.armatureRotation.w},
							new JArray() {armatureInstance.armature.armatureScale.x, armatureInstance.armature.armatureScale.y, armatureInstance.armature.armatureScale.z}
						});
					}
				}
				foreach(var bone in armatureInstance.armature.bones)
				{
					var boneJson = new JObject();
					boneJson.Add("name", bone.name);
					boneJson.Add("type", "bone");

					boneJson.Add("trs", new JArray() {
						new JArray() {bone.localPosition.x, bone.localPosition.y, bone.localPosition.z},
						new JArray() {bone.localRotation.x, bone.localRotation.y, bone.localRotation.z, bone.localRotation.w},
						new JArray() {bone.localScale.x, bone.localScale.y, bone.localScale.z}
					});
					boneJson.Add("children", new JArray(bone.children));

					boneIds.Add(bone.id);
					state.RegisterNode(bone.id, boneJson);
				}
				armatureJson.Add("root", rootBoneId);
				armatureJson.Add("bones", new JArray(boneIds));
				state.RegisterResource(id, armatureJson);
			}
			else // get armature from skinned mesh renderer and bindposes
			{
				this.id = Guid.NewGuid().ToString();
				var boneNodes = new List<JObject>();
				for(int i = 0; i < bindposes.Length; i++)
				{
					var boneIdComponent = bones[i].GetComponent<STFUUID>();
					var boneId = boneIdComponent != null && boneIdComponent.boneId != null && boneIdComponent.boneId.Length > 0 ? boneIdComponent.boneId : Guid.NewGuid().ToString();
					var bone = new JObject();

					bone.Add("name", bones[i].name);
					bone.Add("type", "bone");

					// get this from the bindposes instead at some point
					bone.Add("trs", new JArray() {
						new JArray() {bones[i].localPosition.x, bones[i].localPosition.y, bones[i].localPosition.z},
						new JArray() {bones[i].localRotation.x, bones[i].localRotation.y, bones[i].localRotation.z, bones[i].localRotation.w},
						new JArray() {bones[i].localScale.x, bones[i].localScale.y, bones[i].localScale.z}
					});

					state.RegisterNode(boneId, bone);
					boneNodes.Add(bone);
					boneIds.Add(boneId);
					if(bones[i] == rootBone) rootBoneId = boneId;
				}
				for(int i = 0; i < bindposes.Length; i++)
				{
					var children = new List<string>();
					for(int childIdx = 0; childIdx < bones[i].childCount; childIdx++)
					{
						for(var boneIdx = 0; boneIdx < bones.Length; boneIdx++)
						{
							if(bones[boneIdx] == bones[i].GetChild(childIdx))
							{
								children.Add(boneIds[boneIdx]);
								break;
							}
						}
					}
					if(children.Count > 0) boneNodes[i].Add("children", new JArray(children));
				}

				var armatureJson = new JObject();
				armatureJson.Add("type", STFArmatureImporter._TYPE);
				if(rootBone.parent != null)
				{
					armatureJson.Add("name", rootBone.parent.name);
					armatureJson.Add("trs", new JArray() {
						new JArray() {rootBone.parent.localPosition.x, rootBone.parent.localPosition.y, rootBone.parent.localPosition.z},
						new JArray() {rootBone.parent.localRotation.x, rootBone.parent.localRotation.y, rootBone.parent.localRotation.z, rootBone.parent.localRotation.w},
						new JArray() {rootBone.parent.localScale.x, rootBone.parent.localScale.y, rootBone.parent.localScale.z}
					});
				}
				armatureJson.Add("root", rootBoneId);
				armatureJson.Add("bones", new JArray(boneIds));
				state.RegisterResource(id, armatureJson);
			}
		}
	}

	public class STFArmature : UnityEngine.Object
	{
		public string armatureName;
		public Transform root;
		public Transform[] bones;
		public Matrix4x4[] bindposes;
	}

	public class STFArmatureImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.armature";

		public override UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			var tasks = new List<Task>();
			var armatureTransforms = new STFArmature();
			armatureTransforms.armatureName = (string)json["name"];
			var rootId = (string)json["root"];
			var rootJson = (JObject)jsonRoot["nodes"][rootId];
			var boneIds = json["bones"].ToObject<List<string>>();
			armatureTransforms.bones = new Transform[boneIds.Count];
			armatureTransforms.bindposes = new Matrix4x4[boneIds.Count];

			// create STFArmatureResource and save it
			var armatureResource = ScriptableObject.CreateInstance<STFArmatureResource>();
			armatureResource.name = armatureTransforms.armatureName;
			armatureResource.armatureName = armatureTransforms.armatureName;
			armatureResource.rootId = rootId;
			armatureResource.id = id;
			//armatureResource.bones = new List<STFArmatureResource.Bone>(boneIds.Count);
			var boneResources = new STFArmatureResource.Bone[boneIds.Count];

			for(int i = 0; i < boneIds.Count; i++)
			{
				var boneNodeJson = jsonRoot["nodes"][boneIds[i]];
				var resourceBone = new STFArmatureResource.Bone();
				armatureTransforms.bones[i] = parseBoneFromJson(state, boneNodeJson, resourceBone, tasks).transform;
				var uuidComponent = armatureTransforms.bones[i].gameObject.AddComponent<STFUUID>();
				uuidComponent.id = boneIds[i];
				if(boneIds[i] == rootId)
				{
					armatureTransforms.root = armatureTransforms.bones[i];
				}
				state.AddNode(boneIds[i], armatureTransforms.bones[i].gameObject);
				state.AddTrashObject(armatureTransforms.bones[i].gameObject);
				
				resourceBone.id = boneIds[i];
				resourceBone.name = (string)boneNodeJson["name"];
				boneResources[i] = resourceBone;
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
				if(task.Exception != null) throw task.Exception;
			}
			if(json["trs"] != null)
			{
				var armatureGo = new GameObject();
				state.AddTrashObject(armatureGo);
				armatureGo.name = armatureTransforms.armatureName;
				armatureGo.transform.localPosition = new Vector3((float)json["trs"][0][0], (float)json["trs"][0][1], (float)json["trs"][0][2]);
				armatureGo.transform.localRotation = new Quaternion((float)json["trs"][1][0], (float)json["trs"][1][1], (float)json["trs"][1][2], (float)json["trs"][1][3]);
				armatureGo.transform.localScale = new Vector3((float)json["trs"][2][0], (float)json["trs"][2][1], (float)json["trs"][2][2]);
				armatureTransforms.root.SetParent(armatureGo.transform, false);

				armatureResource.armaturePosition = armatureGo.transform.localPosition;
				armatureResource.armatureRotation = armatureGo.transform.localRotation;
				armatureResource.armatureScale = armatureGo.transform.localScale;

				for(int i = 0; i < boneIds.Count; i++)
				{
					armatureTransforms.bindposes[i] = armatureTransforms.bones[i].worldToLocalMatrix * armatureGo.transform.localToWorldMatrix;
				}
			}
			else
			{
				for(int i = 0; i < boneIds.Count; i++)
				{
					armatureTransforms.bindposes[i] = armatureTransforms.bones[i].worldToLocalMatrix;
				}
			}
			armatureResource.bones = new List<STFArmatureResource.Bone>(boneResources);
			armatureResource.armatureTransforms = armatureTransforms;
			return armatureResource;
		}

		public GameObject parseBoneFromJson(ISTFImporter state, JToken json, STFArmatureResource.Bone boneResource, List<Task> tasks)
		{
			var go = new GameObject();
			go.name = (string)json["name"];
			var children = json["children"]?.ToObject<List<string>>();

			go.transform.localPosition = new Vector3((float)json["trs"][0][0], (float)json["trs"][0][1], (float)json["trs"][0][2]);
			go.transform.localRotation = new Quaternion((float)json["trs"][1][0], (float)json["trs"][1][1], (float)json["trs"][1][2], (float)json["trs"][1][3]);
			go.transform.localScale = new Vector3((float)json["trs"][2][0], (float)json["trs"][2][1], (float)json["trs"][2][2]);
			
			boneResource.localPosition = go.transform.localPosition;
			boneResource.localRotation = go.transform.localRotation;
			boneResource.localScale = go.transform.localScale;

			if(children != null)
			{
				tasks.Add(new Task(() => {
					foreach(var childId in children)
					{
						var child = state.GetNode(childId);
						child.transform.SetParent(go.transform, false);
						boneResource.children.Add(childId);
					}
				}));
			}
			return go;
		}
	}
}

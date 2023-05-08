
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

			// get id from original armature definition

			this.id = Guid.NewGuid().ToString();
			var boneNodes = new List<JObject>();
			for(int i = 0; i < bindposes.Length; i++)
			{
				var boneIdComponent = bones[i].GetComponent<STFUUID>();
				var boneId = boneIdComponent != null && boneIdComponent.boneId != null && boneIdComponent.boneId.Length > 0 ? boneIdComponent.boneId : Guid.NewGuid().ToString();
				var bone = new JObject();

				bone.Add("name", bones[i].name);
				bone.Add("type", "bone");

				bone.Add("trs", new JArray() {
					new JArray() {bones[i].localPosition.x, bones[i].localPosition.y, bones[i].localPosition.z},
					new JArray() {bones[i].localRotation.x, bones[i].localRotation.y, bones[i].localRotation.z, bones[i].localRotation.w},
					new JArray() {bones[i].localScale.x, bones[i].localScale.y, bones[i].localScale.z}
				});
				
				/*Matrix4x4 parentBindpose = Matrix4x4.identity;
				if(bones[i] != rootBone)
				{
					for(int bindposeIdx = 0; bindposeIdx < bindposes.Length; bindposeIdx++)
					{
						if(bones[i].parent == bones[bindposeIdx])
						{
							Debug.Log($"Bone: {bones[i].name} and parent: {bones[i].parent.name}");
							parentBindpose = bindposes[bindposeIdx];
							break;
						}

					}
				}*/
				//var transformMat = bindposes[i].inverse * parentBindpose;
				//var transformMat = bindposes[i] * parentBindpose.inverse;
				//var transformMat = parentBindpose * bindposes[i];
				//var transformMat = parentBindpose.inverse * bindposes[i];
				//var transformMat = parentBindpose.inverse * bindposes[i].inverse;
				//var transformMat = bindposes[i].inverse * parentBindpose.inverse;

				//Debug.Log($"Getting local bindpose transform of: {bones[i].name}");

				//var transformMat = parentBindpose * bindposes[i].inverse;
				/*var parent = new GameObject();
				parent.transform.localPosition = new Vector3(parentBindpose.GetColumn(3).x, parentBindpose.GetColumn(3).y, parentBindpose.GetColumn(3).z);
				parent.transform.localRotation = new Quaternion(parentBindpose.rotation.x, parentBindpose.rotation.y, parentBindpose.rotation.z, parentBindpose.rotation.w);
				parent.transform.localScale = new Vector3(parentBindpose.lossyScale.x, parentBindpose.lossyScale.y, parentBindpose.lossyScale.z);
				var child = new GameObject();
				child.transform.localPosition = new Vector3(bindposes[i].GetColumn(3).x, bindposes[i].GetColumn(3).y, bindposes[i].GetColumn(3).z);
				child.transform.localRotation = new Quaternion(bindposes[i].rotation.x, bindposes[i].rotation.y, bindposes[i].rotation.z, bindposes[i].rotation.w);
				child.transform.localScale = new Vector3(bindposes[i].lossyScale.x, bindposes[i].lossyScale.y, bindposes[i].lossyScale.z);
				child.transform.SetParent(parent.transform, true);*/
				/*bone.Add("trs", new JArray() {
					new JArray() {child.transform.localPosition.x, child.transform.localPosition.y, child.transform.localPosition.z},
					new JArray() {child.transform.localRotation.x, child.transform.localRotation.y, child.transform.localRotation.z, child.transform.localRotation.w},
					new JArray() {child.transform.localScale.x, child.transform.localScale.y, child.transform.localScale.z}
				});
				#if UNITY_EDITOR
					UnityEngine.Object.DestroyImmediate(parent);
					UnityEngine.Object.DestroyImmediate(child);
				#else
					UnityEngine.Object.Destroy(parent);
					UnityEngine.Object.Destroy(child);
				#endif*/

				//new Vector3(transformMat[0,3], transformMat[1,3], transformMat[2,3])

				/*bone.Add("trs", new JArray() {
					//new JArray() {transformMat.GetColumn(3).x, transformMat.GetColumn(3).y, transformMat.GetColumn(3).z},
					new JArray() {transformMat[0,3], transformMat[1,3], transformMat[2,3]},
					new JArray() {transformMat.rotation.x, transformMat.rotation.y, transformMat.rotation.z, transformMat.rotation.w},
					new JArray() {transformMat.lossyScale.x, transformMat.lossyScale.y, transformMat.lossyScale.z}
				});*/
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

	public class STFArmatureResource : MonoBehaviour
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
			var ret = new STFArmatureResource();
			ret.armatureName = (string)json["name"];
			var rootId = (string)json["root"];
			var rootJson = (JObject)jsonRoot["nodes"][rootId];
			var boneIds = json["bones"].ToObject<List<string>>();
			ret.bones = new Transform[boneIds.Count];
			ret.bindposes = new Matrix4x4[boneIds.Count];
			for(int i = 0; i < boneIds.Count; i++)
			{
				var boneNodeJson = jsonRoot["nodes"][boneIds[i]];
				ret.bones[i] = parseBoneFromJson(state, boneNodeJson, tasks).transform;
				var uuidComponent = ret.bones[i].gameObject.AddComponent<STFUUID>();
				uuidComponent.id = boneIds[i];
				if(boneIds[i] == rootId)
				{
					ret.root = ret.bones[i];
				}
				state.AddNode(boneIds[i], ret.bones[i].gameObject);
				state.AddTrashObject(ret.bones[i].gameObject);
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
				if(task.Exception != null) throw task.Exception;
			}
			var armatureGo = new GameObject();
			state.AddTrashObject(armatureGo);
			armatureGo.name = ret.armatureName;
			armatureGo.transform.localPosition = new Vector3((float)json["trs"][0][0], (float)json["trs"][0][1], (float)json["trs"][0][2]);
			armatureGo.transform.localRotation = new Quaternion((float)json["trs"][1][0], (float)json["trs"][1][1], (float)json["trs"][1][2], (float)json["trs"][1][3]);
			armatureGo.transform.localScale = new Vector3((float)json["trs"][2][0], (float)json["trs"][2][1], (float)json["trs"][2][2]);
			Debug.Log($"armatureGo trs: {armatureGo.transform.eulerAngles}");
			ret.root.SetParent(armatureGo.transform, false);
			for(int i = 0; i < boneIds.Count; i++)
			{
				ret.bindposes[i] = ret.bones[i].worldToLocalMatrix * armatureGo.transform.localToWorldMatrix;
			}
			return ret;
		}

		public GameObject parseBoneFromJson(ISTFImporter state, JToken json, List<Task> tasks)
		{
			var go = new GameObject();
			go.name = (string)json["name"];
			var children = json["children"]?.ToObject<List<string>>();

			go.transform.localPosition = new Vector3((float)json["trs"][0][0], (float)json["trs"][0][1], (float)json["trs"][0][2]);
			go.transform.localRotation = new Quaternion((float)json["trs"][1][0], (float)json["trs"][1][1], (float)json["trs"][1][2], (float)json["trs"][1][3]);
			go.transform.localScale = new Vector3((float)json["trs"][2][0], (float)json["trs"][2][1], (float)json["trs"][2][2]);

			if(children != null)
			{
				tasks.Add(new Task(() => {
					foreach(var childId in children)
					{
						var child = state.GetNode(childId);
						child.transform.SetParent(go.transform, false);
					}
				}));
			}
			return go;
		}
	}
}

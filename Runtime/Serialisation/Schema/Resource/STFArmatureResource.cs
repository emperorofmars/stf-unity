
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
				
				Matrix4x4 parentBindposeInverse = Matrix4x4.identity;
				if(bones[i] != rootBone)
				{
					for(int bindposeIdx = 0; bindposeIdx < bindposes.Length; bindposeIdx++)
					{
						if(bones[i].parent == bones[i])
						{
							parentBindposeInverse = bindposes[i].inverse;
							break;
						}

					}
				}
				var transformMat = bindposes[i] * parentBindposeInverse;

				bone.Add("trs", new JArray() {
					new JArray() {transformMat.GetColumn(3).x, transformMat.GetColumn(3).y, transformMat.GetColumn(3).z},
					new JArray() {transformMat.rotation.x, transformMat.rotation.y, transformMat.rotation.z, transformMat.rotation.w},
					new JArray() {transformMat.lossyScale.x, transformMat.lossyScale.y, transformMat.lossyScale.z}
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
			if(rootBone.parent != null) armatureJson.Add("name", rootBone.parent.name);
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
				if(boneIds[i] == rootId) ret.root = ret.bones[i];
				state.AddNode(boneIds[i], ret.bones[i].gameObject);
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
				if(task.Exception != null) throw task.Exception;
			}
			for(int i = 0; i < boneIds.Count; i++)
			{
				ret.bindposes[i] = ret.bones[i].worldToLocalMatrix;
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

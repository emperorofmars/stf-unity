

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public class STFArmatureResource : ScriptableObject
	{
		[Serializable]
		public class Bone
		{
			public string id;
			public string name;
			public Vector3 localPosition;
			public Quaternion localRotation;
			public Vector3 localScale;
			public List<string> children = new List<string>();
		}
		public string id;
		public string armatureName;
		public string rootId;
		public bool hasArmatureTransform = false;
		public Vector3 armaturePosition;
		public Quaternion armatureRotation;
		public Vector3 armatureScale;
		public List<Bone> bones = new List<Bone>();
		public Matrix4x4[] bindposes;

		public void parseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			this.id = id;
			name = (string)json["name"];
			armatureName = (string)json["name"];
			rootId = (string)json["root"];

			if(json["trs"] != null)
			{
				hasArmatureTransform = true;
				armaturePosition = new Vector3((float)json["trs"][0][0], (float)json["trs"][0][1], (float)json["trs"][0][2]);
				armatureRotation = new Quaternion((float)json["trs"][1][0], (float)json["trs"][1][1], (float)json["trs"][1][2], (float)json["trs"][1][3]);
				armatureScale = new Vector3((float)json["trs"][2][0], (float)json["trs"][2][1], (float)json["trs"][2][2]);
			}

			var boneIds = json["bones"].ToObject<List<string>>();

			for(int i = 0; i < boneIds.Count; i++)
			{
				var boneNodeJson = jsonRoot["nodes"][boneIds[i]];
				var resourceBone = new Bone();
				
				resourceBone.id = boneIds[i];
				resourceBone.name = (string)boneNodeJson["name"];
				resourceBone.children = boneNodeJson["children"]?.ToObject<List<string>>();

				resourceBone.localPosition = new Vector3((float)boneNodeJson["trs"][0][0], (float)boneNodeJson["trs"][0][1], (float)boneNodeJson["trs"][0][2]);
				resourceBone.localRotation = new Quaternion((float)boneNodeJson["trs"][1][0], (float)boneNodeJson["trs"][1][1], (float)boneNodeJson["trs"][1][2], (float)boneNodeJson["trs"][1][3]);
				resourceBone.localScale = new Vector3((float)boneNodeJson["trs"][2][0], (float)boneNodeJson["trs"][2][1], (float)boneNodeJson["trs"][2][2]);

				bones.Add(resourceBone);
			}

			bindposes = new Matrix4x4[boneIds.Count];
			var transforms = instantiate();
			for(int i = 0; i < boneIds.Count; i++)
			{
				bindposes[i] = transforms[i].worldToLocalMatrix;
				state.AddNode(boneIds[i], transforms[i].gameObject);
				state.AddTrashObject(transforms[i].gameObject);
			}
		}

		public Transform[] instantiate()
		{
			var ret = new Transform[bones.Count];
			var tasks = new List<Task>();

			Transform rootInstance = null;

			for(int i = 0; i < bones.Count; i++)
			{
				var boneGo = new GameObject();
				boneGo.name = bones[i].name;

				var uuidComponent = boneGo.AddComponent<STFUUID>();
				uuidComponent.boneId = bones[i].id;

				boneGo.transform.localPosition = bones[i].localPosition;
				boneGo.transform.localRotation = bones[i].localRotation;
				boneGo.transform.localScale = bones[i].localScale;

				if(bones[i].children != null)
				{
					foreach(var childId in bones[i].children)
					{
						tasks.Add(new Task(() => {
							var child = ret.First(t => t.GetComponent<STFUUID>().boneId == childId);
							child.transform.SetParent(boneGo.transform, false);
						}));
					}
				}

				ret[i] = boneGo.transform;
				if(bones[i].id == rootId) rootInstance = boneGo.transform;
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
				if(task.Exception != null) throw task.Exception;
			}
			return ret;
		}

		public void serializeToJson(ISTFExporter state)
		{
			var ret = new JObject();
			
			var armatureJson = new JObject();
			armatureJson.Add("type", STFArmatureImporter._TYPE);
			armatureJson.Add("name", armatureName);
			armatureJson.Add("root", rootId);

			if(hasArmatureTransform)
			{
				armatureJson.Add("trs", new JArray() {
					new JArray() {armaturePosition.x, armaturePosition.y, armaturePosition.z},
					new JArray() {armatureRotation.x, armatureRotation.y, armatureRotation.z, armatureRotation.w},
					new JArray() {armatureScale.x, armatureScale.y, armatureScale.z}
				});
			}
			var boneIds = new List<string>();
			foreach(var bone in bones)
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
			armatureJson.Add("bones", new JArray(boneIds));
			state.RegisterResource(id, armatureJson);

			return;
		}
	}
}

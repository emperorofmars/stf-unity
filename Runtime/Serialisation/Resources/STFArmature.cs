

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace stf.serialisation
{
	public class STFArmature : MonoBehaviour
	{
		public string armatureId;
		public string armatureName;
		public Transform root;
		public List<Transform> bones = new List<Transform>();
		public Matrix4x4[] bindposes;
	}

	public class STFArmatureExporter : ASTFResourceExporter
	{
		public override JToken SerializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			var armature = (STFArmature)resource;
			
			var ret = new JObject();
			ret.Add("type", STFArmatureImporter._TYPE);
			ret.Add("name", armature.armatureName);
			ret.Add("root", armature.root.GetComponent<STFUUID>().boneId);

			/*if(armature.transform.position.magnitude > 0.001 && armature.transform.rota)
			{
				ret.Add("trs", new JArray() {
					new JArray() {armature.transform.localPosition.x, armature.transform.localPosition.y, armature.transform.localPosition.z},
					new JArray() {armature.transform.localRotation.x, armature.transform.localRotation.y, armature.transform.localRotation.z, armature.transform.localRotation.w},
					new JArray() {armature.transform.localScale.x, armature.transform.localScale.y, armature.transform.localScale.z}
				});
			}*/
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
					childIds[childIdx] = bone.GetChild(childIdx).GetComponent<STFUUID>().boneId;
				}
				boneJson.Add("children", new JArray(childIds));

				var boneId = bone.GetComponent<STFUUID>().boneId;
				boneIds.Add(boneId);
				state.RegisterNode(boneId, boneJson);
			}
			ret.Add("bones", new JArray(boneIds));
			return ret;
		}
	}

	public class STFArmatureImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.armature";

		public override UnityEngine.Object ParseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			var go = new GameObject();
			var armature = go.AddComponent<STFArmature>();
			
			armature.armatureId = id;
			armature.name = (string)json["name"];
			armature.armatureName = (string)json["name"];

			if(json["trs"] != null)
			{
				armature.transform.localPosition = new Vector3((float)json["trs"][0][0], (float)json["trs"][0][1], (float)json["trs"][0][2]);
				armature.transform.localRotation = new Quaternion((float)json["trs"][1][0], (float)json["trs"][1][1], (float)json["trs"][1][2], (float)json["trs"][1][3]);
				armature.transform.localScale = new Vector3((float)json["trs"][2][0], (float)json["trs"][2][1], (float)json["trs"][2][2]);
			}

			var boneIds = json["bones"].ToObject<List<string>>();

			var tasks = new List<Task>();
			for(int i = 0; i < boneIds.Count; i++)
			{
				var boneNodeJson = jsonRoot["nodes"][boneIds[i]];
				var boneGO = new GameObject();
				var bone = boneGO.AddComponent<STFUUID>();
				
				bone.boneId = boneIds[i];
				bone.name = (string)boneNodeJson["name"];

				bone.transform.localPosition = new Vector3((float)boneNodeJson["trs"][0][0], (float)boneNodeJson["trs"][0][1], (float)boneNodeJson["trs"][0][2]);
				bone.transform.localRotation = new Quaternion((float)boneNodeJson["trs"][1][0], (float)boneNodeJson["trs"][1][1], (float)boneNodeJson["trs"][1][2], (float)boneNodeJson["trs"][1][3]);
				bone.transform.localScale = new Vector3((float)boneNodeJson["trs"][2][0], (float)boneNodeJson["trs"][2][1], (float)boneNodeJson["trs"][2][2]);

				armature.bones.Add(bone.transform);
				
				foreach(string childId in boneNodeJson["children"]?.ToObject<List<string>>())
				{
					tasks.Add(new Task(() => {
						var childBone = armature.bones.Find(b => b.GetComponent<STFUUID>().boneId == childId);
						childBone.SetParent(bone.transform, false);
					}));
				}
			}
			foreach(var task in tasks)
			{
				task.RunSynchronously();
				if(task.Exception != null) throw task.Exception;
			}
			var root = armature.bones.Find(b => b.GetComponent<STFUUID>().boneId == (string)json["root"]);
			root.SetParent(armature.transform, false);
			armature.root = root;

			armature.bindposes = new Matrix4x4[boneIds.Count];
			for(int i = 0; i < boneIds.Count; i++)
			{
				armature.bindposes[i] = armature.bones[i].worldToLocalMatrix;
			}

			return armature;
		}
	}
}

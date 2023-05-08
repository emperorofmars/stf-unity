
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace stf.serialisation
{
	public class STFArmatureInstanceNodeExporter// : ASTFNodeExporter
	{
		public static JObject serializeToJson(GameObject go, ISTFExporter state, string armatureId, Transform[] boneInstances)
		{
			var ret = new JObject();
			ret.Add("name", go.name);
			ret.Add("type", "armature_instance");
			ret.Add("armature", armatureId);
			ret.Add("trs", new JArray() {
				new JArray() {go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z},
				new JArray() {go.transform.localRotation.x, go.transform.localRotation.y, go.transform.localRotation.z, go.transform.localRotation.w},
				new JArray() {go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z}
			});
			state.AddTask(new Task(() => {
				var boneInstanceIds = new List<string>();
				foreach(var boneInstance in boneInstances)
					boneInstanceIds.Add(state.GetNodeId(boneInstance.gameObject));
				ret.Add("bone_instances", new JArray(boneInstanceIds));
			}));
			state.AddTask(new Task(() => {
				var children = new List<string>();
				for(int i = 0; i < go.transform.childCount; i++)
				{
					var child = go.transform.GetChild(i);
					var isBoneInstance = false;
					foreach(var boneInstance in boneInstances) if(child == boneInstance) { isBoneInstance = true; break; }
					if(!isBoneInstance) children.Add(state.GetNodeId(child.gameObject));
				}
				ret.Add("children", new JArray(children));
			}));
			return ret;
		}
	}

	public class STFArmatureInstance : MonoBehaviour
	{
		Transform root;
		Transform[] bones;
	}

	public class STFArmatureInstanceNodeImporter : ISTFNodeImporter
	{
		public static string _TYPE = "armature_instance";

		public GameObject parseFromJson(ISTFImporter state, JToken json, JObject jsonRoot, out List<string> nodesToParse)
		{
			var go = new GameObject();
			go.name = (string)json["name"];
			var children = json["children"].ToObject<List<string>>();
			nodesToParse = new List<string>(children);

			go.transform.localPosition = new Vector3((float)json["trs"][0][0], (float)json["trs"][0][1], (float)json["trs"][0][2]);
			go.transform.localRotation = new Quaternion((float)json["trs"][1][0], (float)json["trs"][1][1], (float)json["trs"][1][2], (float)json["trs"][1][3]);
			go.transform.localScale = new Vector3((float)json["trs"][2][0], (float)json["trs"][2][1], (float)json["trs"][2][2]);

			var armature = (STFArmatureResource)state.GetResource((string)json["armature"]);
			var rootInstance = Object.Instantiate(armature.root);
			rootInstance.name = armature.root.name;

			rootInstance.SetParent(go.transform, false);
			foreach(var bone in rootInstance.GetComponentsInChildren<Transform>())
			{
				var uuidComponent = bone.GetComponent<STFUUID>();
				uuidComponent.boneId = uuidComponent.id;
				uuidComponent.id = null;
			}
			var boneInstanceIds = json["bone_instances"].ToObject<List<string>>();
			for(int i = 0; i < boneInstanceIds.Count; i++)
			{
				var boneInstanceJson = jsonRoot["nodes"][boneInstanceIds[i]];
				var boneInstance = rootInstance.GetComponentsInChildren<STFUUID>().First(bi => (string)boneInstanceJson["bone"] == bi.boneId);
				boneInstance.id = boneInstanceIds[i];
				state.AddNode(boneInstance.id, boneInstance.gameObject);
				nodesToParse.AddRange(boneInstanceJson["children"].ToObject<List<string>>());
			}
			state.AddTask(new Task(() => {
				foreach(var childId in children)
				{
					var child = state.GetNode(childId);
					child.transform.SetParent(go.transform, false);
				}
			}));
			return go;
		}
	}
}

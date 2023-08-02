
using UnityEngine;
using stf.serialisation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace stf
{
	// Instance of an armature.

	public class STFArmatureInstance : MonoBehaviour
	{
		public STFArmature armature;
		public GameObject root;
		public GameObject[] bones;
	}
	public class STFArmatureInstanceNodeExporter
	{
		public static JObject SerializeToJson(GameObject go, ISTFExporter state, string armatureId, Transform[] boneInstances)
		{
			var ret = new JObject();
			ret.Add("name", go.name);
			ret.Add("type", STFArmatureInstanceNodeImporter._TYPE);
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

	public class STFArmatureInstanceNodeImporter : ISTFNodeImporter
	{
		public static string _TYPE = "STF.armature_instance";

		public GameObject ParseFromJson(ISTFImporter state, JToken json, JObject jsonRoot, out List<string> nodesToParse)
		{
			var go = new GameObject();
			go.name = (string)json["name"];
			var children = json["children"].ToObject<List<string>>();
			nodesToParse = new List<string>(children);

			var armatureInstance = go.AddComponent<STFArmatureInstance>();

			go.transform.localPosition = new Vector3((float)json["trs"][0][0], (float)json["trs"][0][1], (float)json["trs"][0][2]);
			go.transform.localRotation = new Quaternion((float)json["trs"][1][0], (float)json["trs"][1][1], (float)json["trs"][1][2], (float)json["trs"][1][3]);
			go.transform.localScale = new Vector3((float)json["trs"][2][0], (float)json["trs"][2][1], (float)json["trs"][2][2]);

			var armature = (STFArmature)state.GetResource((string)json["armature"]);
			var transforms = armature.instantiate();
			var rootInstance = transforms.First(t => t.GetComponent<STFUUID>().boneId == armature.rootId);

			armatureInstance.armature = armature;

			armatureInstance.root = rootInstance.gameObject;
			armatureInstance.bones = new GameObject[transforms.Length];

			rootInstance.SetParent(go.transform, false);
			var boneInstanceIds = json["bone_instances"].ToObject<List<string>>();
			for(int i = 0; i < boneInstanceIds.Count; i++)
			{
				var boneInstanceJson = jsonRoot["nodes"][boneInstanceIds[i]];
				var boneInstance = rootInstance.GetComponentsInChildren<STFUUID>().First(bi => (string)boneInstanceJson["bone"] == bi.boneId);
				boneInstance.id = boneInstanceIds[i];
				armatureInstance.bones[i] = boneInstance.gameObject;

				armatureInstance.bones[i].transform.localPosition = new Vector3((float)boneInstanceJson["trs"][0][0], (float)boneInstanceJson["trs"][0][1], (float)boneInstanceJson["trs"][0][2]);
				armatureInstance.bones[i].transform.localRotation = new Quaternion((float)boneInstanceJson["trs"][1][0], (float)boneInstanceJson["trs"][1][1], (float)boneInstanceJson["trs"][1][2], (float)boneInstanceJson["trs"][1][3]);
				armatureInstance.bones[i].transform.localScale = new Vector3((float)boneInstanceJson["trs"][2][0], (float)boneInstanceJson["trs"][2][1], (float)boneInstanceJson["trs"][2][2]);

				state.AddNode(boneInstance.id, boneInstance.gameObject);

				var instanceChildren = boneInstanceJson["children"].ToObject<List<string>>();
				if(instanceChildren != null)
				{
					nodesToParse.AddRange(boneInstanceJson["children"].ToObject<List<string>>());
					state.AddTask(new Task(() => {
						foreach(var childId in instanceChildren)
						{
							var child = state.GetNode(childId);
							child.transform.SetParent(boneInstance.transform, false);
						}
					}));
				}
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

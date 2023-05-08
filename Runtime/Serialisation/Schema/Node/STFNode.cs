
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{
	public class STFNodeExporter
	{
		public static JObject serializeToJson(GameObject go, ISTFExporter state)
		{
			var ret = new JObject();
			ret.Add("name", go.name);
			ret.Add("trs", new JArray() {
				new JArray() {go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z},
				new JArray() {go.transform.localRotation.x, go.transform.localRotation.y, go.transform.localRotation.z, go.transform.localRotation.w},
				new JArray() {go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z}
			});
			state.AddTask(new Task(() => {
				var children = new List<string>();
				for(int i = 0; i < go.transform.childCount; i++)
				{
					var child = go.transform.GetChild(i);
					children.Add(state.GetNodeId(child.gameObject));
				}
				ret.Add("children", new JArray(children));
			}));
			return ret;
		}

		public static JObject serializeBoneInstanceToJson(GameObject go, ISTFExporter state, string boneId)
		{
			var ret = new JObject();
			ret.Add("name", go.name);
			ret.Add("type", "bone_instance");
			ret.Add("bone", boneId);
			ret.Add("trs", new JArray() {
				new JArray() {go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z},
				new JArray() {go.transform.localRotation.x, go.transform.localRotation.y, go.transform.localRotation.z, go.transform.localRotation.w},
				new JArray() {go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z}
			});
			return ret;
		}
	}

	public class STFNodeImporter : ISTFNodeImporter
	{
		public static string _TYPE = "default";
		
		public GameObject parseFromJson(ISTFImporter state, JToken json, JObject jsonRoot, out List<string> nodesToParse)
		{
			var go = new GameObject();
			go.name = (string)json["name"];
			var children = json["children"].ToObject<List<string>>();
			nodesToParse = children;

			go.transform.localPosition = new Vector3((float)json["trs"][0][0], (float)json["trs"][0][1], (float)json["trs"][0][2]);
			go.transform.localRotation = new Quaternion((float)json["trs"][1][0], (float)json["trs"][1][1], (float)json["trs"][1][2], (float)json["trs"][1][3]);
			go.transform.localScale = new Vector3((float)json["trs"][2][0], (float)json["trs"][2][1], (float)json["trs"][2][2]);

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

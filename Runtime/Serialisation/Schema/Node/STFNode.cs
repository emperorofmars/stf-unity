
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{
	public class STFNodeExporter : ASTFNodeExporter
	{
		override public JToken serializeToJson(GameObject go, ISTFExporter state)
		{
			//Matrix4x4 transformMat = Matrix4x4.TRS(go.transform.localScale, go.transform.localRotation, go.transform.localScale);
			var ret = (JObject)base.serializeToJson(go, state);
			/*ret.Add("transform", new JArray() {
				new JArray() {transformMat.m00, transformMat.m01, transformMat.m02, transformMat.m03},
				new JArray() {transformMat.m10, transformMat.m11, transformMat.m12, transformMat.m13},
				new JArray() {transformMat.m20, transformMat.m21, transformMat.m12, transformMat.m23},
				new JArray() {transformMat.m30, transformMat.m31, transformMat.m32, transformMat.m33}
			});*/
			/*ret.Add("trs", new JArray() {
				new JArray() {go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z},
				new JArray() {go.transform.localRotation.x, go.transform.localRotation.y, go.transform.localRotation.z, go.transform.localRotation.w},
				new JArray() {go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z}
			});*/
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
	}

	public class STFNodeImporter : ASTFNodeImporter
	{
		override public GameObject parseFromJson(ISTFImporter state, JToken json)
		{
			var go = new GameObject();
			go.name = (string)json["name"];
			var children = json["children"].ToObject<List<string>>();

			/*var transformMat = new Matrix4x4();
			transformMat.m00 = (float)json["transform"][0][0];
			transformMat.m01 = (float)json["transform"][0][1];
			transformMat.m02 = (float)json["transform"][0][2];
			transformMat.m03 = (float)json["transform"][0][3];
			transformMat.m10 = (float)json["transform"][1][0];
			transformMat.m11 = (float)json["transform"][1][1];
			transformMat.m12 = (float)json["transform"][1][2];
			transformMat.m13 = (float)json["transform"][1][3];
			transformMat.m20 = (float)json["transform"][2][0];
			transformMat.m21 = (float)json["transform"][2][1];
			transformMat.m22 = (float)json["transform"][2][2];
			transformMat.m23 = (float)json["transform"][2][3];
			transformMat.m30 = (float)json["transform"][3][0];
			transformMat.m31 = (float)json["transform"][3][1];
			transformMat.m32 = (float)json["transform"][3][2];
			transformMat.m33 = (float)json["transform"][3][3];

			go.transform.localPosition = transformMat.GetColumn(3);
			go.transform.localRotation = transformMat.rotation;
			go.transform.localScale = transformMat.lossyScale;*/

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

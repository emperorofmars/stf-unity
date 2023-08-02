
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{
	public class STFBoneInstanceNodeExporter// : ASTFNodeExporter
	{
		public static string _TYPE = "STF.bone_instance";
		public static JObject SerializeToJson(GameObject go, ISTFExporter state, string boneId, Transform[] boneInstances)
		{
			var ret = new JObject();
			ret.Add("name", go.name);
			ret.Add("type", _TYPE);
			ret.Add("bone", boneId);
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
					var isBoneInstance = false;
					foreach(var boneInstance in boneInstances) if(child == boneInstance) { isBoneInstance = true; break; }
					if(!isBoneInstance) children.Add(state.GetNodeId(child.gameObject));
				}
				ret.Add("children", new JArray(children));
			}));
			return ret;
		}
	}
}

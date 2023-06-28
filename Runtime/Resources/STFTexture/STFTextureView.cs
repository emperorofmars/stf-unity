
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace stf.serialisation
{
	public class STFTextureView : ScriptableObject
	{
		public Texture2D texture;
		public int channel;
		public bool invert;
	}

	public class STFTextureViewImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.texture_view";

		public override UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			var ret = ScriptableObject.CreateInstance<STFTextureView>();
			ret.channel = (int)json["channel"];
			ret.invert = (bool)json["invert"];
			state.AddTask(new Task(() => {
				ret.texture = (Texture2D)state.GetResource((string)json["texture"]);
				ret.name = ret.texture.name + "_view_" + ret.channel;
			}));
			return ret;
		}
	}
}

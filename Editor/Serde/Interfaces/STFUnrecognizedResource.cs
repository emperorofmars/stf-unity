
#if UNITY_EDITOR

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace STF.Serde
{
	public class STFUnrecognizedResource : ScriptableObject
	{
		public string id;
		public string json;
		public List<GameObject> referencedNodes;
		//public List<STFBuffer> referencedBuffers;
		public List<UnityEngine.Object> referencedResources;
	}

	/*public class STFUnrecognizedResourceImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.texture_view";

		public override UnityEngine.Object ParseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
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
	}*/

	/*public class STFUnrecognizedResourceExporter : ASTFResourceExporter
	{
		public static string _TYPE = "STF.texture_view";

		public override UnityEngine.Object ParseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
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
	}*/
}

#endif

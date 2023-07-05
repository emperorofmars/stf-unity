
#if UNITY_EDITOR

using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace stf.serialisation
{
	public class STFEncodedImageTextureImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.texture";
		public string imageParentPath = null;

		public override UnityEngine.Object ParseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			var arrayBuffer = state.GetBuffer((string)json["buffer"]);
			var name = (string)json["name"];
			var format = (string)json["format"];

			try{
				// I hate this, why do i have to do this @Unity ???
				string path;
				if(imageParentPath == null) path = "Assets/" + id + "_" + name + "." + format;
				else path = imageParentPath + "/" + name + "." + format;
				File.WriteAllBytes(path, arrayBuffer);
				AssetDatabase.Refresh();
				var ret = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				ret.name = name;
				state.GetMeta().resourceInfo.Add(new STFMeta.ResourceInfo {type = "texture", name = name, originalExternalAssetPath = path, resource = ret, id = id, originalFormat = format, external = true });
				return ret;
			} catch(Exception e)
			{
				Debug.LogError(e);
				throw e;
			}
		}
	}
}

#endif

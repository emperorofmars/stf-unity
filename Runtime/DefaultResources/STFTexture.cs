
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace stf.serialisation
{
	public class STFTextureExporter : ASTFResourceExporter
	{
		public override JToken serializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			try{
				var texture = (Texture2D)resource;
				var ret = new JObject();
				ret.Add("type", "STF.texture");

				var path = AssetDatabase.GetAssetPath(texture);
				
				ret.Add("name", texture.name);
				ret.Add("format", path.Substring(path.LastIndexOf('.') + 1, path.Length - path.LastIndexOf('.') - 1));
				ret.Add("width", texture.width);
				ret.Add("height", texture.height);

				byte[] bytes = File.ReadAllBytes(path);

				var bufferId = state.RegisterBuffer(bytes);
				ret.Add("buffer", bufferId);


				return ret;
			} catch (Exception e)
			{
				Debug.LogError(e);
				throw e;
			}
		}
	}

	public class STFTextureImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.texture";

		public override UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id)
		{
			var arrayBuffer = state.GetBuffer((string)json["buffer"]);

			var ret = new Texture2D((int)json["width"], (int)json["height"]);
			ret.name = (string)json["name"];
			ret.LoadImage(arrayBuffer);
			
			return ret;
		}
	}
}

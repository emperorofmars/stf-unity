
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
				ret.Add("width", texture.width);
				ret.Add("height", texture.height);

				var path = AssetDatabase.GetAssetPath(texture);
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
			//var ret = new Texture2D()
			return null;
		}
	}
}

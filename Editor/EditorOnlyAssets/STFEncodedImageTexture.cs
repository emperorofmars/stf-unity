
#if UNITY_EDITOR

using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace stf.serialisation
{
	public class STFEncodedImageTextureExporter : ASTFResourceExporter
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

	public class STFEncodedImageTextureImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.texture";
		public string imageParentPath = null;

		public override UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id)
		{
			var arrayBuffer = state.GetBuffer((string)json["buffer"]);
			var name = (string)json["name"];
			var format = (string)json["format"];

			try{
				// I hate this, why do i have to do this @Unity ???
				string filename;
				if(imageParentPath == null) filename = "Assets/" + id + "_" + name + "." + format;
				else filename = imageParentPath + "/" + name + "." + format;
				Debug.Log(imageParentPath);
				Debug.Log(filename);
				File.WriteAllBytes(filename, arrayBuffer);
				AssetDatabase.Refresh();
				return AssetDatabase.LoadAssetAtPath<Texture2D>(filename);
			} catch(Exception e)
			{
				Debug.LogError(e);
				throw e;
			}
		}
	}

	[InitializeOnLoad]
	public class Register_STFEncodedImageTextureImporterImporter
	{
		static Register_STFEncodedImageTextureImporterImporter()
		{
			STFRegistry.RegisterResourceImporter(STFEncodedImageTextureImporter._TYPE, new STFEncodedImageTextureImporter());
			STFRegistry.RegisterResourceExporter(typeof(Texture2D), new STFEncodedImageTextureExporter());
		}
	}
}

#endif

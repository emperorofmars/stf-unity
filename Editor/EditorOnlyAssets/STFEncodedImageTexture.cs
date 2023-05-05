
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
		public string imageParentPath = null;

		public override JToken serializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			var texture = (Texture2D)resource;
			var ret = new JObject();
			ret.Add("type", "STF.texture");
			ret.Add("name", texture.name);
			ret.Add("width", texture.width);
			ret.Add("height", texture.height);

			try{
				if(AssetDatabase.IsMainAsset(texture)) // If its an encoded image outside the original import
				{
					var path = AssetDatabase.GetAssetPath(texture);
					byte[] bytes = File.ReadAllBytes(path);
					var bufferId = state.RegisterBuffer(bytes);
					ret.Add("format", path.Substring(path.LastIndexOf('.') + 1, path.Length - path.LastIndexOf('.') - 1));
					ret.Add("buffer", bufferId);
					return ret;
				}
				var meta = AssetDatabase.LoadAssetAtPath<STFMeta>(AssetDatabase.GetAssetPath(texture));
				if(meta != null) // use the stf meta object to find the original encoded image
				{
					var info = meta.resourceInfo.Find(ri => ri.resource == resource);
					if(info != null && info.assetPath != null)
					{
						byte[] bytes = File.ReadAllBytes(info.assetPath);
						if(bytes != null && bytes.Length > 0)
						{
							var bufferId = state.RegisterBuffer(bytes);
							ret.Add("format", info.originalFormat);
							ret.Add("buffer", bufferId);
							return ret;
						}
					}
				}
				/*if(imageParentPath != null
					&& (File.Exists(imageParentPath + "/" + resource.name + ".png")
						|| File.Exists(imageParentPath + "/" + resource.name + ".jpg")
						|| File.Exists(imageParentPath + "/" + resource.name + ".jpeg"))) // If its using the external folder
				{
					Debug.Log("Texture Path: " + AssetDatabase.GetAssetPath(texture) + "; foreign? " + AssetDatabase.IsForeignAsset(texture) + "; sub? " + AssetDatabase.IsSubAsset(texture));

					string path;
					if(imageParentPath != null) path = imageParentPath + "/" + resource.name;
					else throw new Exception("No parent path for encoded images found!");

					if(File.Exists(path + ".png")) path += ".png";
					else if(File.Exists(path + ".jpg")) path += ".jpg";
					else if(File.Exists(path + ".jpeg")) path += ".jpeg";
					
					byte[] bytes = File.ReadAllBytes(path);
					var bufferId = state.RegisterBuffer(bytes);
					ret.Add("format", path.Substring(path.LastIndexOf('.') + 1, path.Length - path.LastIndexOf('.') - 1));
					ret.Add("buffer", bufferId);

					return ret;
				}*/
				
				{ // As a fallback encode the decompressed texture to png. Hopefully it was losslessly encoded, otherwise the loss was just a loss.
					Debug.LogWarning($"Encoding texture {texture.name} as png.");

					byte[] bytes = texture.EncodeToPNG();
					var bufferId = state.RegisterBuffer(bytes);
					ret.Add("buffer", bufferId);
					return ret;
				}
			} catch (Exception e)
			{
				Debug.LogError(e);
				throw(e);
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
				string path;
				if(imageParentPath == null) path = "Assets/" + id + "_" + name + "." + format;
				else path = imageParentPath + "/" + name + "." + format;
				File.WriteAllBytes(path, arrayBuffer);
				AssetDatabase.Refresh();
				var ret = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				ret.name = name;
				state.GetMeta().resourceInfo.Add(new STFMeta.ResourceInfo {name = name, assetPath = path, resource = ret, uuid = id, originalFormat = format, external = true });
				return ret;
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
			//STFRegistry.RegisterResourceImporter(STFEncodedImageTextureImporter._TYPE, new STFEncodedImageTextureImporter());
			STFRegistry.RegisterResourceExporter(typeof(Texture2D), new STFEncodedImageTextureExporter());
		}
	}
}

#endif

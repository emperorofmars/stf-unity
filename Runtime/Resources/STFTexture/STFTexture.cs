
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace stf.serialisation
{
	public class STFTextureExporter : ASTFResourceExporter
	{
		public override JToken serializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			var texture = (Texture2D)resource;
			var ret = new JObject();
			ret.Add("type", STFTextureImporter._TYPE);

			STFTextureResource originalTexture = null;
			foreach(var meta in state.GetMetas())
			{
				var resourceInfo = meta.resourceInfo.Find(ri => ri.resource == texture);
				if(resourceInfo != null)
				{
					originalTexture = (STFTextureResource)resourceInfo.originalResource;
					break;
				}
			}
			if(originalTexture != null)
			{
				ret.Add("name", originalTexture.originalName);
				ret.Add("format", originalTexture.format);
				ret.Add("width", originalTexture.width);
				ret.Add("height", originalTexture.height);
				ret.Add("linear", originalTexture.linear);
				var bufferId = state.RegisterBuffer(originalTexture.data);
				ret.Add("buffer", bufferId);
			}
			#if UNITY_EDITOR
			else if(AssetDatabase.IsMainAsset(texture)) // If its an encoded image outside the original import
			{
				var path = AssetDatabase.GetAssetPath(texture);
				var tmpTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				byte[] bytes = File.ReadAllBytes(path);

				var bufferId = state.RegisterBuffer(bytes);
				
				ret.Add("name", tmpTex.name);
				ret.Add("format", path.Substring(path.LastIndexOf('.') + 1, path.Length - path.LastIndexOf('.') - 1));
				ret.Add("width", tmpTex.width);
				ret.Add("height", tmpTex.height);
				if(tmpTex.graphicsFormat.ToString().ToLower().EndsWith("unorm")) ret.Add("linear", true);
				else ret.Add("linear", false);

				ret.Add("buffer", bufferId);
				return ret;
			}
			#endif
			else // will hard encode potentially lossy texture as png since the original format is unknown and png is lossless
			{
				ret.Add("name", texture.name);
				ret.Add("format", "png");
				ret.Add("width", texture.width);
				ret.Add("height", texture.height);
				if(texture.graphicsFormat.ToString().ToLower().EndsWith("unorm")) ret.Add("linear", true);
				else ret.Add("linear", false);

				var bufferId = state.RegisterBuffer(texture.EncodeToPNG());
				ret.Add("buffer", bufferId);
			}
			return ret;
	}
	}

	public class STFTextureImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.texture";

		public override UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			// will load the gpu compressed data into memory, use only for runtime use
			var arrayBuffer = state.GetBuffer((string)json["buffer"]);
			var ret = new Texture2D((int)json["width"], (int)json["height"], (bool)json["linear"] ? TextureFormat.DXT5 : TextureFormat.BC7, true, (bool)json["linear"]);
			ret.name = (string)json["name"];

			var originalTextureResource = ScriptableObject.CreateInstance<STFTextureResource>();
			originalTextureResource.name = (string)json["name"] + "OriginalBuffer";
			originalTextureResource.originalName = (string)json["name"];
			originalTextureResource.format = (string)json["format"];
			originalTextureResource.width = (int)json["width"];
			originalTextureResource.height = (int)json["height"];
			originalTextureResource.linear = (bool)json["linear"];
			originalTextureResource.data = arrayBuffer;

			state.AddResources(id + "_original", originalTextureResource);

			ret.LoadImage(arrayBuffer);
			ret.Compress(true);

			state.GetMeta().resourceInfo.Add(new STFMeta.ResourceInfo {type = "texture", name = ret.name, resource = ret, id = id, originalFormat = (string)json["format"], external = false, originalResource = originalTextureResource});
			return ret;
		}
	}
}

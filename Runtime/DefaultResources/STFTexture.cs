
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

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
				ret.Add("name", texture.name);
				
				ret.Add("format", "png");
				ret.Add("width", texture.width);
				ret.Add("height", texture.height);

				// will hard encode as png since the original format is unknown
				byte[] bytes = texture.EncodeToPNG();

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

		public override UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			// will load the uncompressed data into memory, use only at runtime
			var arrayBuffer = state.GetBuffer((string)json["buffer"]);
			var ret = new Texture2D((int)json["width"], (int)json["height"]);
			ret.name = (string)json["name"];
			ret.LoadImage(arrayBuffer);
			state.GetMeta().resourceInfo.Add(new STFMeta.ResourceInfo {name = ret.name, resource = ret, uuid = id, originalFormat = (string)json["format"], external = false });
			return ret;
		}
	}
}

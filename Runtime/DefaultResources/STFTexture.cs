
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace stf.serialisation
{
	public class STFTextureExporter : ASTFResourceExporter
	{
		public override JToken serializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			var texture = (Texture2D)resource;
			var ret = new JObject();
			ret.Add("type", "STF.texture");

			return ret;
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

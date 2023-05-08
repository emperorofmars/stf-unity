
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace stf.serialisation
{
	public class STFMaterialExporter : ASTFResourceExporter
	{
		public override JToken serializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			try{
				var material = (Material)resource;
				var ret = new JObject();
				ret.Add("type", STFMaterialImporter._TYPE);
				ret.Add("name", material.name);
				
				/*ret.Add("format", "png");
				ret.Add("width", texture.width);
				ret.Add("height", texture.height);

				// will hard encode as png since the original format is unknown
				byte[] bytes = texture.EncodeToPNG();

				var bufferId = state.RegisterBuffer(bytes);
				ret.Add("buffer", bufferId);*/


				return ret;
			} catch (Exception e)
			{
				Debug.LogError(e);
				throw e;
			}
		}
	}

	public class STFMaterialImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.material";

		public override UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			var ret = new Material(Shader.Find("Standard"));
			ret.name = (string)json["name"];
			return ret;
		}
	}
}
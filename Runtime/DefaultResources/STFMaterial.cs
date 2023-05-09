
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
			state.GetMeta().resourceInfo.Add(new STFMeta.ResourceInfo {name = ret.name, resource = ret, id = id });
			return ret;
		}
	}
}

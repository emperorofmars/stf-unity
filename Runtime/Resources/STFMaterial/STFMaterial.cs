
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace stf.serialisation
{
	public class STFMaterialExporter : ASTFResourceExporter
	{
		public override JToken serializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			var material = (Material)resource;
			var ret = new JObject();
			ret.Add("type", STFMaterialImporter._TYPE);
			ret.Add("name", material.name);

			// Handle Existing STF Materials in STFMeta

			if(STFShaderRegistry.Converters.ContainsKey(material.shader.name))
			{
				var stfMaterial = STFShaderRegistry.Converters[material.shader.name].TranslateUnityToSTF(state, material);
				foreach(var property in stfMaterial.Properties)
				{
					var jsonProperty = new JObject();
					jsonProperty.Add("type", property.Type);
					jsonProperty.Add("value", property.SerializeValue());
					ret.Add(property.Name, jsonProperty);
				}
			}
			else
			{
				Debug.LogWarning($"Shader {material.shader.name} has no registered converter!");
			}

			return ret;
		}
	}

	public class STFMaterialImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.material";

		public override UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			var stfMaterial = ScriptableObject.CreateInstance<STFMaterial>();
			stfMaterial.name = (string)json["name"];
			foreach(var jsonProperty in ((JObject)json).Properties())
			{
				if(jsonProperty.Value.Type != JTokenType.Object) continue;
				var property = new STFMaterial.ShaderProperty();
				property.Name = jsonProperty.Name;
				property.Type = (string)jsonProperty.Value["type"];
				property.Targets = jsonProperty.Value["targets"]?.ToObject<List<string>>();
				property.ParseJsonValue(jsonProperty.Value["value"]);
				stfMaterial.Properties.Add(property);
			}

			Material ret = null;
			if((string)json["target"] != null && STFShaderRegistry.Converters.ContainsKey((string)json["target"]))
			{
				ret = STFShaderRegistry.Converters[(string)json["target"]].TranslateSTFToUnity(state, stfMaterial);
			}
			else
			{
				Debug.LogWarning("Unrecognized shader target, falling back to Standard");

				ret = STFShaderRegistry.Converters["Standard"].TranslateSTFToUnity(state, stfMaterial);
			}

			ret.name = (string)json["name"];
			state.GetMeta().resourceInfo.Add(new STFMeta.ResourceInfo {name = ret.name, resource = ret, id = id });
			return ret;
		}
	}
}

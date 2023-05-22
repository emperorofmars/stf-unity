
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace stf.serialisation
{
	public class STFMaterial : ScriptableObject
	{
		
		[Serializable]
		public class ShaderTarget
		{
			public string target;
			public List<string> shaders;
		}

		[Serializable]
		public class ShaderProperty
		{
			public string Name;
			public string Type;
			public List<ShaderTarget> Targets;
			public object Value;

			public string SerializeValue()
			{
				if(Value.GetType() == typeof(String))
				{
					return (string)Value;
				}
				throw new Exception($"Unknown ShaderProperty Value: {Value.GetType()}");
			}

			public void ParseJsonValue(JToken json)
			{
				switch(Type)
				{
					case "texture": Value = json.ToString(); break;
					case "string": Value = json.ToString(); break;
				}
			}
		}
		public List<ShaderTarget> ShaderTargets = new List<ShaderTarget>();
		public List<ShaderProperty> Properties = new List<ShaderProperty>();
	}

	public class STFMaterialExporter : ASTFResourceExporter
	{
		public override JToken serializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			var material = (Material)resource;
			var ret = new JObject();
			ret.Add("type", STFMaterialImporter._TYPE);
			ret.Add("name", material.name);

			// Handle Existing STF Materials in STFMeta

			ISTFShaderTranslator converter = null;
			string shaderName = "";
			foreach(var c in STFShaderRegistry.Converters)
			{
				if(material.shader.name.Contains(c.Key))
				{
					converter = c.Value;
					shaderName = c.Key;
				}
			}
			if(converter != null)
			{
				ret.Add("targets", new JArray() {new JObject() {{"target", "Unity"}, {"shaders", new JArray() {shaderName}}}});
				var stfMaterial = converter.TranslateUnityToSTF(state, material);
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
				property.Targets = jsonProperty.Value["targets"]?.ToObject<List<STFMaterial.ShaderTarget>>();
				property.ParseJsonValue(jsonProperty.Value["value"]);
				stfMaterial.Properties.Add(property);
			}

			Material ret = null;
			List<string> unityTargets = null;
			foreach(var app in json["targets"])
			{
				if((string)app["target"] == "Unity")
				{
					unityTargets = app["shaders"].ToObject<List<string>>();
					break;
				}
			}
			bool shaderConverted = false;
			if(unityTargets != null) {
				foreach(var t in unityTargets)
				{
					if(STFShaderRegistry.Converters.ContainsKey(t) && STFShaderRegistry.Converters[t].IsShaderPresent())
					{
						ret = STFShaderRegistry.Converters[t].TranslateSTFToUnity(state, stfMaterial);
						shaderConverted = true;
						break;
					}
				}
			}
			// handle shader hints before full fallback
			if(!shaderConverted)
			{
				Debug.LogWarning("Unrecognized shader target, falling back to Standard");

				ret = STFShaderRegistry.Converters[STFShaderTranslatorStandard._SHADER_NAME].TranslateSTFToUnity(state, stfMaterial);
			}

			ret.name = (string)json["name"];
			state.AddResources(id + "_original", stfMaterial);
			state.GetMeta().resourceInfo.Add(new STFMeta.ResourceInfo {type = "material", name = ret.name, resource = ret, originalResource = stfMaterial, id = id });
			return ret;
		}
	}
}

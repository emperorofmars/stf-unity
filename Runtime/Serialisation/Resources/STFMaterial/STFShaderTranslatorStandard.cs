
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{

	public class STFShaderTranslatorStandard : ISTFShaderTranslator
	{
		public static readonly string _SHADER_NAME = "Standard";

		public bool IsShaderPresent()
		{
			return Shader.Find(_SHADER_NAME) != null;
		}

		public Material TranslateSTFToUnity(ISTFImporter state, STFMaterial stfMaterial)
		{
			var ret = new Material(Shader.Find(_SHADER_NAME));
			foreach(var property in stfMaterial.Properties)
			{
				if(property.Name == "transparency_mode")
				{
					int mode = 0;
					switch(property.Value) {
						case "opaque": mode = 0; break;
						case "cutout": mode = 1; break;
						case "transparent": mode = 3; break;
					}
					ret.SetFloat("_Mode", mode);
				}
			}
			STFShaderTranslatorHelpers.ImportTexture(state, ret, stfMaterial, "_MainTex", "albedo");
			STFShaderTranslatorHelpers.ImportTexture(state, ret, stfMaterial, "_BumpMap", "normal");
			STFShaderTranslatorHelpers.ImportTexture(state, ret, stfMaterial, "_MetallicGlossMap", "MetallicSmoothness");
			return ret;
		}

		public STFMaterial TranslateUnityToSTF(ISTFExporter state, Material material)
		{
			// handle existing stf material
			var ret = ScriptableObject.CreateInstance<STFMaterial>();
			ret.name = material.name;
			ret.ShaderTargets.Add(new STFMaterial.ShaderTarget {target = "Unity", shaders = new List<string> {_SHADER_NAME}});

			/*foreach(var name in material.GetTexturePropertyNames())
			{
				Debug.Log($"GetTexturePropertyNames: {name}");
			}*/

			string mode = "";
			switch(material.GetFloat("_Mode")) {
				case 0: mode = "opaque"; break;
				case 1: mode = "cutout"; break;
				case 3: mode = "transparent"; break;
			}
			ret.Properties.Add(new STFMaterial.ShaderProperty {
				Name = "transparency_mode",
				Type = "string",
				Value = mode
			});

			STFShaderTranslatorHelpers.ExportTexture(state, material, ret, "_MainTex", "albedo");
			STFShaderTranslatorHelpers.ExportTexture(state, material, ret, "_BumpMap", "normal");
			STFShaderTranslatorHelpers.ExportTextureChannel(state, material, ret, "_MetallicGlossMap", 0, "metallic");
			STFShaderTranslatorHelpers.ExportTextureChannel(state, material, ret, "_MetallicGlossMap", 3, "roughness", true);
			
			ret.Properties.Add(new STFMaterial.ShaderProperty {
				Name = "lighting_hint",
				Type = "String",
				Value = "realistic"
			});

			return ret;
		}
	}
}
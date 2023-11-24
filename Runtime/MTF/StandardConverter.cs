
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;

namespace MTF
{
	public class StandardConverter : IMaterialConverter
	{
		public const string _SHADER_NAME = "Standard";
		public string ShaderName {get => _SHADER_NAME;}
		public UnityEngine.Material ConvertToUnityMaterial(IMaterialConvertState State, Material MTFMaterial, UnityEngine.Material ExistingUnityMaterial = null)
		{
			var ret = ExistingUnityMaterial != null ? ExistingUnityMaterial : new UnityEngine.Material(Shader.Find(ShaderName));
			ret.name = ExistingUnityMaterial != null ? ExistingUnityMaterial.name : MTFMaterial.name;

			MaterialConverterUtil.SetTextureProperty(MTFMaterial, ret, "albedo", "_MainTex");
			MaterialConverterUtil.SetColorProperty(MTFMaterial, ret, "albedo", "_Color");

			MaterialConverterUtil.SetTextureProperty(MTFMaterial, ret, "normal", "_BumpMap");

			{
				var textureChannels = new List<(List<IPropertyValue>, bool)>();

				var metallicValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "metallic");
				if(metallicValue != null) textureChannels.Add((metallicValue, false));
				else textureChannels.Add((null, false));

				textureChannels.Add((null, false));
				textureChannels.Add((null, false));

				var smoothnessValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "smoothness");
				var roughnessValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "roughness");
				if(smoothnessValue != null) textureChannels.Add((smoothnessValue, false));
				else if(roughnessValue != null) textureChannels.Add((roughnessValue, true));
				else textureChannels.Add((null, false));

				MaterialConverterUtil.AssembleTextureChannels(State, textureChannels, ret, "_MetallicGlossMap");
			}

			MaterialConverterUtil.SetFloatProperty(MTFMaterial, ret, "specular", "_SpecularHighlights");
			return ret;
		}
	}

	public class StandardParser : IMaterialParser
	{
		public string ShaderName {get => StandardConverter._SHADER_NAME;}
		public Material ParseFromUnityMaterial(IMaterialParseState State, UnityEngine.Material UnityMaterial, Material ExistingMTFMaterial = null)
		{
			var ret = ExistingMTFMaterial != null ? ExistingMTFMaterial : ScriptableObject.CreateInstance<Material>();
			ret.name = ExistingMTFMaterial != null ? ExistingMTFMaterial.name : UnityMaterial.name;
			ret.PreferedShaderPerTarget.Add(new Material.ShaderTarget{Platform = "unity3d", Shaders = new List<string>{ShaderName}});
			ret.StyleHints.Add("realistic");

			MaterialParserUtil.ParseTextureProperty(UnityMaterial, ret, "albedo", "_MainTex");
			MaterialParserUtil.ParseColorProperty(UnityMaterial, ret, "albedo", "_Color");
			
			MaterialParserUtil.ParseTextureProperty(UnityMaterial, ret, "normal", "_BumpMap");
			
			MaterialParserUtil.ParseTextureChannelProperty(UnityMaterial, ret, "metallic", 0, "_MetallicGlossMap");
			MaterialParserUtil.ParseTextureChannelProperty(UnityMaterial, ret, "smoothness", 4, "_MetallicGlossMap");
			MaterialParserUtil.ParseFloatProperty(UnityMaterial, ret, "specular", "_SpecularHighlights");
			return ret;
		}
	}
}

#endif

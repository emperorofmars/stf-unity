
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MTF.Addons
{
	public class PoiyomiConverter : IMaterialConverter
	{
		public const string _SHADER_NAME = ".poiyomi/Poiyomi 8.1/Poiyomi Toon";
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

				var smoothnessValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "smoothness");
				var roughnessValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "roughness");
				if(smoothnessValue != null) textureChannels.Add((smoothnessValue, false));
				else if(roughnessValue != null) textureChannels.Add((roughnessValue, true));
				else textureChannels.Add((null, false));

				var reflectionValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "reflection");
				if(reflectionValue != null) textureChannels.Add((reflectionValue, false));
				else if(metallicValue != null) textureChannels.Add((metallicValue, false));
				else textureChannels.Add((null, false));

				var specularValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "specular");
				if(specularValue != null) textureChannels.Add((specularValue, false));
				else if(smoothnessValue != null) textureChannels.Add((smoothnessValue, true));
				else if(roughnessValue != null) textureChannels.Add((roughnessValue, true));
				else textureChannels.Add((null, false));

				if(MaterialConverterUtil.AssembleTextureChannels(State, textureChannels, ret, "_MochieMetallicMaps"))
				{
					ret.SetFloat("_MochieBRDF", 1);
					ret.SetFloat("_MochieMetallicMultiplier", 1);
				}
			}
			return ret;
		}
	}

	public class PoiyomiParser : IMaterialParser
	{
		public string ShaderName {get => PoiyomiConverter._SHADER_NAME;}
		public Material ParseFromUnityMaterial(IMaterialParseState State, UnityEngine.Material UnityMaterial, Material ExistingMTFMaterial = null)
		{
			var ret = ExistingMTFMaterial != null ? ExistingMTFMaterial : ScriptableObject.CreateInstance<Material>();
			ret.name = ExistingMTFMaterial != null ? ExistingMTFMaterial.name : UnityMaterial.name;
			ret.PreferedShaderPerTarget.Add(new Material.ShaderTarget{Platform = "unity3d", Shaders = new List<string>{ShaderName}});
			//ret.StyleHints.Add("realistic");
			
			MaterialParserUtil.ParseTextureProperty(UnityMaterial, ret, "albedo", "_MainTex");
			MaterialParserUtil.ParseColorProperty(UnityMaterial, ret, "albedo", "_Color");
			MaterialParserUtil.ParseTextureProperty(UnityMaterial, ret, "normal", "_BumpMap");

			MaterialParserUtil.ParseTextureChannelProperty(UnityMaterial, ret, "metallic", 0, "_MochieMetallicMaps");
			MaterialParserUtil.ParseTextureChannelProperty(UnityMaterial, ret, "smoothness", 1, "_MochieMetallicMaps");
			MaterialParserUtil.ParseTextureChannelProperty(UnityMaterial, ret, "reflection", 2, "_MochieMetallicMaps");
			MaterialParserUtil.ParseTextureChannelProperty(UnityMaterial, ret, "specular", 3, "_MochieMetallicMaps");
			return ret;
		}
	}

	[InitializeOnLoad]
	class Register_PoiyomiMaterial
	{
		static Register_PoiyomiMaterial()
		{
			ShaderConverterRegistry.RegisterMaterialConverter(PoiyomiConverter._SHADER_NAME, new PoiyomiConverter());
			ShaderConverterRegistry.RegisterMaterialParser(PoiyomiConverter._SHADER_NAME, new PoiyomiParser());
		}
	}
}

#endif

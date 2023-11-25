
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
				var metallicValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "metallic");
				var smoothnessValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "smoothness");
				var roughnessValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "roughness");
				var reflectionValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "reflection");
				var specularValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "specular");

				var channelMetallic = metallicValue != null ? new ImageChannelSetup.ImageChannel(metallicValue[0], false) : ImageChannelSetup.ImageChannel.Empty();

				var channelSmoothness = ImageChannelSetup.ImageChannel.Empty();
				if(smoothnessValue != null) channelSmoothness = new ImageChannelSetup.ImageChannel(smoothnessValue[0], false);
				else if(roughnessValue != null) channelSmoothness = new ImageChannelSetup.ImageChannel(roughnessValue[0], true);

				var channelReflection = ImageChannelSetup.ImageChannel.Empty();
				if(reflectionValue != null) channelReflection = new ImageChannelSetup.ImageChannel(reflectionValue[0], false);
				else if(metallicValue != null) channelReflection = new ImageChannelSetup.ImageChannel(metallicValue[0], false);

				var channelSpecular = ImageChannelSetup.ImageChannel.Empty();
				if(specularValue != null) channelSpecular = new ImageChannelSetup.ImageChannel(specularValue[0], false);
				else if(smoothnessValue != null) channelSpecular = new ImageChannelSetup.ImageChannel(smoothnessValue[0], false);
				else if(roughnessValue != null) channelSpecular = new ImageChannelSetup.ImageChannel(roughnessValue[0], true);

				var imageChannels = new ImageChannelSetup(
					channelMetallic,
					channelSmoothness,
					channelReflection,
					channelSpecular
				);
				MaterialConverterUtil.AssembleTextureChannels(State, imageChannels, ret, "_MochieMetallicMaps");
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


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

		public string ConvertPropertyPath(string MTFProperty, UnityEngine.Material UnityMaterial)
		{
			throw new NotImplementedException();
		}

		public UnityEngine.Material ConvertToUnityMaterial(IMaterialConvertState State, Material MTFMaterial, UnityEngine.Material ExistingUnityMaterial = null)
		{
			var ret = new UnityEngine.Material(Shader.Find(ShaderName));
			ret.name = ExistingUnityMaterial != null ? ExistingUnityMaterial.name : MTFMaterial.name + "_Converted";

			MaterialConverterUtil.SetTextureProperty(MTFMaterial, ret, "albedo", "_MainTex");
			MaterialConverterUtil.SetColorProperty(MTFMaterial, ret, "albedo", "_Color");

			MaterialConverterUtil.SetTextureProperty(MTFMaterial, ret, "normal", "_BumpMap");

			{
				var metallicValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "metallic");
				var smoothnessValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "smoothness");
				var roughnessValue = MaterialConverterUtil.FindPropertyValues(MTFMaterial, "roughness");

				var channelMetallic = metallicValue != null ? new ImageChannelSetup.ImageChannel(metallicValue[0], false) : ImageChannelSetup.ImageChannel.Empty();

				var channelSmoothnes = ImageChannelSetup.ImageChannel.Empty();
				if(smoothnessValue != null) channelSmoothnes = new ImageChannelSetup.ImageChannel(smoothnessValue[0], false);
				else if(roughnessValue != null) channelSmoothnes = new ImageChannelSetup.ImageChannel(roughnessValue[0], true);

				var imageChannels = new ImageChannelSetup(
					channelMetallic,
					ImageChannelSetup.ImageChannel.Empty(),
					ImageChannelSetup.ImageChannel.Empty(),
					channelSmoothnes
				);
				MaterialConverterUtil.AssembleTextureChannels(State, imageChannels, ret, "_MetallicGlossMap");
			}

			MaterialConverterUtil.SetFloatProperty(MTFMaterial, ret, "specular", "_SpecularHighlights");
			return ret;
		}
	}

	public class StandardParser : IMaterialParser
	{
		public string ShaderName {get => StandardConverter._SHADER_NAME;}

		public string ConvertPropertyPath(string UnityProperty, UnityEngine.Material UnityMaterial)
		{
			throw new NotImplementedException();
		}

		public Material ParseFromUnityMaterial(IMaterialParseState State, UnityEngine.Material UnityMaterial, Material ExistingMTFMaterial = null)
		{
			var ret = ExistingMTFMaterial != null ? ExistingMTFMaterial : ScriptableObject.CreateInstance<Material>();
			ret.name = ExistingMTFMaterial != null ? ExistingMTFMaterial.name : UnityMaterial.name;
			ret.PreferedShaderPerTarget.Add(new Material.ShaderTarget{Platform = "unity3d", Shaders = new List<string>{ShaderName}});
			ret.StyleHints.Add(new Material.StyleHint { Name = "style", Value = "realistic" });

			MaterialParserUtil.ParseTextureProperty(State, UnityMaterial, ret, "albedo", "_MainTex");
			MaterialParserUtil.ParseColorProperty(State, UnityMaterial, ret, "albedo", "_Color");
			
			MaterialParserUtil.ParseTextureProperty(State, UnityMaterial, ret, "normal", "_BumpMap");
			
			MaterialParserUtil.ParseTextureChannelProperty(State, UnityMaterial, ret, "metallic", 0, "_MetallicGlossMap");
			MaterialParserUtil.ParseTextureChannelProperty(State, UnityMaterial, ret, "smoothness", 4, "_MetallicGlossMap");
			MaterialParserUtil.ParseFloatProperty(State, UnityMaterial, ret, "specular", "_SpecularHighlights");
			return ret;
		}
	}
}

#endif

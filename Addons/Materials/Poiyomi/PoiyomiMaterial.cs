
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using UnityEngine;
using UnityEditor;

namespace MTF.Addons
{
	public class PoiyomiConverter : IMaterialConverter
	{
		public const string _SHADER_NAME = ".poiyomi/Poiyomi 8.1/Poiyomi Toon";
		public string ShaderName {get => _SHADER_NAME;}
		public UnityEngine.Material ConvertToUnityMaterial(Material MTFMaterial, UnityEngine.Material ExistingUnityMaterial = null)
		{
			var ret = ExistingUnityMaterial != null ? ExistingUnityMaterial : new UnityEngine.Material(Shader.Find(ShaderName));
			ret.name = ExistingUnityMaterial != null ? ExistingUnityMaterial.name : MTFMaterial.name;

			MaterialConverterUtil.SetTextureProperty(MTFMaterial, ret, "albedo", "_MainTex");
			MaterialConverterUtil.SetColorProperty(MTFMaterial, ret, "albedo", "_Color");
			MaterialConverterUtil.SetTextureProperty(MTFMaterial, ret, "normal", "_BumpMap");

			var pbrChannels = new List<TextureChannelPropertyValue>();
			{
				var property = MTFMaterial.Properties.Find(p => p.Type == "metallic");
				if(property != null) foreach(var value in property.Values)
				{
					switch(value.Type)
					{
						case TextureChannelPropertyValue._TYPE: pbrChannels.Add((TextureChannelPropertyValue)value); break;
					}
				}
			}
			{
				var property = MTFMaterial.Properties.Find(p => p.Type == "smoothness");
				if(property != null) foreach(var value in property.Values)
				{
					switch(value.Type)
					{
						case TextureChannelPropertyValue._TYPE: pbrChannels.Add((TextureChannelPropertyValue)value); break;
					}
				}
			}
			{
				var property = MTFMaterial.Properties.Find(p => p.Type == "reflection");
				if(property != null) foreach(var value in property.Values)
				{
					switch(value.Type)
					{
						case TextureChannelPropertyValue._TYPE: pbrChannels.Add((TextureChannelPropertyValue)value); break;
					}
				}
			}
			{
				var property = MTFMaterial.Properties.Find(p => p.Type == "specular");
				if(property != null) foreach(var value in property.Values)
				{
					switch(value.Type)
					{
						case TextureChannelPropertyValue._TYPE: pbrChannels.Add((TextureChannelPropertyValue)value); break;
					}
				}
			}
			// assemble texture
			if(pbrChannels.Count > 0)
			{
				//var assembledTexture = new Texture2D();
				//ret.SetTexture("_BumpMap", assembledTexture);
				ret.SetFloat("_MochieBRDF", 1);
				ret.SetFloat("_MochieMetallicMultiplier", 1);
			}
			return ret;
		}
	}

	public class PoiyomiParser : IMaterialParser
	{
		public string ShaderName {get => PoiyomiConverter._SHADER_NAME;}
		public Material ParseFromUnityMaterial(UnityEngine.Material UnityMaterial, Material ExistingMTFMaterial = null)
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

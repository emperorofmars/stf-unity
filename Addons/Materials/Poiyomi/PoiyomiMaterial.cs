
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
			{
				var property = MTFMaterial.Properties.Find(p => p.Type == "albedo");
				if(property != null) foreach(var value in property.Values)
				{
					switch(value.Type)
					{
						case TexturePropertyValue._TYPE: ret.SetTexture("_MainTex", ((TexturePropertyValue)value).Texture); break;
						case ColorPropertyValue._TYPE: ret.SetColor("_Color", ((ColorPropertyValue)value).Color); break;
					}
				}
			}
			{
				var property = MTFMaterial.Properties.Find(p => p.Type == "normal");
				if(property != null) foreach(var value in property.Values)
				{
					switch(value.Type)
					{
						case TexturePropertyValue._TYPE: ret.SetTexture("_BumpMap", ((TexturePropertyValue)value).Texture); break;
					}
				}
			}
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
			ret.PreferedShaderPerTarget.Add(new Material.ShaderTarget{Platform = "unity3d", Shaders = new List<string>{ShaderName}});
			//ret.StyleHints.Add("realistic");
			
			{
				var values = new List<IPropertyValue> {
						UnityMaterial.HasProperty("_MainTex") ? new TexturePropertyValue{Texture = (Texture2D)UnityMaterial.GetTexture("_MainTex")} : null,
						UnityMaterial.HasProperty("_Color") ? new ColorPropertyValue{Color = (Color)UnityMaterial.GetColor("_Color")} : null,
				}.FindAll(e => e != null);
				if(values.Count > 0) ret.Properties.Add(new Material.Property { Type = "albedo", Values = values});
			}
			{
				var values = new List<IPropertyValue> {
						UnityMaterial.HasProperty("_BumpMap") ? new TexturePropertyValue{Texture = (Texture2D)UnityMaterial.GetTexture("_BumpMap")} : null,
				}.FindAll(e => e != null);
				if(values.Count > 0) ret.Properties.Add(new Material.Property { Type = "normal", Values = values});
			}

			{
				var values = new List<IPropertyValue> {
						UnityMaterial.HasProperty("_MochieMetallicMaps") ? new TextureChannelPropertyValue{Texture = (Texture2D)UnityMaterial.GetTexture("_MochieMetallicMaps"), Channel = 0} : null,
				}.FindAll(e => e != null);
				if(values.Count > 0) ret.Properties.Add(new Material.Property { Type = "metallic", Values = values});
			}
			{
				var values = new List<IPropertyValue> {
						UnityMaterial.HasProperty("_MochieMetallicMaps") ? new TextureChannelPropertyValue{Texture = (Texture2D)UnityMaterial.GetTexture("_MochieMetallicMaps"), Channel = 1} : null,
				}.FindAll(e => e != null);
				if(values.Count > 0) ret.Properties.Add(new Material.Property { Type = "smoothness", Values = values});
			}
			{
				var values = new List<IPropertyValue> {
						UnityMaterial.HasProperty("_MochieMetallicMaps") ? new TextureChannelPropertyValue{Texture = (Texture2D)UnityMaterial.GetTexture("_MochieMetallicMaps"), Channel = 2} : null,
				}.FindAll(e => e != null);
				if(values.Count > 0) ret.Properties.Add(new Material.Property { Type = "reflection", Values = values});
			}
			{
				var values = new List<IPropertyValue> {
						UnityMaterial.HasProperty("_MochieMetallicMaps") ? new TextureChannelPropertyValue{Texture = (Texture2D)UnityMaterial.GetTexture("_MochieMetallicMaps"), Channel = 3} : null,
				}.FindAll(e => e != null);
				if(values.Count > 0) ret.Properties.Add(new Material.Property { Type = "specular", Values = values});
			}
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

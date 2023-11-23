using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using UnityEngine;

namespace MTF
{
	public class StandardConverter : IMaterialConverter
	{
		public const string _SHADER_NAME = "Standard";
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
			{
				var property = MTFMaterial.Properties.Find(p => p.Type == "specular");
				if(property != null) foreach(var value in property.Values)
				{
					switch(value.Type)
					{
						case FloatPropertyValue._TYPE: ret.SetFloat("_SpecularHighlights", ((FloatPropertyValue)value).Value); break;
					}
				}
			}
			return ret;
		}
	}

	public class StandardParser : IMaterialParser
	{
		public string ShaderName {get => StandardConverter._SHADER_NAME;}
		public Material ParseFromUnityMaterial(UnityEngine.Material UnityMaterial, Material ExistingMTFMaterial = null)
		{
			var ret = ExistingMTFMaterial != null ? ExistingMTFMaterial : ScriptableObject.CreateInstance<Material>();
			ret.PreferedShaderPerTarget.Add(new Material.ShaderTarget{Platform = "unity3d", Shaders = new List<string>{ShaderName}});
			ret.StyleHints.Add("realistic");

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
						UnityMaterial.HasProperty("_SpecularHighlights") ? new FloatPropertyValue{Value = (float)UnityMaterial.GetFloat("_SpecularHighlights")} : null,
				}.FindAll(e => e != null);
				if(values.Count > 0) ret.Properties.Add(new Material.Property { Type = "specular", Values = values});
			}
			return ret;
		}
	}
}
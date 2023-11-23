using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using UnityEngine;

namespace MTF
{
	public class MaterialConverterUtil
	{
		public static bool ConvertValue(UnityEngine.Material UnityMaterial, object Value, string UnityPropertyName, string UnityPropertyType)
		{
			switch(UnityPropertyType)
			{
				case TexturePropertyValue._TYPE:
					UnityMaterial.SetTexture(UnityPropertyName, (Texture2D)Value);
					return true;
				case ColorPropertyValue._TYPE:
					UnityMaterial.SetColor(UnityPropertyName, (Color)Value);
					return true;
				case FloatPropertyValue._TYPE:
					UnityMaterial.SetFloat(UnityPropertyName, (float)Value);
					return true;
			}
			return false;
		}

		/* ValueOrder: List<(MTF value-type, Unity property-name, Optional Override Unity Property Type)> */
		public static void ConvertProperty(Material MTFMaterial, UnityEngine.Material UnityMaterial, string MTFPropertyType, List<(string, string, string)> ValueOrder)
		{
			var property = MTFMaterial.Properties.Find(p => p.Type == MTFPropertyType);
			if(property != null)
			{
				foreach(var valueMapping in ValueOrder)
				{
					var value = property.Values.Find(v => v.Type == valueMapping.Item1);
					if(value != null)
					{
						var UsedUnityPropertyType = valueMapping.Item3 != null ? valueMapping.Item3 : value.Type;
						switch(value.Type)
						{
							case TexturePropertyValue._TYPE: if(ConvertValue(UnityMaterial, ((TexturePropertyValue)value).Texture, valueMapping.Item2, UsedUnityPropertyType)) {return;} else {break;}
							case ColorPropertyValue._TYPE: if(ConvertValue(UnityMaterial, ((ColorPropertyValue)value).Color, valueMapping.Item2, UsedUnityPropertyType)) {return;} else {break;}
							case FloatPropertyValue._TYPE: if(ConvertValue(UnityMaterial, ((FloatPropertyValue)value).Value, valueMapping.Item2, UsedUnityPropertyType)) {return;} else {break;}
						}
					}
				}
			}
		}

		public static object RetrieveValue(UnityEngine.Material UnityMaterial, string UnityPropertyName, string UnityPropertyType)
		{
			switch(UnityPropertyType)
			{
				case TexturePropertyValue._TYPE:
					return UnityMaterial.GetTexture(UnityPropertyName);
				case ColorPropertyValue._TYPE:
					return UnityMaterial.GetColor(UnityPropertyName);
				case FloatPropertyValue._TYPE:
					return UnityMaterial.GetFloat(UnityPropertyName);
			}
			return default;
		}

		/* ValueOrder: List<(MTF value-type, Unity property-name, Optional Override Unity Property Type)> */
		public static void ParseProperty(UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, List<(string, string, string)> ValueOrder)
		{
			var property = MTFMaterial.Properties.FirstOrDefault(p => p.Type == MTFPropertyType);
			if(property == null)
			{
				property = new Material.Property {Type = MTFPropertyType};
				MTFMaterial.Properties.Add(property);
			}
			foreach(var valueMapping in ValueOrder)
			{
				var UsedUnityPropertyType = valueMapping.Item3 != null ? valueMapping.Item3 : valueMapping.Item1;
				if(UnityMaterial.HasProperty(valueMapping.Item2))
				{
					switch(valueMapping.Item1)
					{
						case TexturePropertyValue._TYPE:
							var texture = (Texture2D)RetrieveValue(UnityMaterial, valueMapping.Item2, UsedUnityPropertyType);
							if(texture != null) property.Values.Add(new TexturePropertyValue {Texture = texture});
							break;
						case ColorPropertyValue._TYPE:
							var color = (Color)RetrieveValue(UnityMaterial, valueMapping.Item2, UsedUnityPropertyType);
							if(color != null) property.Values.Add(new ColorPropertyValue {Color = color});
							break;
						case FloatPropertyValue._TYPE:
							property.Values.Add(new FloatPropertyValue {Value = (float)RetrieveValue(UnityMaterial, valueMapping.Item2, UsedUnityPropertyType)});
							break;
					}
				}
			}
		}
	}

	public class StandardConverter : IMaterialConverter
	{
		public const string _SHADER_NAME = "Standard";
		public string ShaderName {get => _SHADER_NAME;}
		public UnityEngine.Material ConvertToUnityMaterial(Material MTFMaterial, UnityEngine.Material ExistingUnityMaterial = null)
		{
			var ret = ExistingUnityMaterial != null ? ExistingUnityMaterial : new UnityEngine.Material(Shader.Find(_SHADER_NAME));

			MaterialConverterUtil.ConvertProperty(MTFMaterial, ret, "albedo", new List<(string, string, string)> {
				(TexturePropertyValue._TYPE, "_MainTex", null),
				(ColorPropertyValue._TYPE, "_Color", null),
			});
			MaterialConverterUtil.ConvertProperty(MTFMaterial, ret, "normal", new List<(string, string, string)> {
				(TexturePropertyValue._TYPE, "_BumpMap", null),
			});
			MaterialConverterUtil.ConvertProperty(MTFMaterial, ret, "specular", new List<(string, string, string)> {
				(FloatPropertyValue._TYPE, "_SpecularHighlights", null)
			});
			return ret;
		}
	}

	public class StandardParser : IMaterialParser
	{
		public string ShaderName {get => StandardConverter._SHADER_NAME;}
		public Material ParseFromUnityMaterial(UnityEngine.Material UnityMaterial, Material ExistingMTFMaterial = null)
		{
			var ret = ExistingMTFMaterial != null ? ExistingMTFMaterial : ScriptableObject.CreateInstance<Material>();
			ret.PreferedShaderPerTarget.Add(new Material.ShaderTarget{Platform = "unity3d", Shaders = new List<string>{StandardConverter._SHADER_NAME}});
			ret.StyleHints.Add("realistic");
			
			MaterialConverterUtil.ParseProperty(UnityMaterial, ret, "albedo", new List<(string, string, string)> {
				(TexturePropertyValue._TYPE, "_MainTex", null),
				(ColorPropertyValue._TYPE, "_Color", null),
				(FloatPropertyValue._TYPE, "_SpecularHighlights", null)
			});
			MaterialConverterUtil.ParseProperty(UnityMaterial, ret, "normal", new List<(string, string, string)> {
				(TexturePropertyValue._TYPE, "_BumpMap", null),
			});
			MaterialConverterUtil.ParseProperty(UnityMaterial, ret, "specular", new List<(string, string, string)> {
				(FloatPropertyValue._TYPE, "_SpecularHighlights", null)
			});
			return ret;
		}
	}
}
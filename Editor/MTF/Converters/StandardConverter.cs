using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using UnityEngine;

namespace MTF
{
	public class MaterialConverterUtil
	{
		public static bool ConvertValue(UnityEngine.Material UnityMaterial, IPropertyValue Value, string UnityPropertyName, string UnityPropertyType = null)
		{
			var UsedUnityPropertyType = UnityPropertyType != null ? UnityPropertyType : Value.Type;
			switch(UsedUnityPropertyType)
			{
				case TexturePropertyValue._TYPE:
					UnityMaterial.SetTexture(UnityPropertyName, ((TexturePropertyValue)Value).Texture);
					return true;
				case ColorPropertyValue._TYPE:
					UnityMaterial.SetColor(UnityPropertyName, ((ColorPropertyValue)Value).Color);
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
						switch(value.Type)
						{
							case TexturePropertyValue._TYPE: if(ConvertValue(UnityMaterial, value, valueMapping.Item2, valueMapping.Item3)) {return;} else {break;}
							case ColorPropertyValue._TYPE: if(ConvertValue(UnityMaterial, value, valueMapping.Item2, valueMapping.Item3)) {return;} else {break;}
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
			}
			return default;
		}
		
		public static void ParseValue(UnityEngine.Material UnityMaterial, Material.Property MTFProperty, string MTFValueType, string UnityPropertyName, string UnityPropertyType = null)
		{
			var UsedUnityPropertyType = UnityPropertyType != null ? UnityPropertyType : MTFValueType;
			switch(MTFValueType)
			{
				case TexturePropertyValue._TYPE:
					MTFProperty.Values.Add(new TexturePropertyValue {Texture = (Texture2D)RetrieveValue(UnityMaterial, UnityPropertyName, UsedUnityPropertyType)});
					break;
				case ColorPropertyValue._TYPE:
					MTFProperty.Values.Add(new ColorPropertyValue {Color = (Color)RetrieveValue(UnityMaterial, UnityPropertyName, UsedUnityPropertyType)});
					break;
			}
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
				var MTFValueType = valueMapping.Item1;
				var UnityPropertyName = valueMapping.Item2;
				var UnityPropertyType = valueMapping.Item3;
				
				var UsedUnityPropertyType = UnityPropertyType != null ? UnityPropertyType : MTFValueType;
				switch(MTFValueType)
				{
					case TexturePropertyValue._TYPE:
						var texture = (Texture2D)RetrieveValue(UnityMaterial, UnityPropertyName, UsedUnityPropertyType);
						if(texture != null) property.Values.Add(new TexturePropertyValue {Texture = texture});
						break;
					case ColorPropertyValue._TYPE:
						property.Values.Add(new ColorPropertyValue {Color = (Color)RetrieveValue(UnityMaterial, UnityPropertyName, UsedUnityPropertyType)});
						var color = (Color)RetrieveValue(UnityMaterial, UnityPropertyName, UsedUnityPropertyType);
						if(color != null) property.Values.Add(new ColorPropertyValue {Color = color});
						break;
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
			});
			return ret;
		}
	}
}
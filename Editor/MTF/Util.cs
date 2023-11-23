
using System;
using UnityEngine;

namespace MTF
{
	public class MaterialConverterUtil
	{
		public static bool SetProperty(Material MTFMaterial, UnityEngine.Material UnityMaterial, string MTFPropertyType, string MTFValueType, string UnityPropertyName)
		{
			var property = MTFMaterial.Properties.Find(p => p.Type == MTFPropertyType);
			if(property != null) foreach(var value in property.Values)
			{
				if(value.Type != MTFValueType) continue;
				switch(MTFValueType)
				{
					case TexturePropertyValue._TYPE: UnityMaterial.SetTexture(UnityPropertyName, ((TexturePropertyValue)value).Texture); return true;
					case ColorPropertyValue._TYPE: UnityMaterial.SetColor(UnityPropertyName, ((ColorPropertyValue)value).Color); return true;
					case FloatPropertyValue._TYPE: UnityMaterial.SetFloat(UnityPropertyName, ((FloatPropertyValue)value).Value); return true;
				}
			}
			return false;
		}
		public static bool SetTextureProperty(Material MTFMaterial, UnityEngine.Material UnityMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			var property = MTFMaterial.Properties.Find(p => p.Type == MTFPropertyType);
			if(property != null) foreach(var value in property.Values)
			{
				if(value.Type != TexturePropertyValue._TYPE) continue;
				UnityMaterial.SetTexture(UnityPropertyName, ((TexturePropertyValue)value).Texture);
				return true;
			}
			return false;
		}
		public static bool SetColorProperty(Material MTFMaterial, UnityEngine.Material UnityMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			var property = MTFMaterial.Properties.Find(p => p.Type == MTFPropertyType);
			if(property != null) foreach(var value in property.Values)
			{
				if(value.Type != ColorPropertyValue._TYPE) continue;
				UnityMaterial.SetColor(UnityPropertyName, ((ColorPropertyValue)value).Color);
				return true;
			}
			return false;
		}
		public static bool SetFloatProperty(Material MTFMaterial, UnityEngine.Material UnityMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			var property = MTFMaterial.Properties.Find(p => p.Type == MTFPropertyType);
			if(property != null) foreach(var value in property.Values)
			{
				if(value.Type != FloatPropertyValue._TYPE) continue;
				UnityMaterial.SetFloat(UnityPropertyName, ((FloatPropertyValue)value).Value);
				return true;
			}
			return false;
		}
	}

	public class MaterialParserUtil
	{
		private static Material.Property _EnsureProperty(Material MTFMaterial, string MTFPropertyType)
		{
			var property = MTFMaterial.Properties.Find(p => p.Type == MTFPropertyType);
			if(property == null)
			{
				property = new Material.Property { Type = MTFPropertyType };
				MTFMaterial.Properties.Add(property);
			}
			return property;
		}

		public static bool ParseProperty(UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, string MTFValueType, string UnityPropertyName)
		{
			if(UnityMaterial.HasProperty(UnityPropertyName))
			{
				var property = _EnsureProperty(MTFMaterial, MTFPropertyType);
				switch(MTFValueType)
				{
					case TexturePropertyValue._TYPE: property.Values.Add(new TexturePropertyValue{Texture = (Texture2D)UnityMaterial.GetTexture(UnityPropertyName)}); return true;
					case ColorPropertyValue._TYPE: property.Values.Add(new ColorPropertyValue{Color = UnityMaterial.GetColor(UnityPropertyName)}); return true;
					case FloatPropertyValue._TYPE: property.Values.Add(new FloatPropertyValue{Value = UnityMaterial.GetFloat(UnityPropertyName)}); return true;
				}
			}
			return false;
		}

		public static bool ParseTextureProperty(UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			if(UnityMaterial.HasProperty(UnityPropertyName))
			{
				_EnsureProperty(MTFMaterial, MTFPropertyType).Values.Add(new TexturePropertyValue{Texture = (Texture2D)UnityMaterial.GetTexture(UnityPropertyName)});
				return true;
			}
			return false;
		}

		public static bool ParseTextureChannelProperty(UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, int Channel, string UnityPropertyName)
		{
			if(UnityMaterial.HasProperty(UnityPropertyName))
			{
				_EnsureProperty(MTFMaterial, MTFPropertyType).Values.Add(new TextureChannelPropertyValue{Texture = (Texture2D)UnityMaterial.GetTexture(UnityPropertyName), Channel = Channel });
				return true;
			}
			return false;
		}
		
		public static bool ParseColorProperty(UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			if(UnityMaterial.HasProperty(UnityPropertyName))
			{
				_EnsureProperty(MTFMaterial, MTFPropertyType).Values.Add(new ColorPropertyValue{Color = UnityMaterial.GetColor(UnityPropertyName)});
				return true;
			}
			return false;
		}
		
		public static bool ParseFloatProperty(UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			if(UnityMaterial.HasProperty(UnityPropertyName))
			{
				_EnsureProperty(MTFMaterial, MTFPropertyType).Values.Add(new FloatPropertyValue{Value = UnityMaterial.GetFloat(UnityPropertyName)});
				return true;
			}
			return false;
		}
	}
}
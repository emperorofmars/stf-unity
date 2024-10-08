
using System.Collections.Generic;
using System.Drawing;
using MTF.PropertyValues;
using UnityEngine;

namespace MTF
{
	public static class MaterialConverterUtil
	{
		public static bool HasProperty(Material MTFMaterial, string MTFPropertyType)
		{
			return MTFMaterial.Properties.Find(p => p.Type == MTFPropertyType) != null;
		}

		public static List<IPropertyValue> FindPropertyValues(Material MTFMaterial, string MTFPropertyType)
		{
			return MTFMaterial.Properties.Find(p => p.Type == MTFPropertyType)?.Values;
		}

		public static IPropertyValue FindPropertyValue(Material MTFMaterial, string MTFPropertyType, string MTFValueType)
		{
			var property = MTFMaterial.Properties.Find(p => p.Type == MTFPropertyType);
			if(property != null) foreach(var value in property.Values)
			{
				if(value.Type == MTFValueType) return value;
			}
			return null;
		}

		public static bool SetTextureProperty(Material MTFMaterial, UnityEngine.Material UnityMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			var value = FindPropertyValue(MTFMaterial, MTFPropertyType, TexturePropertyValue._TYPE);
			if(value != null)
			{
				UnityMaterial.SetTexture(UnityPropertyName, ((TexturePropertyValue)value).Texture);
				return true;
			}
			return false;
		}
		public static bool SetColorProperty(Material MTFMaterial, UnityEngine.Material UnityMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			var value = FindPropertyValue(MTFMaterial, MTFPropertyType, ColorPropertyValue._TYPE);
			if(value != null)
			{
				UnityMaterial.SetColor(UnityPropertyName, ((ColorPropertyValue)value).Color);
				return true;
			}
			return false;
		}
		public static bool SetFloatProperty(Material MTFMaterial, UnityEngine.Material UnityMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			var value = FindPropertyValue(MTFMaterial, MTFPropertyType, FloatPropertyValue._TYPE);
			if(value != null)
			{
				UnityMaterial.SetFloat(UnityPropertyName, ((FloatPropertyValue)value).Value);
				return true;
			}
			return false;
		}

		public static bool AssembleTextureChannels(IMaterialConvertState State, ImageChannelSetup Channels, UnityEngine.Material UnityMaterial, string UnityPropertyName)
		{
			// check if these are texture channel property values pointing to the same texture
			var isSameTexture = true;
			Texture2D originalTexture = null;
			for(int i = 0; i < 4; i++)
			{
				if(Channels[i].Source == null) { isSameTexture = false; break; }
				else if(Channels[i].Source.Type != TextureChannelPropertyValue._TYPE) { isSameTexture = false; break; }
				else if(((TextureChannelPropertyValue)Channels[i].Source).Channel != i) { isSameTexture = false; break; }
				else if(originalTexture == null) originalTexture = ((TextureChannelPropertyValue)Channels[i].Source).Texture;
				else if(originalTexture != ((TextureChannelPropertyValue)Channels[i].Source).Texture) { isSameTexture = false; break; }
			}
			if(isSameTexture)
			{
				UnityMaterial.SetTexture(UnityPropertyName, originalTexture);
				return true;
			}

			var finalTexture = ImageUtil.AssembleTextureChannels(Channels);
			finalTexture.name = UnityPropertyName;
			finalTexture = State.SaveImageResource(finalTexture.EncodeToPNG(), UnityPropertyName, "png");
			UnityMaterial.SetTexture(UnityPropertyName, finalTexture);
			return true;
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

		public static bool ParseTextureProperty(IMaterialParseState State, UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			if(UnityMaterial.HasProperty(UnityPropertyName) && UnityMaterial.GetTexture(UnityPropertyName) != null)
			{
				var prop = ScriptableObject.CreateInstance<TexturePropertyValue>();
				prop.Texture = (Texture2D)UnityMaterial.GetTexture(UnityPropertyName);
				//_EnsureProperty(MTFMaterial, MTFPropertyType).Values.Add(prop);
				State.SavePropertyValue(prop, _EnsureProperty(MTFMaterial, MTFPropertyType), MTFMaterial);
				return true;
			}
			return false;
		}

		public static bool ParseTextureChannelProperty(IMaterialParseState State, UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, int Channel, string UnityPropertyName)
		{
			if(UnityMaterial.HasProperty(UnityPropertyName) && UnityMaterial.GetTexture(UnityPropertyName) != null)
			{
				var prop = ScriptableObject.CreateInstance<TextureChannelPropertyValue>();
				prop.Texture = (Texture2D)UnityMaterial.GetTexture(UnityPropertyName);
				prop.Channel = Channel;
				//_EnsureProperty(MTFMaterial, MTFPropertyType).Values.Add(prop);
				State.SavePropertyValue(prop, _EnsureProperty(MTFMaterial, MTFPropertyType), MTFMaterial);
				return true;
			}
			return false;
		}
		
		public static bool ParseColorProperty(IMaterialParseState State, UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			if(UnityMaterial.HasProperty(UnityPropertyName) && UnityMaterial.GetColor(UnityPropertyName) != null)
			{
				var prop = ScriptableObject.CreateInstance<ColorPropertyValue>();
				prop.Color = UnityMaterial.GetColor(UnityPropertyName);
				//_EnsureProperty(MTFMaterial, MTFPropertyType).Values.Add(prop);
				State.SavePropertyValue(prop, _EnsureProperty(MTFMaterial, MTFPropertyType), MTFMaterial);
				return true;
			}
			return false;
		}
		
		public static bool ParseFloatProperty(IMaterialParseState State, UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, string UnityPropertyName)
		{
			if(UnityMaterial.HasProperty(UnityPropertyName))
			{
				var prop = ScriptableObject.CreateInstance<FloatPropertyValue>();
				prop.Value = UnityMaterial.GetFloat(UnityPropertyName);
				//_EnsureProperty(MTFMaterial, MTFPropertyType).Values.Add(prop);
				State.SavePropertyValue(prop, _EnsureProperty(MTFMaterial, MTFPropertyType), MTFMaterial);
				return true;
			}
			return false;
		}
	}
}

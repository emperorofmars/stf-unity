
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace MTF
{
	public class MaterialConverterUtil
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

		// This is very ugly, slow and error prone, TODO: unfuck this, use a proper lib
		/* Channels: List of Property-values, invert; 4 channels hard required */
		public static bool AssembleTextureChannels(List<(List<IPropertyValue>, bool)> Channels, UnityEngine.Material UnityMaterial, string UnityPropertyName, string SavePath)
		{
			// check if these are texture channel property values pointing to the same texture
			var isSameTexture = true;
			Texture2D originalTexture = null;
			for(int i = 0; i < Channels.Count; i++)
			{
				if(Channels[i].Item1 == null) { isSameTexture = false; break; }
				else if(Channels[i].Item1.Count == 0) { isSameTexture = false; break; }
				else if(Channels[i].Item1[0].Type != TextureChannelPropertyValue._TYPE) { isSameTexture = false; break; }
				else if(((TextureChannelPropertyValue)Channels[i].Item1[0]).Channel != i) { isSameTexture = false; break; }
				else if(originalTexture == null) originalTexture = ((TextureChannelPropertyValue)Channels[i].Item1[0]).Texture;
				else if(originalTexture != ((TextureChannelPropertyValue)Channels[i].Item1[0]).Texture) { isSameTexture = false; break; }
			}
			if(isSameTexture)
			{
				Debug.Log("SAME TEXTURE");
				UnityMaterial.SetTexture(UnityPropertyName, originalTexture);
				return true;
			}

			// assemble combined texture
			var parsedChannels = new List<(Bitmap, int, bool)>();
			foreach(var channel in Channels)
			{
				if(channel.Item1 != null)
				{
					foreach(var propertyValue in channel.Item1)
					{
						var parsed = false;
						switch(propertyValue.Type)
						{
							case TexturePropertyValue._TYPE:
								var textureProperty = (TexturePropertyValue)propertyValue;
								// TODO: check if asset exists in filesystem, otherwise take from buffer
								var texturePropertyPath = AssetDatabase.GetAssetPath(textureProperty.Texture);
								parsedChannels.Add((new Bitmap(texturePropertyPath), -1, channel.Item2));
								parsed = true;
								break;
							case TextureChannelPropertyValue._TYPE:
								var textureChannelProperty = (TextureChannelPropertyValue)propertyValue;
								// TODO: check if asset exists in filesystem, otherwise take from buffer
								var textureChannelPropertyPath = AssetDatabase.GetAssetPath(textureChannelProperty.Texture);
								parsedChannels.Add((new Bitmap(textureChannelPropertyPath), textureChannelProperty.Channel, channel.Item2));
								parsed = true;
								break;
							// also handle color and other property types
						}
						if(parsed) break;
					}
				}
			}
			var largestResolution = new Size();
			foreach(var channel in parsedChannels)
			{
				if(channel.Item1 != null)
				{
					if(channel.Item1.Width > largestResolution.Width) largestResolution.Width = channel.Item1.Width;
					if(channel.Item1.Height > largestResolution.Height) largestResolution.Height = channel.Item1.Height;
				}
			}
			var convertedChannels = new List<(Bitmap, int, bool)>();
			// convert to the largest size
			foreach(var channel in parsedChannels)
			{
				if(channel.Item1 != null) convertedChannels.Add((new Bitmap(channel.Item1, largestResolution), channel.Item2, channel.Item3));
				else convertedChannels.Add((null, -1, false));
			}
			var ret = new Bitmap(largestResolution.Width, largestResolution.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			for(int x = 0; x < largestResolution.Width; x++)
			{
				for(int y = 0; y < largestResolution.Height; y++)
				{
					var color = new int[4];
					for(int c = 0; c < convertedChannels.Count; c++)
					{
						if(convertedChannels[c].Item1 != null) color[c] = GetColorChannel(convertedChannels[c].Item1.GetPixel(x, y), convertedChannels[c].Item2, convertedChannels[c].Item3);
						else color[c] = 0;
					}
					ret.SetPixel(x, y, System.Drawing.Color.FromArgb(color[0], color[1], color[2], color[3]));
				}
			}
			ret.Save(SavePath + ".png", System.Drawing.Imaging.ImageFormat.Png);
			AssetDatabase.Refresh();
			var finalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(SavePath + ".png");
			UnityMaterial.SetTexture(UnityPropertyName, finalTexture);
			return true;
		}

		private static int GetColorChannel(System.Drawing.Color color, int channel, bool inverted)
		{
			int ret = 0;
			if(channel == 0) ret = color.A;
			else if(channel == 1) ret = color.R;
			else if(channel == 2) ret = color.G;
			else if(channel == 3) ret = color.B;
			return inverted ? 255 - ret : ret;
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

		/*public static Bitmap GetSingleChannelTexture(Texture OriginalTexture, int Channel)
		{
			var assetPath = AssetDatabase.GetAssetPath(OriginalTexture);
			var bitmap = new Bitmap(assetPath);
			Debug.Log("AAA: " + bitmap.Width + " : " + bitmap.Height);
			var ret = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			//var ret = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			//var ret = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Alpha);
			for(int x = 0; x < bitmap.Width; x++)
			{
				for(int y = 0; y < bitmap.Height; y++)
				{
					int color = 0;
					if(Channel == 0) color = bitmap.GetPixel(x, y).A;
					if(Channel == 1) color = bitmap.GetPixel(x, y).R;
					if(Channel == 2) color = bitmap.GetPixel(x, y).G;
					if(Channel == 3) color = bitmap.GetPixel(x, y).B;
					ret.SetPixel(x, y, System.Drawing.Color.FromArgb(color, color, color, color));
				}
			}
			return ret;
		}

		public static bool ParsoIntoSingleChannelTexture(UnityEngine.Material UnityMaterial, Material MTFMaterial, string MTFPropertyType, string UnityPropertyName, int Channel, string SavePath)
		{
			if(UnityMaterial.HasProperty(UnityPropertyName))
			{
				var tex = GetSingleChannelTexture(UnityMaterial.GetTexture(UnityPropertyName), Channel);
				tex.Save(SavePath + ".png", System.Drawing.Imaging.ImageFormat.Png);
				AssetDatabase.Refresh();
				var processedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(SavePath + ".png");
				_EnsureProperty(MTFMaterial, MTFPropertyType).Values.Add(new TexturePropertyValue{Texture = processedTexture });
				return true;
			}
			return false;
		}*/
	}
}

#endif

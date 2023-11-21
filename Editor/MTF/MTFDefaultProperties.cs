using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTF
{
	[Serializable]
	public class TexturePropertyValue : IPropertyValue
	{
		public static string _TYPE = "Texture";
		public string Type => _TYPE;
		Texture2D Texture;
	}

	[Serializable]
	public class TextureChannelPropertyValue : IPropertyValue
	{
		public static string _TYPE = "TextureChannel";
		public string Type => _TYPE;
		Texture2D Texture;
		int Channel;
	}

	[Serializable]
	public class ColorPropertyValue : IPropertyValue
	{
		public static string _TYPE = "Color";
		public string Type => _TYPE;
		Color Texture;
	}

	[Serializable]
	public class AlbedoProperty : AProperty
	{
		public static string _TYPE = "Albedo";
		public override string Type => _TYPE;
	}

	[Serializable]
	public class NormalProperty : AProperty
	{
		public static string _TYPE = "Normal";
		public override string Type => _TYPE;
	}

	[Serializable]
	public class MetallicProperty : AProperty
	{
		public static string _TYPE = "Metallic";
		public override string Type => _TYPE;
	}

	[Serializable]
	public class SpecularProperty : AProperty
	{
		public static string _TYPE = "Specular";
		public override string Type => _TYPE;
	}

	public class StandardProperties
	{
		public static Dictionary<string, Type> DefaultProperties = new Dictionary<string, Type> {
			{AlbedoProperty._TYPE, typeof(AlbedoProperty)},
			{NormalProperty._TYPE, typeof(NormalProperty)},
			{MetallicProperty._TYPE, typeof(MetallicProperty)},
			{SpecularProperty._TYPE, typeof(SpecularProperty)},
		};
		public static Dictionary<string, Type> DefaultPropertyValues = new Dictionary<string, Type> {
			{TexturePropertyValue._TYPE, typeof(TexturePropertyValue)},
			{TextureChannelPropertyValue._TYPE, typeof(TextureChannelPropertyValue)},
			{ColorPropertyValue._TYPE, typeof(ColorPropertyValue)},
		};
	}
}
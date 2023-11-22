using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
	[Serializable]
	public class TexturePropertyValue : IPropertyValue
	{
		public static string _TYPE = "MTF.texture";
		public string Type => _TYPE;
		public Texture2D Texture;
	}
	public class TexturePropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IImportState State, JObject Json)
		{
			return new TexturePropertyValue { Texture = (Texture2D)State.GetResource((string)Json["value"]) };
		}
	}
	public class TexturePropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IExportState State, IPropertyValue MTFProperty)
		{
			return new JObject {{"type", TexturePropertyValue._TYPE}, {"value", State.AddResource(((TexturePropertyValue)MTFProperty).Texture)}};
		}
	}

	[Serializable]
	public class TextureChannelPropertyValue : IPropertyValue
	{
		public static string _TYPE = "MTF.texture_channel";
		public string Type => _TYPE;
		public Texture2D Texture;
		public int Channel;
	}

	[Serializable]
	public class ColorPropertyValue : IPropertyValue
	{
		public static string _TYPE = "MTF.color";
		public string Type => _TYPE;
		public Color Color;
	}
	public class ColorPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IImportState State, JObject Json)
		{
			return new ColorPropertyValue {Color = new Color((float)Json["value"][0], (float)Json["value"][1], (float)Json["value"][2])};
		}
	}
	public class ColorPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IExportState State, IPropertyValue MTFProperty)
		{
			var v = (ColorPropertyValue)MTFProperty;
			return new JObject {{"type", ColorPropertyValue._TYPE}, {"value", new JArray{v.Color.r, v.Color.g, v.Color.b}}};
		}
	}

	[Serializable]
	public class IntPropertyValue : IPropertyValue
	{
		public static string _TYPE = "MTF.int";
		public string Type => _TYPE;
		public int Value;
	}

	[Serializable]
	public class FloatPropertyValue : IPropertyValue
	{
		public static string _TYPE = "MTF.float";
		public string Type => _TYPE;
		public float Value;
	}

	[Serializable]
	public class StringPropertyValue : IPropertyValue
	{
		public static string _TYPE = "MTF.string";
		public string Type => _TYPE;
		public string Value;
	}
}
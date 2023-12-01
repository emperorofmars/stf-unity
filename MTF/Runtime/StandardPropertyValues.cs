using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
	public class TexturePropertyValue : IPropertyValue
	{
		public const string _TYPE = "MTF.texture";
		public override string Type => _TYPE;
		public Texture2D Texture;
	}
	public class TexturePropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IPropertyValueImportState State, JObject Json)
		{
			var prop = ScriptableObject.CreateInstance<TexturePropertyValue>();
			prop.Texture = (Texture2D)State.GetResource((string)Json["texture"]);
			return prop;
		}
	}
	public class TexturePropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IPropertyValueExportState State, IPropertyValue MTFProperty)
		{
			return new JObject {{"type", TexturePropertyValue._TYPE}, {"texture", State.AddResource(((TexturePropertyValue)MTFProperty).Texture)}};
		}
	}

	public class TextureChannelPropertyValue : IPropertyValue
	{
		public const string _TYPE = "MTF.texture_channel";
		public override string Type => _TYPE;
		public Texture2D Texture;
		public int Channel;
	}
	public class TextureChannelPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IPropertyValueImportState State, JObject Json)
		{
			var prop = ScriptableObject.CreateInstance<TextureChannelPropertyValue>();
			prop.Texture = (Texture2D)State.GetResource((string)Json["texture"]);
			prop.Channel = (int)Json["channel"];
			return prop;
		}
	}
	public class TextureChannelPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IPropertyValueExportState State, IPropertyValue MTFProperty)
		{
			var p = (TextureChannelPropertyValue)MTFProperty;
			return new JObject {{"type", TextureChannelPropertyValue._TYPE}, {"texture", State.AddResource(p.Texture)}, {"channel", p.Channel}};
		}
	}

	public class ColorPropertyValue : IPropertyValue
	{
		public const string _TYPE = "MTF.color";
		public override string Type => _TYPE;
		public Color Color;
	}
	public class ColorPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IPropertyValueImportState State, JObject Json)
		{
			var prop = ScriptableObject.CreateInstance<ColorPropertyValue>();
			prop.Color = new Color((float)Json["value"][0], (float)Json["value"][1], (float)Json["value"][2]);
			return prop;
		}
	}
	public class ColorPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IPropertyValueExportState State, IPropertyValue MTFProperty)
		{
			var v = (ColorPropertyValue)MTFProperty;
			return new JObject {{"type", ColorPropertyValue._TYPE}, {"value", new JArray{v.Color.r, v.Color.g, v.Color.b}}};
		}
	}

	public class IntPropertyValue : IPropertyValue
	{
		public const string _TYPE = "MTF.int";
		public override string Type => _TYPE;
		public int Value;
	}
	public class IntPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IPropertyValueImportState State, JObject Json)
		{
			var prop = ScriptableObject.CreateInstance<IntPropertyValue>();
			prop.Value = (int)Json["value"];
			return prop;
		}
	}
	public class IntPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IPropertyValueExportState State, IPropertyValue MTFProperty)
		{
			var v = (IntPropertyValue)MTFProperty;
			return new JObject {{"type", IntPropertyValue._TYPE}, {"value", v.Value}};
		}
	}

	public class FloatPropertyValue : IPropertyValue
	{
		public const string _TYPE = "MTF.float";
		public override string Type => _TYPE;
		public float Value;
	}
	public class FloatPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IPropertyValueImportState State, JObject Json)
		{
			var prop = ScriptableObject.CreateInstance<FloatPropertyValue>();
			prop.Value = (float)Json["value"];
			return prop;
		}
	}
	public class FloatPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IPropertyValueExportState State, IPropertyValue MTFProperty)
		{
			var v = (FloatPropertyValue)MTFProperty;
			return new JObject {{"type", FloatPropertyValue._TYPE}, {"value", v.Value}};
		}
	}

	public class StringPropertyValue : IPropertyValue
	{
		public const string _TYPE = "MTF.string";
		public override string Type => _TYPE;
		public string Value;
	}
	public class StringPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IPropertyValueImportState State, JObject Json)
		{
			var prop = ScriptableObject.CreateInstance<StringPropertyValue>();
			prop.Value = (string)Json["value"];
			return prop;
		}
	}
	public class StringPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IPropertyValueExportState State, IPropertyValue MTFProperty)
		{
			var v = (StringPropertyValue)MTFProperty;
			return new JObject {{"type", StringPropertyValue._TYPE}, {"value", v.Value}};
		}
	}
}
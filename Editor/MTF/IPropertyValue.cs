using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
	public interface IPropertyValue
	{
		string Type {get;}
	}

	public interface IPropertyValueExporter
	{
		string ConvertPropertyPath(string UnityProperty);
		JObject SerializeToJson(IExportState State, IPropertyValue MTFProperty);
	}

	public interface IPropertyValueImporter
	{
		string ConvertPropertyPath(string STFProperty);
		IPropertyValue ParseFromJson(IImportState State, JObject Json);
	}

	
	// Fallback Property

	[Serializable]
	public class UnrecognizedPropertyValue : IPropertyValue
	{
		public string _Type;
		public string Type => _Type;
		public string PreservedJson;
		public List<UnityEngine.Object> UsedResources = new List<UnityEngine.Object>();
	}
	public class UnrecognizedPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IImportState State, JObject Json)
		{
			var usedResources = new List<UnityEngine.Object>();
			foreach(string r in Json["used_resources"]) usedResources.Add(State.GetResource(r));
			return new UnrecognizedPropertyValue {_Type = (string)Json["type"], PreservedJson = Json.ToString(), UsedResources = usedResources};
		}
	}
	public class UnrecognizedPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IExportState State, IPropertyValue MTFProperty)
		{
			var p = (UnrecognizedPropertyValue)MTFProperty;
			foreach(var r in p.UsedResources) State.AddResource(r);
			return JObject.Parse(p.PreservedJson);
		}
	}


	// Default Properties

	[Serializable]
	public class TexturePropertyValue : IPropertyValue
	{
		public const string _TYPE = "MTF.texture";
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
			return new TexturePropertyValue {Texture = (Texture2D)State.GetResource((string)Json["value"])};
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
		public const string _TYPE = "MTF.texture_channel";
		public string Type => _TYPE;
		public Texture2D Texture;
		public int Channel;
	}
	public class TextureChannelPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IImportState State, JObject Json)
		{
			return new TextureChannelPropertyValue {Texture = (Texture2D)State.GetResource((string)Json["texture"]), Channel = (int)Json["channel"]};
		}
	}
	public class TextureChannelPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IExportState State, IPropertyValue MTFProperty)
		{
			var p = (TextureChannelPropertyValue)MTFProperty;
			return new JObject {{"type", TextureChannelPropertyValue._TYPE}, {"texture", State.AddResource(p.Texture)}, {"channel", p.Channel}};
		}
	}

	[Serializable]
	public class ColorPropertyValue : IPropertyValue
	{
		public const string _TYPE = "MTF.color";
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
		public const string _TYPE = "MTF.int";
		public string Type => _TYPE;
		public int Value;
	}
	public class IntPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IImportState State, JObject Json)
		{
			return new IntPropertyValue {Value = (int)Json["value"]};
		}
	}
	public class IntPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IExportState State, IPropertyValue MTFProperty)
		{
			var v = (IntPropertyValue)MTFProperty;
			return new JObject {{"type", IntPropertyValue._TYPE}, {"value", v.Value}};
		}
	}

	[Serializable]
	public class FloatPropertyValue : IPropertyValue
	{
		public const string _TYPE = "MTF.float";
		public string Type => _TYPE;
		public float Value;
	}
	public class FloatPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IImportState State, JObject Json)
		{
			return new FloatPropertyValue {Value = (float)Json["value"]};
		}
	}
	public class FloatPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IExportState State, IPropertyValue MTFProperty)
		{
			var v = (FloatPropertyValue)MTFProperty;
			return new JObject {{"type", FloatPropertyValue._TYPE}, {"value", v.Value}};
		}
	}

	[Serializable]
	public class StringPropertyValue : IPropertyValue
	{
		public const string _TYPE = "MTF.string";
		public string Type => _TYPE;
		public string Value;
	}
	public class StringPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IImportState State, JObject Json)
		{
			return new StringPropertyValue {Value = (string)Json["value"]};
		}
	}
	public class StringPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IExportState State, IPropertyValue MTFProperty)
		{
			var v = (StringPropertyValue)MTFProperty;
			return new JObject {{"type", StringPropertyValue._TYPE}, {"value", v.Value}};
		}
	}
}
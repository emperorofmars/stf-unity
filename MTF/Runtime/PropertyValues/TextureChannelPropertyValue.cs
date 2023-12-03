using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
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
}
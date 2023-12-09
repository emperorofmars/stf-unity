using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
	[CreateAssetMenu(fileName = "TexturePropertyValue", menuName = "MTF/PropertyValues/Texture", order = 1)]
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
}
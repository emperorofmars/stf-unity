using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
	[CreateAssetMenu(fileName = "ColorPropertyValue", menuName = "MTF/PropertyValues/Color", order = 1)]
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
}
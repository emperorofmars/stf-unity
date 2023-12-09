using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
	[CreateAssetMenu(fileName = "StringPropertyValue", menuName = "MTF/PropertyValues/String", order = 1)]
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
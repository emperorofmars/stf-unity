using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
	[CreateAssetMenu(fileName = "IntPropertyValue", menuName = "MTF/PropertyValues/Int", order = 1)]
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
}
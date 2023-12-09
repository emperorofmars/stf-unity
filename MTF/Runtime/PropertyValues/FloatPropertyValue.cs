using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
	[CreateAssetMenu(fileName = "FloatPropertyValue", menuName = "MTF/PropertyValues/Float", order = 1)]
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
}
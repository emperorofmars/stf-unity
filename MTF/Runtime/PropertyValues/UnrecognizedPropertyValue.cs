using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF.PropertyValues
{
	[CreateAssetMenu(fileName = "UnrecognizedPropertyValue", menuName = "MTF/PropertyValues/Unrecognized", order = 2)]
	public class UnrecognizedPropertyValue : IPropertyValue
	{
		public string _Type;
		public override string Type => _Type;

		public string PreservedJson;
	}
	public class UnrecognizedPropertyValueImporter : IPropertyValueImporter
	{
		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public IPropertyValue ParseFromJson(IPropertyValueImportState State, JObject Json)
		{
			var prop = ScriptableObject.CreateInstance<UnrecognizedPropertyValue>();
			prop.PreservedJson = Json.ToString();
			return prop;
		}
	}
	public class UnrecognizedPropertyValueExporter : IPropertyValueExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public JObject SerializeToJson(IPropertyValueExportState State, IPropertyValue MTFProperty)
		{
			var v = (UnrecognizedPropertyValue)MTFProperty;
			return JObject.Parse(v.PreservedJson);
		}
	}
}
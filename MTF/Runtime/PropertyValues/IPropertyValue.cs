using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF.PropertyValues
{
	public abstract class IPropertyValue : ScriptableObject
	{
		public abstract string Type {get;}
	}
	public interface IPropertyValueImportState
	{
		Object GetResource(JToken Id);
	}

	public interface IPropertyValueExportState
	{
		string AddResource(Object Resource);
	}

	public interface IPropertyValueExporter
	{
		string ConvertPropertyPath(string UnityProperty);
		JObject SerializeToJson(IPropertyValueExportState State, IPropertyValue MTFProperty);
	}

	public interface IPropertyValueImporter
	{
		string ConvertPropertyPath(string STFProperty);
		IPropertyValue ParseFromJson(IPropertyValueImportState State, JObject Json);
	}
}
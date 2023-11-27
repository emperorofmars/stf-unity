using Newtonsoft.Json.Linq;

namespace MTF
{
	public interface IPropertyValue
	{
		string Type {get;}
	}
	public interface IPropertyValueImportState
	{
		UnityEngine.Object GetResource(string Id);
	}

	public interface IPropertyValueExportState
	{
		string AddResource(UnityEngine.Object Resource);
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
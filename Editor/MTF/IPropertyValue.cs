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
}
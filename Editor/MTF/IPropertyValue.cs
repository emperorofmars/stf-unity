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
		JObject SerializeToJson(IExportState State, IPropertyValue MTFProperty);
	}

	public interface IPropertyValueImporter
	{
		IPropertyValue ParseFromJson(IImportState State, JObject Json);
	}
}
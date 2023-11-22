using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTF
{
	public class PropertyValueRegistry
	{
		public static readonly Dictionary<string, IPropertyValueImporter> DefaultPropertyValueImporters = new Dictionary<string, IPropertyValueImporter> {
			{TexturePropertyValue._TYPE, new TexturePropertyValueImporter()},
			{TextureChannelPropertyValue._TYPE, new TextureChannelPropertyValueImporter()},
			{ColorPropertyValue._TYPE, new ColorPropertyValueImporter()},
		};
		public static readonly Dictionary<string, IPropertyValueExporter> DefaultPropertyValueExporters = new Dictionary<string, IPropertyValueExporter> {
			{TexturePropertyValue._TYPE, new TexturePropertyValueExporter()},
			{TextureChannelPropertyValue._TYPE, new TextureChannelPropertyValueExporter()},
			{ColorPropertyValue._TYPE, new ColorPropertyValueExporter()},
		};
	}

	public class ShaderConverterRegistry
	{
		public static readonly Dictionary<string, IMaterialConverter> DefaultMaterialConverters = new Dictionary<string, IMaterialConverter> {
			{StandardConverter._SHADER_NAME, new StandardConverter()},
		};
		public static readonly Dictionary<string, IMaterialParser> DefaultMaterialParsers = new Dictionary<string, IMaterialParser> {
			{StandardConverter._SHADER_NAME, new StandardParser()},
		};
	}
}
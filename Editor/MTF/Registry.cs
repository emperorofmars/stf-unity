using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTF
{
	static class CollectionUtil
	{
		public static Dictionary<A, B> Combine<A, B>(Dictionary<A, B> Base, Dictionary<A, B> ToBeMerged)
		{
			var ret = new Dictionary<A, B>(Base);
			foreach(var e in ToBeMerged)
			{
				if(ret.ContainsKey(e.Key)) ret[e.Key] = e.Value;
				else ret.Add(e.Key, e.Value);
			}
			return ret;
		}
		public static List<A> Combine<A>(List<A> Base, List<A> ToBeMerged)
		{
			var ret = new List<A>(Base);
			foreach(var e in ToBeMerged)
			{
				if(!ret.Contains(e)) ret.Add(e);
			}
			return ret;
		}
	}

	public class PropertyValueRegistry
	{
		public static readonly Dictionary<string, IPropertyValueImporter> DefaultPropertyValueImporters = new Dictionary<string, IPropertyValueImporter> {
			{TexturePropertyValue._TYPE, new TexturePropertyValueImporter()},
			{TextureChannelPropertyValue._TYPE, new TextureChannelPropertyValueImporter()},
			{ColorPropertyValue._TYPE, new ColorPropertyValueImporter()},
			{IntPropertyValue._TYPE, new IntPropertyValueImporter()},
			{FloatPropertyValue._TYPE, new FloatPropertyValueImporter()},
			{StringPropertyValue._TYPE, new StringPropertyValueImporter()},
		};
		public static readonly Dictionary<string, IPropertyValueExporter> DefaultPropertyValueExporters = new Dictionary<string, IPropertyValueExporter> {
			{TexturePropertyValue._TYPE, new TexturePropertyValueExporter()},
			{TextureChannelPropertyValue._TYPE, new TextureChannelPropertyValueExporter()},
			{ColorPropertyValue._TYPE, new ColorPropertyValueExporter()},
			{IntPropertyValue._TYPE, new IntPropertyValueExporter()},
			{FloatPropertyValue._TYPE, new FloatPropertyValueExporter()},
			{StringPropertyValue._TYPE, new StringPropertyValueExporter()},
		};
		
		private static Dictionary<string, IPropertyValueImporter> RegisteredPropertyValueImporters = new Dictionary<string, IPropertyValueImporter>();
		private static Dictionary<string, IPropertyValueExporter> RegisteredPropertyValueExporters = new Dictionary<string, IPropertyValueExporter>();

		public static Dictionary<string, IPropertyValueImporter> PropertyValueImporters {get => CollectionUtil.Combine(DefaultPropertyValueImporters, RegisteredPropertyValueImporters);}
		public static Dictionary<string, IPropertyValueExporter> PropertyValueExporters {get => CollectionUtil.Combine(DefaultPropertyValueExporters, RegisteredPropertyValueExporters);}

		public static void RegisterValueImporter(string Type, IPropertyValueImporter Importer) { RegisteredPropertyValueImporters.Add(Type, Importer); }
		public static void RegisterValueExporter(string Type, IPropertyValueExporter Exporter) { RegisteredPropertyValueExporters.Add(Type, Exporter); }
	}

	public class ShaderConverterRegistry
	{
		public static readonly Dictionary<string, IMaterialConverter> DefaultMaterialConverters = new Dictionary<string, IMaterialConverter> {
			{StandardConverter._SHADER_NAME, new StandardConverter()},
		};
		public static readonly Dictionary<string, IMaterialParser> DefaultMaterialParsers = new Dictionary<string, IMaterialParser> {
			{StandardConverter._SHADER_NAME, new StandardParser()},
		};

		private static Dictionary<string, IMaterialConverter> RegisteredMaterialConverters = new Dictionary<string, IMaterialConverter>();
		private static Dictionary<string, IMaterialParser> RegisteredMaterialParsers = new Dictionary<string, IMaterialParser>();
		
		public static Dictionary<string, IMaterialConverter> MaterialConverters {get => CollectionUtil.Combine(DefaultMaterialConverters, RegisteredMaterialConverters);}
		public static Dictionary<string, IMaterialParser> MaterialParsers {get => CollectionUtil.Combine(DefaultMaterialParsers, RegisteredMaterialParsers);}

		public static void RegisterMaterialConverter(string Type, IMaterialConverter Converter) { RegisteredMaterialConverters.Add(Type, Converter); }
		public static void RegisterMaterialParser(string Type, IMaterialParser Exporter) { RegisteredMaterialParsers.Add(Type, Exporter); }
	}
}
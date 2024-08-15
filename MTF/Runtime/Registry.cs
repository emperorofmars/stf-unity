using System.Collections.Generic;
using MTF.PropertyValues;

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

	public static class PropertyValueRegistry
	{
		public static readonly Dictionary<string, IPropertyValueImporter> DefaultPropertyValueImporters = new()
		{
			{TexturePropertyValue._TYPE, new TexturePropertyValueImporter()},
			{TextureChannelPropertyValue._TYPE, new TextureChannelPropertyValueImporter()},
			{ColorPropertyValue._TYPE, new ColorPropertyValueImporter()},
			{IntPropertyValue._TYPE, new IntPropertyValueImporter()},
			{FloatPropertyValue._TYPE, new FloatPropertyValueImporter()},
			{StringPropertyValue._TYPE, new StringPropertyValueImporter()},
		};
		public static readonly Dictionary<string, IPropertyValueExporter> DefaultPropertyValueExporters = new()
		{
			{TexturePropertyValue._TYPE, new TexturePropertyValueExporter()},
			{TextureChannelPropertyValue._TYPE, new TextureChannelPropertyValueExporter()},
			{ColorPropertyValue._TYPE, new ColorPropertyValueExporter()},
			{IntPropertyValue._TYPE, new IntPropertyValueExporter()},
			{FloatPropertyValue._TYPE, new FloatPropertyValueExporter()},
			{StringPropertyValue._TYPE, new StringPropertyValueExporter()},
		};
		
		private static readonly Dictionary<string, IPropertyValueImporter> RegisteredPropertyValueImporters = new();
		private static readonly Dictionary<string, IPropertyValueExporter> RegisteredPropertyValueExporters = new();

		public static Dictionary<string, IPropertyValueImporter> PropertyValueImporters => CollectionUtil.Combine(DefaultPropertyValueImporters, RegisteredPropertyValueImporters);
		public static Dictionary<string, IPropertyValueExporter> PropertyValueExporters => CollectionUtil.Combine(DefaultPropertyValueExporters, RegisteredPropertyValueExporters);

		public static void RegisterValueImporter(string Type, IPropertyValueImporter Importer) { RegisteredPropertyValueImporters.Add(Type, Importer); }
		public static void RegisterValueExporter(string Type, IPropertyValueExporter Exporter) { RegisteredPropertyValueExporters.Add(Type, Exporter); }

		public static IPropertyValueImporter GetPropertyValueImporter(string Type)
		{
			return PropertyValueImporters.ContainsKey(Type) ? PropertyValueImporters[Type] : new UnrecognizedPropertyValueImporter();
		}
		public static IPropertyValueExporter GetPropertyValueExporter(string Type)
		{
			return PropertyValueExporters.ContainsKey(Type) ? PropertyValueExporters[Type] : new UnrecognizedPropertyValueExporter();
		}
	}

	public static class ShaderConverterRegistry
	{
		public static readonly Dictionary<string, IMaterialConverter> DefaultMaterialConverters = new()
		{
			{StandardConverter._SHADER_NAME, new StandardConverter()},
		};
		public static readonly Dictionary<string, IMaterialParser> DefaultMaterialParsers = new()
		{
			{StandardConverter._SHADER_NAME, new StandardParser()},
		};

		private static readonly Dictionary<string, IMaterialConverter> RegisteredMaterialConverters = new();
		private static readonly Dictionary<string, IMaterialParser> RegisteredMaterialParsers = new();
		
		public static Dictionary<string, IMaterialConverter> MaterialConverters => CollectionUtil.Combine(DefaultMaterialConverters, RegisteredMaterialConverters);
		public static Dictionary<string, IMaterialParser> MaterialParsers => CollectionUtil.Combine(DefaultMaterialParsers, RegisteredMaterialParsers);

		public static void RegisterMaterialConverter(string Type, IMaterialConverter Converter) { RegisteredMaterialConverters.Add(Type, Converter); }
		public static void RegisterMaterialParser(string Type, IMaterialParser Exporter) { RegisteredMaterialParsers.Add(Type, Exporter); }

		public static IMaterialConverter GetMaterialConverter(string Type)
		{
			return MaterialConverters.ContainsKey(Type) ? MaterialConverters[Type] : MaterialConverters[StandardConverter._SHADER_NAME];
		}
		public static IMaterialParser GetMaterialParser(string Type)
		{
			return MaterialParsers.ContainsKey(Type) ? MaterialParsers[Type] : MaterialParsers[StandardConverter._SHADER_NAME];
		}
	}
}
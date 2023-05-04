
using System.Collections.Generic;
using System;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	public static class STFRegistry
	{
		public static readonly Dictionary<string, ISTFAssetImporter> DefaultAssetImporters = new Dictionary<string, ISTFAssetImporter>() {
			{"asset", new STFAssetImporter()}
			//{"patch", null}
		};
		public static readonly Dictionary<string, ASTFNodeImporter> DefaultNodeImporters = new Dictionary<string, ASTFNodeImporter>() {
			{"default", new STFNodeImporter()},
			//{"patch", null},
			//{"appendage", null}
		};
		public static readonly Dictionary<string, ASTFComponentImporter> DefaultComponentImporters = new Dictionary<string, ASTFComponentImporter>() {
			{STFMeshInstanceImporter._TYPE, new STFMeshInstanceImporter()},
			{STFTwistConstraint._TYPE, new STFTwistConstraintImporter()},
			{STFTwistConstraintBack._TYPE, new STFTwistConstraintBackImporter()},
			{STFTwistConstraintForward._TYPE, new STFTwistConstraintForwardImporter()}
		};
		public static readonly Dictionary<Type, ASTFComponentExporter> DefaultComponentExporters = new Dictionary<Type, ASTFComponentExporter>() {
			{typeof(SkinnedMeshRenderer), new STFMeshInstanceExporter()},
			{typeof(STFTwistConstraint), new STFTwistConstraintExporter()},
			{typeof(STFTwistConstraintBack), new STFTwistConstraintBackExporter()},
			{typeof(STFTwistConstraintForward), new STFTwistConstraintForwardExporter()}
		};
		public static readonly Dictionary<string, ASTFResourceImporter> DefaultResourceImporters = new Dictionary<string, ASTFResourceImporter>() {
			{STFMeshImporter._TYPE, new STFMeshImporter()},
			{STFTextureImporter._TYPE, new STFTextureImporter()}
			//Material
			//Texture
			//Animation
		};
		public static readonly Dictionary<Type, ASTFResourceExporter> DefaultResourceExporters = new Dictionary<Type, ASTFResourceExporter>() {
			{typeof(Mesh), new STFMeshExporter()},
			{typeof(Texture2D), new STFTextureExporter()}
			//Material
			//Texture
			//Animation
		};

		private static Dictionary<string, ISTFAssetImporter> RegisteredAssetImporters = new Dictionary<string, ISTFAssetImporter>();

		private static Dictionary<string, ASTFNodeImporter> RegisteredNodeImporters = new Dictionary<string, ASTFNodeImporter>();

		private static Dictionary<string, ASTFComponentImporter> RegisteredComponentImporters = new Dictionary<string, ASTFComponentImporter>();
		private static Dictionary<Type, ASTFComponentExporter> RegisteredComponentExporters = new Dictionary<Type, ASTFComponentExporter>();
		private static Dictionary<string, ASTFResourceImporter> RegisteredResourceImporters = new Dictionary<string, ASTFResourceImporter>();
		private static Dictionary<Type, ASTFResourceExporter> RegisteredResourceExporters = new Dictionary<Type, ASTFResourceExporter>();

		public static void RegisterAssetImporter(string type, ISTFAssetImporter importer) { RegisteredAssetImporters.Add(type, importer); }
		public static void RegisterNodeImporter(string type, ASTFNodeImporter importer) { RegisteredNodeImporters.Add(type, importer); }
		public static void RegisterComponentImporter(string type, ASTFComponentImporter importer) { RegisteredComponentImporters.Add(type, importer); }
		public static void RegisterComponentExporter(Type type, ASTFComponentExporter exporter) { RegisteredComponentExporters.Add(type, exporter); }
		public static void RegisterResourceImporter(string type, ASTFResourceImporter importer) { RegisteredResourceImporters.Add(type, importer); }
		public static void RegisterResourceExporter(Type type, ASTFResourceExporter exporter) { RegisteredResourceExporters.Add(type, exporter); }

		public static bool IsAssetImporterRegistered(string type) { return RegisteredAssetImporters.ContainsKey(type); }
		public static bool IsNodeImporterRegistered(string type) { return RegisteredNodeImporters.ContainsKey(type); }
		public static bool IsComponentImporterRegistered(string type) { return RegisteredComponentImporters.ContainsKey(type); }
		public static bool IsComponentExporterRegistered(Type type) { return RegisteredComponentExporters.ContainsKey(type); }
		public static bool IsResourceImporterRegistered(string type) { return RegisteredResourceImporters.ContainsKey(type); }
		public static bool IsResourceExporterRegistered(Type type) { return RegisteredResourceExporters.ContainsKey(type); }

		public static ISTFAssetImporter GetAssetImporter(string type) { return RegisteredAssetImporters[type]; }
		public static ASTFNodeImporter GetNodeImporter(string type) { return RegisteredNodeImporters[type]; }
		public static ASTFComponentImporter GetComponentImporter(string type) { return RegisteredComponentImporters[type]; }
		public static ASTFComponentExporter GetComponentExporter(Type type) { return RegisteredComponentExporters[type]; }
		public static ASTFResourceImporter GetResourceImporter(string type) { return RegisteredResourceImporters[type]; }
		public static ASTFResourceExporter GetResourceExporter(Type type) { return RegisteredResourceExporters[type]; }

		public static STFImportContext GetDefaultImportContext()
		{
			var assetImporters = new Dictionary<string, ISTFAssetImporter>(DefaultAssetImporters);
			foreach(var e in RegisteredAssetImporters) assetImporters.Add(e.Key, e.Value);

			var nodeImporters = new Dictionary<string, ASTFNodeImporter>(DefaultNodeImporters);
			foreach(var e in RegisteredNodeImporters) nodeImporters.Add(e.Key, e.Value);

			var componentImporters = new Dictionary<string, ASTFComponentImporter>(DefaultComponentImporters);
			foreach(var e in RegisteredComponentImporters) componentImporters.Add(e.Key, e.Value);

			var resourceImporters = new Dictionary<string, ASTFResourceImporter>(DefaultResourceImporters);
			foreach(var e in RegisteredResourceImporters) resourceImporters.Add(e.Key, e.Value);

			return new STFImportContext() {
				AssetImporters = assetImporters,
				NodeImporters = nodeImporters,
				ComponentImporters = componentImporters,
				ResourceImporters = resourceImporters
			};
		}

		public static STFExportContext GetDefaultExportContext()
		{
			var componentExporters = new Dictionary<Type, ASTFComponentExporter>(DefaultComponentExporters);
			foreach(var e in RegisteredComponentExporters) componentExporters.Add(e.Key, e.Value);

			var resourceExporters = new Dictionary<Type, ASTFResourceExporter>(DefaultResourceExporters);
			foreach(var e in RegisteredResourceExporters) resourceExporters.Add(e.Key, e.Value);

			return new STFExportContext() {
				ComponentExporters = componentExporters,
				ResourceExporters = resourceExporters
			};
		}
	}
}

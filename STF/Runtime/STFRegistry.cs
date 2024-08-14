
using System.Collections.Generic;
using System;
using UnityEngine;
using STF.Types;
using STF.ApplicationConversion;

namespace STF.Serialisation
{
	// Context's to pass into an importer and exporter respectively. Default ones are created automatically, construct these manually only for specific use cases.
	public class STFImportContext
	{
		public Dictionary<string, ISTFAssetImporter> AssetImporters = new();
		public Dictionary<string, ISTFNodeImporter> NodeImporters = new();
		public Dictionary<string, ISTFNodeComponentImporter> NodeComponentImporters = new();
		public Dictionary<string, ISTFResourceImporter> ResourceImporters = new();
		public Dictionary<string, ISTFResourceComponentImporter> ResourceComponentImporters = new();
		public List<ISTFImportPostProcessor> ImportPostProcessors = new();

		public ISTFAssetImporter GetAssetImporter(string Type) => AssetImporters.ContainsKey(Type) ? AssetImporters[Type] : new STFUnrecognizedAssetImporter();
		public ISTFNodeImporter GetNodeImporter(string Type) => NodeImporters.ContainsKey(Type) ? NodeImporters[Type] : new STFUnrecognizedNodeImporter();
		public ISTFNodeComponentImporter GetNodeComponentImporter(string Type) => NodeComponentImporters.ContainsKey(Type) ? NodeComponentImporters[Type] : new STFUnrecognizedNodeComponentImporter();
		public ISTFResourceImporter GetResourceImporter(string Type) => ResourceImporters.ContainsKey(Type) ? ResourceImporters[Type] : new STFUnrecognizedResourceImporter();
		public ISTFResourceComponentImporter GetResourceComponentImporter(string Type) => ResourceComponentImporters.ContainsKey(Type) ? ResourceComponentImporters[Type] : new STFUnrecognizedResourceComponentImporter();
	}

	public class STFExportContext
	{
		public Dictionary<string, ISTFAssetExporter> AssetExporters = new();
		public Dictionary<string, ISTFNodeExporter> NodeExporters = new();
		public Dictionary<Type, ISTFNodeComponentExporter> NodeComponentExporters = new();
		public Dictionary<Type, ISTFResourceExporter> ResourceExporters = new();
		public Dictionary<string, ISTFResourceComponentExporter> ResourceComponentExporters = new();
		public List<Type> ExportExclusions = new();

		public ISTFAssetExporter GetAssetExporter(string Type) => AssetExporters.ContainsKey(Type) ? AssetExporters[Type] : new STFUnrecognizedAssetExporter();
		public ISTFNodeExporter GetNodeExporter(string Type) => NodeExporters.ContainsKey(Type) ? NodeExporters[Type] : new STFUnrecognizedNodeExporter();
		public ISTFNodeComponentExporter GetNodeComponentExporter(Type Type) => NodeComponentExporters.ContainsKey(Type) ? NodeComponentExporters[Type] : Type == typeof(STFUnrecognizedNodeComponent) ? new STFUnrecognizedNodeComponentExporter() : throw new Exception($"Cannot export unrecognized Unity type: {Type}");
		public ISTFResourceExporter GetResourceExporter(Type Type) => ResourceExporters.ContainsKey(Type) ? ResourceExporters[Type] : Type == typeof(STFUnrecognizedResource) ? new STFUnrecognizedResourceExporter() : throw new Exception($"Cannot export unrecognized Unity type: {Type}");
		public ISTFResourceComponentExporter GetResourceComponentExporter(string Type) => ResourceComponentExporters.ContainsKey(Type) ? ResourceComponentExporters[Type] : new STFUnrecognizedResourceComponentExporter();
	}

	// Used to register STF object types. Default ones are included here, additional ones can be added using the appropriate method.
	public static class STFRegistry
	{
		public static readonly Dictionary<string, ISTFAssetExporter> DefaultAssetExporters = new() {
			{STFAsset._TYPE, new STFAssetExporter()},
			{STFAddonAsset._TYPE, new STFAddonAssetExporter()}
		};
		public static readonly Dictionary<string, ISTFAssetImporter> DefaultAssetImporters = new() {
			{STFAsset._TYPE, new STFAssetImporter()},
			{STFAddonAsset._TYPE, new STFAddonAssetImporter()}
		};
		public static readonly Dictionary<string, ISTFNodeImporter> DefaultNodeImporters = new() {
			{STFNode._TYPE, new STFNodeImporter()},
			{STFArmatureInstanceNode._TYPE, new STFArmatureInstanceImporter()},
			{STFBoneNode._TYPE, new DontInvokeNodeImporter()},
			{STFBoneInstanceNode._TYPE, new DontInvokeNodeImporter()},
			{STFPatchNode._TYPE, new STFPatchNodeImporter()},
			{STFAppendageNode._TYPE, new STFAppendageNodeImporter()},
		};
		public static readonly Dictionary<string, ISTFNodeExporter> DefaultNodeExporters = new() {
			{STFNode._TYPE, new STFNodeExporter()},
			{STFArmatureInstanceNode._TYPE, new STFArmatureInstanceExporter()},
			{STFBoneNode._TYPE, new DontInvokeNodeExporter()},
			{STFBoneInstanceNode._TYPE, new DontInvokeNodeExporter()},
			{STFPatchNode._TYPE, new STFPatchNodeExporter()},
			{STFAppendageNode._TYPE, new STFAppendageNodeExporter()},
		};
		public static readonly Dictionary<string, ISTFNodeComponentImporter> DefaultNodeComponentImporters = new() {
			{STFMeshInstance._TYPE, new STFMeshInstanceImporter()},
			{STFTwistConstraint._TYPE, new STFTwistConstraintImporter()},
			{STFResourceHolder._TYPE, new STFResourceHolderImporter()}
		};
		public static readonly Dictionary<Type, ISTFNodeComponentExporter> DefaultNodeComponentExporters = new() {
			{typeof(SkinnedMeshRenderer), new STFMeshInstanceExporter()},
			{typeof(MeshRenderer), new STFMeshInstanceExporter()},
			{typeof(STFTwistConstraint), new STFTwistConstraintExporter()},
			{typeof(STFResourceHolder), new STFResourceHolderExporter()}
		};
		public static readonly Dictionary<string, ISTFResourceImporter> DefaultResourceImporters = new() {
			{STFMesh._TYPE, new STFMeshImporter()},
			{STFTexture._TYPE, new STFTextureImporter()},
			{STFArmature._TYPE, new STFArmatureImporter()},
			{MTFMaterial._TYPE, new MTFMaterialImporter()},
		};
		public static readonly Dictionary<Type, ISTFResourceExporter> DefaultResourceExporters = new() {
			{typeof(Mesh), new STFMeshExporter()},
			{typeof(Texture2D), new STFTexture2dExporter()},
			{typeof(STFArmature), new STFArmatureExporter()},
			{typeof(MTFMaterial), new MTFMaterialExporter()},
			{typeof(Material), new UnityMaterialExporter()},
		};
		public static readonly Dictionary<string, ISTFResourceComponentImporter> DefaultResourceComponentImporters = new() {
			{STFTextureDownscalePriority._TYPE, new STFTextureDownscalePriorityImporter()},
			{STFTextureCompression._TYPE, new STFTextureCompressionImporter()},
			{STFHumanoidArmature._TYPE, new STFHumanoidArmatureImporter()},
		};
		public static readonly Dictionary<string, ISTFResourceComponentExporter> DefaultResourceComponentExporters = new() {
			{STFTextureDownscalePriority._TYPE, new STFTextureDownscalePriorityExporter()},
			{STFTextureCompression._TYPE, new STFTextureCompressionExporter()},
			{STFHumanoidArmature._TYPE, new STFHumanoidArmatureExporter()},
		};
		public static readonly List<ISTFImportPostProcessor> DefaultImportPostProcessors = new();
		public static readonly List<Type> DefaultExportExclusions = new() {
			typeof(Transform), typeof(ISTFNode), typeof(Animator), typeof(STFAsset), typeof(STFAddonAsset), typeof(STFNode), typeof(STFBoneNode), typeof(STFBoneInstanceNode),
			typeof(STFMeshInstance), typeof(STFArmatureInstanceNode), typeof(STFArmatureNodeInfo), typeof(MeshFilter)
		};

		public static readonly List<ASTFApplicationConverter> ApplicationConverters = new();

		private static readonly Dictionary<string, ISTFAssetImporter> RegisteredAssetImporters = new();
		private static readonly Dictionary<string, ISTFAssetExporter> RegisteredAssetExporters = new();

		private static readonly Dictionary<string, ISTFNodeImporter> RegisteredNodeImporters = new();
		private static readonly Dictionary<string, ISTFNodeExporter> RegisteredNodeExporters = new();

		private static readonly Dictionary<string, ISTFNodeComponentImporter> RegisteredNodeComponentImporters = new();
		private static readonly Dictionary<Type, ISTFNodeComponentExporter> RegisteredNodeComponentExporters = new();

		private static readonly Dictionary<string, ISTFResourceImporter> RegisteredResourceImporters = new();
		private static readonly Dictionary<Type, ISTFResourceExporter> RegisteredResourceExporters = new();

		private static readonly Dictionary<string, ISTFResourceComponentImporter> RegisteredResourceComponentImporters = new();
		private static readonly Dictionary<string, ISTFResourceComponentExporter> RegisteredResourceComponentExporters = new();

		private static readonly List<ISTFImportPostProcessor> RegisteredImportPostProcessors = new();
		private static readonly List<Type> RegisteredExportExclusions = new();

		public static Dictionary<string, ISTFAssetImporter> AssetImporters => CollectionUtil.Combine(DefaultAssetImporters, RegisteredAssetImporters);
		public static Dictionary<string, ISTFAssetExporter> AssetExporters => CollectionUtil.Combine(DefaultAssetExporters, RegisteredAssetExporters);

		public static Dictionary<string, ISTFNodeImporter> NodeImporters => CollectionUtil.Combine(DefaultNodeImporters, RegisteredNodeImporters);
		public static Dictionary<string, ISTFNodeExporter> NodeExporters => CollectionUtil.Combine(DefaultNodeExporters, RegisteredNodeExporters);

		public static Dictionary<string, ISTFNodeComponentImporter> NodeComponentImporters => CollectionUtil.Combine(DefaultNodeComponentImporters, RegisteredNodeComponentImporters);
		public static Dictionary<Type, ISTFNodeComponentExporter> NodeComponentExporters => CollectionUtil.Combine(DefaultNodeComponentExporters, RegisteredNodeComponentExporters);

		public static Dictionary<string, ISTFResourceImporter> ResourceImporters => CollectionUtil.Combine(DefaultResourceImporters, RegisteredResourceImporters);
		public static Dictionary<Type, ISTFResourceExporter> ResourceExporters => CollectionUtil.Combine(DefaultResourceExporters, RegisteredResourceExporters);

		public static Dictionary<string, ISTFResourceComponentImporter> ResourceComponentImporters => CollectionUtil.Combine(DefaultResourceComponentImporters, RegisteredResourceComponentImporters);
		public static Dictionary<string, ISTFResourceComponentExporter> ResourceComponentExporters => CollectionUtil.Combine(DefaultResourceComponentExporters, RegisteredResourceComponentExporters);

		
		public static List<ISTFImportPostProcessor> ImportPostProcessors => CollectionUtil.Combine(DefaultImportPostProcessors, RegisteredImportPostProcessors);

		public static List<Type> ExportExclusions => CollectionUtil.Combine(DefaultExportExclusions, RegisteredExportExclusions);

		public static void RegisterAssetImporter(string type, ISTFAssetImporter importer) { RegisteredAssetImporters.Add(type, importer); }
		public static void RegisterAssetExporter(string type, ISTFAssetExporter exporter) { RegisteredAssetExporters.Add(type, exporter); }
		public static void RegisterNodeImporter(string type, ISTFNodeImporter importer) { RegisteredNodeImporters.Add(type, importer); }
		public static void RegisterNodeExporter(string type, ISTFNodeExporter exporter) { RegisteredNodeExporters.Add(type, exporter); }
		public static void RegisterNodeComponentImporter(string type, ISTFNodeComponentImporter importer) { RegisteredNodeComponentImporters.Add(type, importer); }
		public static void RegisterNodeComponentExporter(Type type, ISTFNodeComponentExporter exporter) { RegisteredNodeComponentExporters.Add(type, exporter); }
		public static void RegisterResourceImporter(string type, ISTFResourceImporter importer) { RegisteredResourceImporters.Add(type, importer); }
		public static void RegisterResourceExporter(Type type, ISTFResourceExporter exporter) { RegisteredResourceExporters.Add(type, exporter); }
		public static void RegisterResourceComponentImporter(string type, ISTFResourceComponentImporter importer) { RegisteredResourceComponentImporters.Add(type, importer); }
		public static void RegisterResourceComponentExporter(string type, ISTFResourceComponentExporter exporter) { RegisteredResourceComponentExporters.Add(type, exporter); }
		public static void RegisterImportPostProcessor(ISTFImportPostProcessor ImportPostProcessor) { RegisteredImportPostProcessors.Add(ImportPostProcessor); }
		public static void RegisterExportExclusion(Type type) { RegisteredExportExclusions.Add(type); }

		public static void RegisterApplicationConverter(ASTFApplicationConverter converter) { ApplicationConverters.Add(converter); }

		public static STFImportContext GetDefaultImportContext()
		{
			return new STFImportContext() {
				AssetImporters = AssetImporters,
				NodeImporters = NodeImporters,
				NodeComponentImporters = NodeComponentImporters,
				ResourceImporters = ResourceImporters,
				ResourceComponentImporters = ResourceComponentImporters,
				ImportPostProcessors = ImportPostProcessors
			};
		}

		public static STFExportContext GetDefaultExportContext()
		{
			return new STFExportContext() {
				AssetExporters = AssetExporters,
				NodeExporters = NodeExporters,
				NodeComponentExporters = NodeComponentExporters,
				ResourceExporters = ResourceExporters,
				ResourceComponentExporters = ResourceComponentExporters,
				ExportExclusions = ExportExclusions
			};
		}
	}
}

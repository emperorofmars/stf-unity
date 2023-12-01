
using System.Collections.Generic;
using System;
using UnityEngine;

namespace STF.Serialisation
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

	// Context's to pass into an importer and exporter respectively. Default ones are created automatically, construct these manually only for specific use cases.
	public class STFImportContext
	{
		public Dictionary<string, ISTFAssetImporter> AssetImporters = new Dictionary<string, ISTFAssetImporter>();
		public Dictionary<string, ISTFNodeImporter> NodeImporters = new Dictionary<string, ISTFNodeImporter>();
		public Dictionary<string, ISTFNodeComponentImporter> NodeComponentImporters = new Dictionary<string, ISTFNodeComponentImporter>();
		public Dictionary<string, ISTFResourceImporter> ResourceImporters = new Dictionary<string, ISTFResourceImporter>();
		public Dictionary<string, ISTFResourceComponentImporter> ResourceComponentImporters = new Dictionary<string, ISTFResourceComponentImporter>();
	}

	public class STFExportContext
	{
		public Dictionary<string, ISTFAssetExporter> AssetExporters = new Dictionary<string, ISTFAssetExporter>();
		public Dictionary<string, ISTFNodeExporter> NodeExporters = new Dictionary<string, ISTFNodeExporter>();
		public Dictionary<Type, ISTFNodeComponentExporter> NodeComponentExporters = new Dictionary<Type, ISTFNodeComponentExporter>();
		public Dictionary<Type, ISTFResourceExporter> ResourceExporters = new Dictionary<Type, ISTFResourceExporter>();
		public Dictionary<string, ISTFResourceComponentExporter> ResourceComponentExporters = new Dictionary<string, ISTFResourceComponentExporter>();
		public List<Type> ExportExclusions = new List<Type>();
	}

	// Used to register STF object types. Default ones are included by default, additional ones can be added automatically.
	public static class STFRegistry
	{
		public static readonly Dictionary<string, ISTFAssetExporter> DefaultAssetExporters = new Dictionary<string, ISTFAssetExporter>() {
			{STFAsset._TYPE, new STFAssetExporter()},
			//{STFAddonAssetExporter._TYPE, new STFAddonAssetExporter()}
		};
		public static readonly Dictionary<string, ISTFAssetImporter> DefaultAssetImporters = new Dictionary<string, ISTFAssetImporter>() {
			{STFAsset._TYPE, new STFAssetImporter()},
			//{STFAddonAssetImporter._TYPE, new STFAddonAssetImporter()}
		};
		public static readonly Dictionary<string, ISTFNodeImporter> DefaultNodeImporters = new Dictionary<string, ISTFNodeImporter>() {
			{STFNode._TYPE, new STFNodeImporter()},
			{STFArmatureInstanceNode._TYPE, new STFArmatureInstanceImporter()},
			{STFBoneNode._TYPE, new DontInvokeNodeImporter()},
			{STFBoneInstanceNode._TYPE, new DontInvokeNodeImporter()},
			//{STFAppendageNodeImporter._TYPE, new STFAppendageNodeImporter()},
			//{STFPatchNodeImporter._TYPE, new STFPatchNodeImporter()}
		};
		public static readonly Dictionary<string, ISTFNodeExporter> DefaultNodeExporters = new Dictionary<string, ISTFNodeExporter>() {
			{STFNode._TYPE, new STFNodeExporter()},
			{STFArmatureInstanceNode._TYPE, new STFArmatureInstanceExporter()},
			{STFBoneNode._TYPE, new DontInvokeNodeExporter()},
			{STFBoneInstanceNode._TYPE, new DontInvokeNodeExporter()},
		};
		public static readonly Dictionary<string, ISTFNodeComponentImporter> DefaultNodeComponentImporters = new Dictionary<string, ISTFNodeComponentImporter>() {
			{STFMeshInstance._TYPE, new STFMeshInstanceImporter()},
			{STFTwistConstraint._TYPE, new STFTwistConstraintImporter()},
			{STFResourceHolder._TYPE, new STFResourceHolderImporter()}
		};
		public static readonly Dictionary<Type, ISTFNodeComponentExporter> DefaultNodeComponentExporters = new Dictionary<Type, ISTFNodeComponentExporter>() {
			{typeof(SkinnedMeshRenderer), new STFMeshInstanceExporter()},
			{typeof(STFTwistConstraint), new STFTwistConstraintExporter()},
			{typeof(STFResourceHolder), new STFResourceHolderExporter()}
		};
		public static readonly Dictionary<string, ISTFResourceImporter> DefaultResourceImporters = new Dictionary<string, ISTFResourceImporter>() {
			{STFMeshImporter._TYPE, new STFMeshImporter()},
			{STFTextureImporter._TYPE, new STFTextureImporter()},
			{STFArmatureImporter._TYPE, new STFArmatureImporter()},
			{MTFMaterialImporter._TYPE, new MTFMaterialImporter()},
		};
		public static readonly Dictionary<Type, ISTFResourceExporter> DefaultResourceExporters = new Dictionary<Type, ISTFResourceExporter>() {
			{typeof(Mesh), new STFMeshExporter()},
			{typeof(Texture2D), new STFTexture2dExporter()},
			{typeof(STFArmature), new STFArmatureExporter()},
			{typeof(MTF.Material), new MTFMaterialExporter()},
			{typeof(Material), new UnityMaterialExporter()},
		};
		public static readonly Dictionary<string, ISTFResourceComponentImporter> DefaultResourceComponentImporters = new Dictionary<string, ISTFResourceComponentImporter>() {
			{STFTextureDownscalePriority._TYPE, new STFTextureDownscalePriorityImporter()},
			{STFHumanoidArmature._TYPE, new STFHumanoidArmatureImporter()},
		};
		public static readonly Dictionary<string, ISTFResourceComponentExporter> DefaultResourceComponentExporters = new Dictionary<string, ISTFResourceComponentExporter>() {
			{STFTextureDownscalePriority._TYPE, new STFTextureDownscalePriorityExporter()},
			{STFHumanoidArmature._TYPE, new STFHumanoidArmatureExporter()},
		};
		public static readonly List<Type> DefaultExportExclusions = new List<Type>() {
			typeof(Transform), typeof(ISTFNode), typeof(Animator), typeof(STFAsset), typeof(STFNode), typeof(STFBoneNode), typeof(STFBoneInstanceNode), typeof(STFMeshInstance), typeof(STFArmatureInstanceNode), typeof(STFArmatureNodeInfo)
		};

		private static Dictionary<string, ISTFAssetImporter> RegisteredAssetImporters = new Dictionary<string, ISTFAssetImporter>();
		private static Dictionary<string, ISTFAssetExporter> RegisteredAssetExporters = new Dictionary<string, ISTFAssetExporter>();

		private static Dictionary<string, ISTFNodeImporter> RegisteredNodeImporters = new Dictionary<string, ISTFNodeImporter>();
		private static Dictionary<string, ISTFNodeExporter> RegisteredNodeExporters = new Dictionary<string, ISTFNodeExporter>();

		private static Dictionary<string, ISTFNodeComponentImporter> RegisteredNodeComponentImporters = new Dictionary<string, ISTFNodeComponentImporter>();
		private static Dictionary<Type, ISTFNodeComponentExporter> RegisteredNodeComponentExporters = new Dictionary<Type, ISTFNodeComponentExporter>();

		private static Dictionary<string, ISTFResourceImporter> RegisteredResourceImporters = new Dictionary<string, ISTFResourceImporter>();
		private static Dictionary<Type, ISTFResourceExporter> RegisteredResourceExporters = new Dictionary<Type, ISTFResourceExporter>();

		private static Dictionary<string, ISTFResourceComponentImporter> RegisteredResourceComponentImporters = new Dictionary<string, ISTFResourceComponentImporter>();
		private static Dictionary<string, ISTFResourceComponentExporter> RegisteredResourceComponentExporters = new Dictionary<string, ISTFResourceComponentExporter>();

		private static readonly List<Type> RegisteredExportExclusions = new List<Type>();

		public static Dictionary<string, ISTFAssetImporter> AssetImporters {get => CollectionUtil.Combine(DefaultAssetImporters, RegisteredAssetImporters);}
		public static Dictionary<string, ISTFAssetExporter> AssetExporters {get => CollectionUtil.Combine(DefaultAssetExporters, RegisteredAssetExporters);}

		public static Dictionary<string, ISTFNodeImporter> NodeImporters {get => CollectionUtil.Combine(DefaultNodeImporters, RegisteredNodeImporters);}
		public static Dictionary<string, ISTFNodeExporter> NodeExporters {get => CollectionUtil.Combine(DefaultNodeExporters, RegisteredNodeExporters);}

		public static Dictionary<string, ISTFNodeComponentImporter> NodeComponentImporters {get => CollectionUtil.Combine(DefaultNodeComponentImporters, RegisteredNodeComponentImporters);}
		public static Dictionary<Type, ISTFNodeComponentExporter> NodeComponentExporters {get => CollectionUtil.Combine(DefaultNodeComponentExporters, RegisteredNodeComponentExporters);}

		public static Dictionary<string, ISTFResourceImporter> ResourceImporters {get => CollectionUtil.Combine(DefaultResourceImporters, RegisteredResourceImporters);}
		public static Dictionary<Type, ISTFResourceExporter> ResourceExporters {get => CollectionUtil.Combine(DefaultResourceExporters, RegisteredResourceExporters);}

		public static Dictionary<string, ISTFResourceComponentImporter> ResourceComponentImporters {get => CollectionUtil.Combine(DefaultResourceComponentImporters, RegisteredResourceComponentImporters);}
		public static Dictionary<string, ISTFResourceComponentExporter> ResourceComponentExporters {get => CollectionUtil.Combine(DefaultResourceComponentExporters, RegisteredResourceComponentExporters);}

		public static List<Type> ExportExclusions {get => CollectionUtil.Combine(DefaultExportExclusions, RegisteredExportExclusions);}

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
		public static void RegisterExportExclusion(Type type) { RegisteredExportExclusions.Add(type); }

		public static STFImportContext GetDefaultImportContext()
		{
			return new STFImportContext() {
				AssetImporters = AssetImporters,
				NodeImporters = NodeImporters,
				NodeComponentImporters = NodeComponentImporters,
				ResourceImporters = ResourceImporters,
				ResourceComponentImporters = ResourceComponentImporters
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

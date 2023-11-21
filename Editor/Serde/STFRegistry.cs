
using System.Collections.Generic;
using System;
using UnityEngine;
using STF.IdComponents;
using System.Linq;

namespace STF.Serde
{
	// Context's to pass into an importer and exporter respectively. Default ones are created automatically, construct these manually only for specific use cases.
	public class STFImportContext
	{
		public Dictionary<string, ISTFAssetImporter> AssetImporters = new Dictionary<string, ISTFAssetImporter>();
		public Dictionary<string, ISTFNodeImporter> NodeImporters = new Dictionary<string, ISTFNodeImporter>();
		public Dictionary<string, ISTFNodeComponentImporter> NodeComponentImporters = new Dictionary<string, ISTFNodeComponentImporter>();
		public Dictionary<string, ISTFResourceImporter> ResourceImporters = new Dictionary<string, ISTFResourceImporter>();
		//public Dictionary<Type, ISTFResourceComponentImporter> ResourceComponentImporters = new Dictionary<Type, ISTFResourceComponentImporter>();
	}

	public class STFExportContext
	{
		public Dictionary<string, ISTFAssetExporter> AssetExporters = new Dictionary<string, ISTFAssetExporter>();
		public Dictionary<string, ISTFNodeExporter> NodeExporters = new Dictionary<string, ISTFNodeExporter>();
		public Dictionary<Type, ISTFNodeComponentExporter> NodeComponentExporters = new Dictionary<Type, ISTFNodeComponentExporter>();
		public Dictionary<Type, ISTFResourceExporter> ResourceExporters = new Dictionary<Type, ISTFResourceExporter>();
		//public Dictionary<Type, ISTFResourceComponentExporter> ResourceComponentExporters = new Dictionary<Type, ISTFResourceComponentExporter>();
		public List<Type> ExportExclusions = new List<Type>();
	}

	// Used to register STF object types. Default ones are included by default, additional ones can be added automatically.
	public static class STFRegistry
	{
		public static readonly Dictionary<string, ISTFAssetExporter> DefaultAssetExporters = new Dictionary<string, ISTFAssetExporter>() {
			{STFAssetImporter._TYPE, new STFAssetExporter()},
			//{STFAddonAssetExporter._TYPE, new STFAddonAssetExporter()}
		};
		public static readonly Dictionary<string, ISTFAssetImporter> DefaultAssetImporters = new Dictionary<string, ISTFAssetImporter>() {
			{STFAssetImporter._TYPE, new STFAssetImporter()},
			//{STFAddonAssetImporter._TYPE, new STFAddonAssetImporter()}
		};
		public static readonly Dictionary<string, ISTFNodeImporter> DefaultNodeImporters = new Dictionary<string, ISTFNodeImporter>() {
			{STFNode._TYPE, new STFNodeImporter()},
			{STFArmatureInstanceNode._TYPE, new STFArmatureInstanceImporter()},
			//{STFAppendageNodeImporter._TYPE, new STFAppendageNodeImporter()},
			//{STFPatchNodeImporter._TYPE, new STFPatchNodeImporter()}
		};
		public static readonly Dictionary<string, ISTFNodeExporter> DefaultNodeExporters = new Dictionary<string, ISTFNodeExporter>() {
			{STFNode._TYPE, new STFNodeExporter()},
			{STFArmatureInstanceNode._TYPE, new STFArmatureInstanceExporter()},
		};
		// Also add node exporters. As they cannot be mapped to a Unity type, devise a way for them to determine wether to handle a node or not, perhaps with priorities or something
		public static readonly Dictionary<string, ISTFNodeComponentImporter> DefaultNodeComponentImporters = new Dictionary<string, ISTFNodeComponentImporter>() {
			{STFMeshInstance._TYPE, new STFMeshInstanceImporter()},
			{STFTwistConstraintBack._TYPE, new STFTwistConstraintBackImporter()},
			//{STFTwistConstraintForward._TYPE, new STFTwistConstraintForwardImporter()},
			//{STFAnimationHolder._TYPE, new STFAnimationHolderImporter()}
		};
		public static readonly Dictionary<Type, ISTFNodeComponentExporter> DefaultNodeComponentExporters = new Dictionary<Type, ISTFNodeComponentExporter>() {
			{typeof(SkinnedMeshRenderer), new STFMeshInstanceExporter()},
			{typeof(STFTwistConstraintBack), new STFTwistConstraintBackExporter()},
			//{typeof(STFTwistConstraintForward), new STFTwistConstraintForwardExporter()},
			//{typeof(STFAnimationHolder), new STFAnimationHolderExporter()}
		};
		public static readonly Dictionary<string, ISTFResourceImporter> DefaultResourceImporters = new Dictionary<string, ISTFResourceImporter>() {
			{STFMeshImporter._TYPE, new STFMeshImporter()},
			{STFTextureImporter._TYPE, new STFTextureImporter()},
			{STFArmatureImporter._TYPE, new STFArmatureImporter()},
			//{MTFMaterialImporter._TYPE, new MTFMaterialImporter()},
			//{STFAnimationImporter._TYPE, new STFAnimationImporter()}
		};
		public static readonly Dictionary<Type, ISTFResourceExporter> DefaultResourceExporters = new Dictionary<Type, ISTFResourceExporter>() {
			{typeof(Mesh), new STFMeshExporter()},
			{typeof(Texture2D), new STFTexture2dExporter()},
			{typeof(STFArmature), new STFArmatureExporter()},
			//{typeof(Material), new MTFMaterialExporter()},
			//{typeof(AnimationClip), new STFAnimationExporter()}
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

		private static readonly List<Type> RegisteredExportExclusions = new List<Type>();

		public static void RegisterAssetImporter(string type, ISTFAssetImporter importer) { RegisteredAssetImporters.Add(type, importer); }
		public static void RegisterAssetExporter(string type, ISTFAssetExporter exporter) { RegisteredAssetExporters.Add(type, exporter); }
		public static void RegisterNodeImporter(string type, ISTFNodeImporter importer) { RegisteredNodeImporters.Add(type, importer); }
		public static void RegisterNodeExporter(string type, ISTFNodeExporter exporter) { RegisteredNodeExporters.Add(type, exporter); }
		public static void RegisterNodeComponentImporter(string type, ISTFNodeComponentImporter importer) { RegisteredNodeComponentImporters.Add(type, importer); }
		public static void RegisterNodeComponentExporter(Type type, ISTFNodeComponentExporter exporter) { RegisteredNodeComponentExporters.Add(type, exporter); }
		public static void RegisterResourceImporter(string type, ISTFResourceImporter importer) { RegisteredResourceImporters.Add(type, importer); }
		public static void RegisterResourceExporter(Type type, ISTFResourceExporter exporter) { RegisteredResourceExporters.Add(type, exporter); }
		public static void RegisterExportExclusion(Type type) { RegisteredExportExclusions.Add(type); }

		public static bool IsAssetImporterRegistered(string type) { return RegisteredAssetImporters.ContainsKey(type); }
		public static bool IsAssetExporterRegistered(string type) { return RegisteredAssetExporters.ContainsKey(type); }
		public static bool IsNodeImporterRegistered(string type) { return RegisteredNodeImporters.ContainsKey(type); }
		public static bool IsNodeExporterRegistered(string type) { return RegisteredNodeExporters.ContainsKey(type); }
		public static bool IsNodeComponentImporterRegistered(string type) { return RegisteredNodeComponentImporters.ContainsKey(type); }
		public static bool IsNodeComponentExporterRegistered(Type type) { return RegisteredNodeComponentExporters.ContainsKey(type); }
		public static bool IsResourceImporterRegistered(string type) { return RegisteredResourceImporters.ContainsKey(type); }
		public static bool IsResourceExporterRegistered(Type type) { return RegisteredResourceExporters.ContainsKey(type); }
		public static bool IsExportExclusionRegistered(Type type) { return RegisteredExportExclusions.Contains(type); }

		public static ISTFAssetImporter GetAssetImporter(string type) { return RegisteredAssetImporters[type]; }
		public static ISTFAssetExporter GetAssetExporter(string type) { return RegisteredAssetExporters[type]; }
		public static ISTFNodeImporter GetNodeImporter(string type) { return RegisteredNodeImporters[type]; }
		public static ISTFNodeExporter GetNodeExporter(string type) { return RegisteredNodeExporters[type]; }
		public static ISTFNodeComponentImporter GetNodeComponentImporter(string type) { return RegisteredNodeComponentImporters[type]; }
		public static ISTFNodeComponentExporter GetNodeComponentExporter(Type type) { return RegisteredNodeComponentExporters[type]; }
		public static ISTFResourceImporter GetResourceImporter(string type) { return RegisteredResourceImporters[type]; }
		public static ISTFResourceExporter GetResourceExporter(Type type) { return RegisteredResourceExporters[type]; }
		public static List<Type> GetExportExclusions() { return RegisteredExportExclusions; }

		public static STFImportContext GetDefaultImportContext()
		{
			var assetImporters = new Dictionary<string, ISTFAssetImporter>(DefaultAssetImporters);
			foreach(var e in RegisteredAssetImporters)
			{
				if(assetImporters.ContainsKey(e.Key)) assetImporters[e.Key] = e.Value;
				else assetImporters.Add(e.Key, e.Value);
			}

			var nodeImporters = new Dictionary<string, ISTFNodeImporter>(DefaultNodeImporters);
			foreach(var e in RegisteredNodeImporters)
			{
				if(nodeImporters.ContainsKey(e.Key)) nodeImporters[e.Key] = e.Value;
				else nodeImporters.Add(e.Key, e.Value);
			}

			var componentImporters = new Dictionary<string, ISTFNodeComponentImporter>(DefaultNodeComponentImporters);
			foreach(var e in RegisteredNodeComponentImporters)
			{
				if(componentImporters.ContainsKey(e.Key)) componentImporters[e.Key] = e.Value;
				else componentImporters.Add(e.Key, e.Value);
			}

			var resourceImporters = new Dictionary<string, ISTFResourceImporter>(DefaultResourceImporters);
			foreach(var e in RegisteredResourceImporters)
			{
				if(resourceImporters.ContainsKey(e.Key)) resourceImporters[e.Key] = e.Value;
				else resourceImporters.Add(e.Key, e.Value);
			}

			//ResourceComponents

			return new STFImportContext() {
				AssetImporters = assetImporters,
				NodeImporters = nodeImporters,
				NodeComponentImporters = componentImporters,
				ResourceImporters = resourceImporters
			};
		}

		public static STFExportContext GetDefaultExportContext()
		{
			var assetExporters = new Dictionary<string, ISTFAssetExporter>(DefaultAssetExporters);
			foreach(var e in RegisteredAssetExporters)
			{
				if(assetExporters.ContainsKey(e.Key)) assetExporters[e.Key] = e.Value;
				else assetExporters.Add(e.Key, e.Value);
			}

			var nodeExporters = new Dictionary<string, ISTFNodeExporter>(DefaultNodeExporters);
			foreach(var e in RegisteredNodeExporters)
			{
				if(nodeExporters.ContainsKey(e.Key)) nodeExporters[e.Key] = e.Value;
				else nodeExporters.Add(e.Key, e.Value);
			}

			var componentExporters = new Dictionary<Type, ISTFNodeComponentExporter>(DefaultNodeComponentExporters);
			foreach(var e in RegisteredNodeComponentExporters)
			{
				if(componentExporters.ContainsKey(e.Key)) componentExporters[e.Key] = e.Value;
				else componentExporters.Add(e.Key, e.Value);
			}

			var resourceExporters = new Dictionary<Type, ISTFResourceExporter>(DefaultResourceExporters);
			foreach(var e in RegisteredResourceExporters)
			{
				if(resourceExporters.ContainsKey(e.Key)) resourceExporters[e.Key] = e.Value;
				else resourceExporters.Add(e.Key, e.Value);
			}

			var exportExclusions = new List<Type>(DefaultExportExclusions.Union(RegisteredExportExclusions));

			//ResourceComponents

			return new STFExportContext() {
				AssetExporters = assetExporters,
				NodeExporters = nodeExporters,
				NodeComponentExporters = componentExporters,
				ResourceExporters = resourceExporters,
				ExportExclusions = exportExclusions
			};
		}
	}
}

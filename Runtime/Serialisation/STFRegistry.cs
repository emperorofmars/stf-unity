
using System.Collections.Generic;
using System;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	public static class STFRegistry
	{
		public static readonly Dictionary<string, ISTFAssetImporter> DefaultAssetImporters = new Dictionary<string, ISTFAssetImporter>() {
			{"asset", new STFAssetImporter()},
			{"addon", new STFAddonAssetImporter()}
		};
		public static readonly Dictionary<string, ISTFNodeImporter> DefaultNodeImporters = new Dictionary<string, ISTFNodeImporter>() {
			{STFNodeImporter._TYPE, new STFNodeImporter()},
			{STFArmatureInstanceNodeImporter._TYPE, new STFArmatureInstanceNodeImporter()},
			{STFAppendageNodeImporter._TYPE, new STFAppendageNodeImporter()},
			{STFPatchNodeImporter._TYPE, new STFPatchNodeImporter()}
		};
		public static readonly Dictionary<string, ASTFComponentImporter> DefaultComponentImporters = new Dictionary<string, ASTFComponentImporter>() {
			{STFMeshInstanceImporter._TYPE, new STFMeshInstanceImporter()},
			{STFTwistConstraintBack._TYPE, new STFTwistConstraintBackImporter()},
			{STFTwistConstraintForward._TYPE, new STFTwistConstraintForwardImporter()},
			{STFAnimationHolder._TYPE, new STFAnimationHolderImporter()}
		};
		public static readonly Dictionary<Type, ASTFComponentExporter> DefaultComponentExporters = new Dictionary<Type, ASTFComponentExporter>() {
			{typeof(SkinnedMeshRenderer), new STFMeshInstanceExporter()},
			{typeof(STFTwistConstraintBack), new STFTwistConstraintBackExporter()},
			{typeof(STFTwistConstraintForward), new STFTwistConstraintForwardExporter()},
			{typeof(STFAnimationHolder), new STFAnimationHolderExporter()}
		};
		public static readonly Dictionary<string, ASTFResourceImporter> DefaultResourceImporters = new Dictionary<string, ASTFResourceImporter>() {
			{STFMeshImporter._TYPE, new STFMeshImporter()},
			{STFTextureImporter._TYPE, new STFTextureImporter()},
			{STFArmatureImporter._TYPE, new STFArmatureImporter()},
			{STFMaterialImporter._TYPE, new STFMaterialImporter()},
			{STFTextureViewImporter._TYPE, new STFTextureViewImporter()},
			{STFAnimationImporter._TYPE, new STFAnimationImporter()}
		};
		public static readonly Dictionary<Type, ASTFResourceExporter> DefaultResourceExporters = new Dictionary<Type, ASTFResourceExporter>() {
			{typeof(STFArmature), new STFArmatureExporter()},
			{typeof(Mesh), new STFMeshExporter()},
			{typeof(Texture2D), new STFTextureExporter()},
			{typeof(Material), new STFMaterialExporter()},
#if UNITY_EDITOR
			{typeof(AnimationClip), new STFAnimationExporter()}
#endif
		};

		public static readonly Dictionary<Type, ISTFAnimationPathTranslator> DefaultAnimationTranslators = new Dictionary<Type, ISTFAnimationPathTranslator>() {
			{typeof(Transform), new STFTransformAnimationPathTranslator()},
			{typeof(GameObject), new STFTransformAnimationPathTranslator()},
			{typeof(SkinnedMeshRenderer), new SkinnedMeshRendererAnimationPathTranslator()}
		};

		private static Dictionary<string, ISTFAssetImporter> RegisteredAssetImporters = new Dictionary<string, ISTFAssetImporter>();

		private static Dictionary<string, ISTFNodeImporter> RegisteredNodeImporters = new Dictionary<string, ISTFNodeImporter>();

		private static Dictionary<string, ASTFComponentImporter> RegisteredComponentImporters = new Dictionary<string, ASTFComponentImporter>();
		private static Dictionary<Type, ASTFComponentExporter> RegisteredComponentExporters = new Dictionary<Type, ASTFComponentExporter>();

		private static Dictionary<string, ASTFResourceImporter> RegisteredResourceImporters = new Dictionary<string, ASTFResourceImporter>();
		private static Dictionary<Type, ASTFResourceExporter> RegisteredResourceExporters = new Dictionary<Type, ASTFResourceExporter>();
		
		public static readonly Dictionary<Type, ISTFAnimationPathTranslator> RegisteredAnimationTranslators = new Dictionary<Type, ISTFAnimationPathTranslator>();

		public static void RegisterAssetImporter(string type, ISTFAssetImporter importer) { RegisteredAssetImporters.Add(type, importer); }
		public static void RegisterNodeImporter(string type, ISTFNodeImporter importer) { RegisteredNodeImporters.Add(type, importer); }
		public static void RegisterComponentImporter(string type, ASTFComponentImporter importer) { RegisteredComponentImporters.Add(type, importer); }
		public static void RegisterComponentExporter(Type type, ASTFComponentExporter exporter) { RegisteredComponentExporters.Add(type, exporter); }
		public static void RegisterResourceImporter(string type, ASTFResourceImporter importer) { RegisteredResourceImporters.Add(type, importer); }
		public static void RegisterResourceExporter(Type type, ASTFResourceExporter exporter) { RegisteredResourceExporters.Add(type, exporter); }
		public static void RegisterAnimationTranslators(Type type, ISTFAnimationPathTranslator translator) { RegisteredAnimationTranslators.Add(type, translator); }

		public static bool IsAssetImporterRegistered(string type) { return RegisteredAssetImporters.ContainsKey(type); }
		public static bool IsNodeImporterRegistered(string type) { return RegisteredNodeImporters.ContainsKey(type); }
		public static bool IsComponentImporterRegistered(string type) { return RegisteredComponentImporters.ContainsKey(type); }
		public static bool IsComponentExporterRegistered(Type type) { return RegisteredComponentExporters.ContainsKey(type); }
		public static bool IsResourceImporterRegistered(string type) { return RegisteredResourceImporters.ContainsKey(type); }
		public static bool IsResourceExporterRegistered(Type type) { return RegisteredResourceExporters.ContainsKey(type); }
		public static bool IsAnimationTranslatorRegistered(Type type) { return RegisteredAnimationTranslators.ContainsKey(type); }

		public static ISTFAssetImporter GetAssetImporter(string type) { return RegisteredAssetImporters[type]; }
		public static ISTFNodeImporter GetNodeImporter(string type) { return RegisteredNodeImporters[type]; }
		public static ASTFComponentImporter GetComponentImporter(string type) { return RegisteredComponentImporters[type]; }
		public static ASTFComponentExporter GetComponentExporter(Type type) { return RegisteredComponentExporters[type]; }
		public static ASTFResourceImporter GetResourceImporter(string type) { return RegisteredResourceImporters[type]; }
		public static ASTFResourceExporter GetResourceExporter(Type type) { return RegisteredResourceExporters[type]; }
		public static ISTFAnimationPathTranslator GetAnimationTranslator(Type type) { return RegisteredAnimationTranslators[type]; }

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

			var componentImporters = new Dictionary<string, ASTFComponentImporter>(DefaultComponentImporters);
			foreach(var e in RegisteredComponentImporters)
			{
				if(componentImporters.ContainsKey(e.Key)) componentImporters[e.Key] = e.Value;
				else componentImporters.Add(e.Key, e.Value);
			}

			var resourceImporters = new Dictionary<string, ASTFResourceImporter>(DefaultResourceImporters);
			foreach(var e in RegisteredResourceImporters)
			{
				if(resourceImporters.ContainsKey(e.Key)) resourceImporters[e.Key] = e.Value;
				else resourceImporters.Add(e.Key, e.Value);
			}

			var animationTranslators = new Dictionary<Type, ISTFAnimationPathTranslator>(DefaultAnimationTranslators);
			foreach(var e in RegisteredAnimationTranslators)
			{
				if(animationTranslators.ContainsKey(e.Key)) animationTranslators[e.Key] = e.Value;
				else animationTranslators.Add(e.Key, e.Value);
			}

			return new STFImportContext() {
				AssetImporters = assetImporters,
				NodeImporters = nodeImporters,
				ComponentImporters = componentImporters,
				ResourceImporters = resourceImporters,
				AnimationTranslators = animationTranslators
			};
		}

		public static STFExportContext GetDefaultExportContext()
		{
			var componentExporters = new Dictionary<Type, ASTFComponentExporter>(DefaultComponentExporters);
			foreach(var e in RegisteredComponentExporters)
			{
				if(componentExporters.ContainsKey(e.Key)) componentExporters[e.Key] = e.Value;
				else componentExporters.Add(e.Key, e.Value);
			}

			var resourceExporters = new Dictionary<Type, ASTFResourceExporter>(DefaultResourceExporters);
			foreach(var e in RegisteredResourceExporters)
			{
				if(resourceExporters.ContainsKey(e.Key)) resourceExporters[e.Key] = e.Value;
				else resourceExporters.Add(e.Key, e.Value);
			}

			var animationTranslators = new Dictionary<Type, ISTFAnimationPathTranslator>(DefaultAnimationTranslators);
			foreach(var e in RegisteredAnimationTranslators)
			{
				if(animationTranslators.ContainsKey(e.Key)) animationTranslators[e.Key] = e.Value;
				else animationTranslators.Add(e.Key, e.Value);
			}

			return new STFExportContext() {
				ComponentExporters = componentExporters,
				ResourceExporters = resourceExporters,
				AnimationTranslators = animationTranslators
			};
		}
	}
}

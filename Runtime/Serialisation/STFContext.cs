
using System.Collections.Generic;
using System;
using stf.Components;

namespace stf.serialisation
{
	public class STFImportContext
	{
		public Dictionary<string, ISTFAssetImporter> AssetImporters = new Dictionary<string, ISTFAssetImporter>();
		public Dictionary<string, ISTFNodeImporter> NodeImporters = new Dictionary<string, ISTFNodeImporter>();
		public Dictionary<string, ASTFComponentImporter> ComponentImporters = new Dictionary<string, ASTFComponentImporter>();
		public Dictionary<string, ASTFResourceImporter> ResourceImporters = new Dictionary<string, ASTFResourceImporter>();
		public Dictionary<Type, ISTFAnimationPathTranslator> AnimationTranslators = new Dictionary<Type, ISTFAnimationPathTranslator>();
	}

	public class STFExportContext
	{
		//Assets
		//public Dictionary<string, ASTFNodeExporter> NodeExporters = new Dictionary<string, ASTFNodeExporter>();
		public Dictionary<Type, ASTFComponentExporter> ComponentExporters = new Dictionary<Type, ASTFComponentExporter>();
		public Dictionary<Type, ASTFResourceExporter> ResourceExporters = new Dictionary<Type, ASTFResourceExporter>();
		public Dictionary<Type, ISTFAnimationPathTranslator> AnimationTranslators = new Dictionary<Type, ISTFAnimationPathTranslator>();
	}
}

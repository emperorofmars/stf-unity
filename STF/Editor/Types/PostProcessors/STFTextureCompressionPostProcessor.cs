
#if UNITY_EDITOR

using System;
using System.Linq;
using STF.Types;
using UnityEditor;
using static STF.Util.STFConstants;

namespace STF.Serialisation
{
	public class STFTextureCompressionPostProcessor : ISTFImportPostProcessor
	{
		public STFObjectType STFObjectType => STFObjectType.Resource;

		public Type TargetType => typeof(EditorSTFTexture);

		public void PostProcess(STFImportState State, object Resource)
		{
			var meta = (EditorSTFTexture)Resource;
			TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(meta.Resource));

			if(meta && textureImporter)
			{
				var textureCompression = meta.Components.FirstOrDefault(c => c.Type == STFTextureCompression._TYPE);
				if(textureCompression)
				{
					textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
					textureImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", format = TextureImporterFormat.BC7 });
					
					textureImporter.SaveAndReimport();
				}
			}
		}
	}

	[InitializeOnLoad]
	public class Register_STFTextureCompressionPostProcessor
	{
		static Register_STFTextureCompressionPostProcessor()
		{
			STFRegistry.RegisterImportPostProcessor(new STFTextureCompressionPostProcessor());
		}
	}
}

#endif

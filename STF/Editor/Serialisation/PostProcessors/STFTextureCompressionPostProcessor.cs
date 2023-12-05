
#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static STF.Serialisation.STFConstants;

namespace STF.Serialisation
{
	public class STFTextureCompressionPostProcessor : ISTFImportPostProcessor
	{
		public STFObjectType STFObjectType => STFObjectType.Resource;

		public Type TargetType => typeof(STFTexture);

		public void PostProcess(ISTFImportState State, object Resource)
		{
			var meta = (STFTexture)Resource;
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

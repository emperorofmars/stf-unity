
#if UNITY_EDITOR

using System;
using UnityEditor;
using static STF.Util.STFConstants;

namespace STF.Serialisation
{
	public class STFTexturePostProcessor : ISTFImportPostProcessor
	{
		public STFObjectType STFObjectType => STFObjectType.Resource;

		public Type TargetType => typeof(STFTexture);

		public void PostProcess(STFImportState State, object Resource)
		{
			var meta = (STFTexture)Resource;
			TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(meta.Resource));
			if(meta && textureImporter)
			{
				textureImporter.sRGBTexture = meta.TextureType == "color";
				if(meta.TextureType == "normal") textureImporter.textureType = TextureImporterType.NormalMap;

				textureImporter.maxTextureSize = Math.Max(meta.TextureSize.x, meta.TextureSize.y);

				textureImporter.SaveAndReimport();
			}
		}
	}

	[InitializeOnLoad]
	public class Register_STFTexturePostProcessor
	{
		static Register_STFTexturePostProcessor()
		{
			STFRegistry.RegisterImportPostProcessor(new STFTexturePostProcessor());
		}
	}
}

#endif

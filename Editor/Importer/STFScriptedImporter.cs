
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using stf.serialisation;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace stf
{

	[ScriptedImporter(1, "stf")]
	public class STFScriptedImporter : ScriptedImporter
	{
		public bool AuthoringMode = false;
		public string OriginalTexturesFolder = "Assets/authoring_stf_external";

		private void ensureTexturePath()
		{
			if(!Directory.Exists(OriginalTexturesFolder))
			{
				Directory.CreateDirectory(OriginalTexturesFolder);
				AssetDatabase.Refresh();
			}
		}
		
		public override void OnImportAsset(AssetImportContext ctx)
		{
			byte[] byteArray = File.ReadAllBytes(ctx.assetPath);

			var context = STFRegistry.GetDefaultImportContext();
			if(AuthoringMode)
			{
				var image_bullshit = new STFEncodedImageTextureImporter();
				ensureTexturePath();
				image_bullshit.imageParentPath = OriginalTexturesFolder;
				context.ResourceImporters[STFEncodedImageTextureImporter._TYPE] = image_bullshit;
			}
			var importer = new STFImporter(byteArray, context);
			
			foreach(var resource in importer.GetResources())
			{
				if(resource != null)
				{
					ctx.AddObjectToAsset(resource.name, resource);
				}
			}
			importer.GetMeta().name = "STFMeta";
			ctx.AddObjectToAsset("STFMeta", importer.GetMeta());
			foreach(var asset in importer.GetAssets())
			{
				ctx.AddObjectToAsset(asset.Key, asset.Value.GetAsset());
			}
			ctx.SetMainObject(importer.GetAssets()[importer.mainAssetId].GetAsset());
		}
	}
}

#endif

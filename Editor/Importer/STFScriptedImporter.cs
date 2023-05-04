
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
		public override void OnImportAsset(AssetImportContext ctx)
		{
			byte[] byteArray = File.ReadAllBytes(ctx.assetPath);

			var context = STFRegistry.GetDefaultImportContext();
			var image_bullshit = new STFEncodedImageTextureImporter();
			
			var path = Path.GetDirectoryName(ctx.assetPath);
			path = path + "/" + Path.GetFileName(ctx.assetPath) + "_textures";
			image_bullshit.imageParentPath = path;
			if(!Directory.Exists(image_bullshit.imageParentPath))
			{
				Directory.CreateDirectory(path);
				AssetDatabase.Refresh();
			}

			context.ResourceImporters[STFEncodedImageTextureImporter._TYPE] = image_bullshit;

			Debug.Log("AAAAAAAAAAAA: " + ((STFEncodedImageTextureImporter)context.ResourceImporters[STFEncodedImageTextureImporter._TYPE]).imageParentPath);

			var importer = new STFImporter(byteArray, context);
			
			foreach(var resource in importer.GetResources())
			{
				if(resource != null)
					ctx.AddObjectToAsset(resource.name, resource);
			}
			foreach(var asset in importer.GetAssets())
			{
				ctx.AddObjectToAsset(asset.Key, asset.Value.GetAsset());
			}
			ctx.SetMainObject(importer.GetAssets()[importer.mainAssetId].GetAsset());
		}
	}
}

#endif

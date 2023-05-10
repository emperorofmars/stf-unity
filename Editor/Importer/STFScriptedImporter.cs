
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using stf.serialisation;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace stf
{

	[ScriptedImporter(1, "stf")]
	public class STFScriptedImporter : ScriptedImporter
	{
		[HideInInspector]
		public bool SafeImagesExternal = false;
		[HideInInspector]
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
			if(SafeImagesExternal)
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
					if(resource.GetType() == typeof(Mesh))
						ctx.AddObjectToAsset("meshes/" + resource.name + ".mesh", resource);
					else if(resource.GetType() == typeof(Texture2D))
						ctx.AddObjectToAsset("textures/" + resource.name + ".texture2d", resource);
					else
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

	[CustomEditor(typeof(STFScriptedImporter))]
	public class Car_Inspector : Editor
	{
		public override void OnInspectorGUI()
		{
			var importer = (STFScriptedImporter)target;
			base.DrawDefaultInspector();
			
			GUILayout.Space(10f);
			 importer.SafeImagesExternal = GUILayout.Toggle(importer.SafeImagesExternal, "Save images to external folder");
			if(importer.SafeImagesExternal) GUILayout.Label($"External image location: {importer.OriginalTexturesFolder}");
			if(importer.SafeImagesExternal && GUILayout.Button("Choose external image location", GUILayout.ExpandWidth(true))) {
				importer.OriginalTexturesFolder = EditorUtility.OpenFolderPanel("Export Standalone", "Assets", "authoring_stf_external");
				if(importer.OriginalTexturesFolder != null && importer.OriginalTexturesFolder.Length > 0)
				{
					if (importer.OriginalTexturesFolder.StartsWith(Application.dataPath)) {
						importer.OriginalTexturesFolder = "Assets" + importer.OriginalTexturesFolder.Substring(Application.dataPath.Length);
					}
				}
			}

			GUILayout.Space(10f);
			GUILayout.Label("Detected Main Assets", EditorStyles.boldLabel);

			var assets = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath);
			foreach(var asset in assets.Where(a => a.GetType() == typeof(STFAssetInfo) && ((STFAssetInfo)a).assetType == "asset"))
			{
				GUILayout.Space(5f);
				var a = (STFAssetInfo)asset;
				GUILayout.Label($"Name: {a.assetName}, Type: {a.assetType}, ID: {a.assetId}");
				if(GUILayout.Button("Instantiate into current scene"))
				{
					var instantiated = Object.Instantiate(a);
					instantiated.name = a.name;
				}
			}
			
			GUILayout.Space(20f);
			if(GUILayout.Button("Save and reimport"))
			{
				EditorUtility.SetDirty(importer);
				importer.SaveAndReimport();
			}
		}
	}
}

#endif

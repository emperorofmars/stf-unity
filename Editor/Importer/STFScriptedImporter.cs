
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
		public bool AuthoringMode = false;
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
			importer.AuthoringMode = GUILayout.Toggle(importer.AuthoringMode, "Authoring Mode");
			if(importer.AuthoringMode) importer.SafeImagesExternal = GUILayout.Toggle(importer.SafeImagesExternal, "Save Images To External Folder");
			if(importer.AuthoringMode && importer.SafeImagesExternal) GUILayout.Label($"External Image Location: {importer.OriginalTexturesFolder}");
			if(importer.AuthoringMode && importer.SafeImagesExternal && GUILayout.Button("Choose External Image Location", GUILayout.ExpandWidth(true))) {
				importer.OriginalTexturesFolder = EditorUtility.OpenFolderPanel("Export Standalone", "Assets", "authoring_stf_external");
			}

			GUILayout.Space(10f);
			GUILayout.Label("Detected Main Assets", EditorStyles.boldLabel);

			var assets = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath);
			foreach(var asset in assets.Where(a => a.GetType() == typeof(STFAssetInfo) && ((STFAssetInfo)a).assetType == "asset"))
			{
				var a = (STFAssetInfo)asset;
				GUILayout.Label($"Name: {a.assetName}, Type: {a.assetType}, ID: {a.assetId}");

			}
		}
	}
}

#endif


#if UNITY_EDITOR

using System.IO;
using System.Linq;
using stf.serialisation;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace stf
{
	[ScriptedImporter(1, new string[] {"stf"})]
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
			var icon = new Texture2D(256, 256);
			icon.LoadImage(STFIcon.icon_png_array.ToArray());

			ctx.AddObjectToAsset("STFMeta", importer.GetMeta(), icon);
			//ctx.SetMainObject(importer.GetMeta());

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
			base.DrawDefaultInspector();

			var importer = (STFScriptedImporter)target;
			var meta = AssetDatabase.LoadAssetAtPath<STFMeta>(importer.assetPath);
			//var meta = (STFMeta)AssetDatabase.LoadMainAssetAtPath(importer.assetPath);
			
			GUILayout.Label("File Info", EditorStyles.boldLabel);
			GUILayout.Space(5);

			if(meta)
			{
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
					GUILayout.Label("Binary Version");
					GUILayout.Label("Definition Version");
					GUILayout.Label("Author");
					GUILayout.Label("Copyright");
					GUILayout.Label("Generator");
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
					GUILayout.Label(meta.versionBinary);
					GUILayout.Label(meta.versionDefinition);
					GUILayout.Label(meta.author);
					GUILayout.Label(meta.copyright);
					GUILayout.Label(meta.generator);
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}

			drawHLine();
			
			GUILayout.Label("STF Import Settings", EditorStyles.boldLabel);
			GUILayout.Space(5);

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

			drawHLine();

			GUILayout.Label("Imported Assets", EditorStyles.boldLabel);
			GUILayout.Space(5);

			// Second Stage Assets

			drawHLine();

			GUILayout.Label("Authoring", EditorStyles.boldLabel);
			GUILayout.Space(5);
			importer.AuthoringMode = GUILayout.Toggle(importer.AuthoringMode, "Authoring Mode");
			if(importer.AuthoringMode)
			{
				GUILayout.Space(10f);
				GUILayout.Label("Imported Raw Assets", EditorStyles.boldLabel);
				
				if(meta != null && meta.importedRawAssets != null)
				{
					var mainAsset = meta.importedRawAssets.Find(a => a.assetId == meta.mainAssetId);
					GUILayout.Space(5f);
					GUILayout.Label("Main", EditorStyles.boldLabel);
					GUILayout.Label($"Name: {mainAsset.assetName} | Type: {mainAsset.assetType} | Id: {mainAsset.assetId}");
					if(GUILayout.Button("Instantiate into current scene"))
					{
						var instantiated = Object.Instantiate(mainAsset.assetRoot);
						instantiated.name = mainAsset.assetRoot.name;
					}
					if(meta.importedRawAssets.Count > 1)
					{
						GUILayout.Space(5f);
						GUILayout.Label("Secondary", EditorStyles.boldLabel);
						foreach(var asset in meta.importedRawAssets.FindAll(a => a.assetId != meta.mainAssetId))
						{
							GUILayout.Space(5f);
							GUILayout.Label($"Name: {asset.assetName}, Type: {asset.assetType}, ID: {asset.assetId}");
							if(GUILayout.Button("Instantiate into current scene"))
							{
								var instantiated = Object.Instantiate(asset.assetRoot);
								instantiated.name = asset.assetRoot.name;
							}
						}
					}
				}
			}
			
			drawHLine();

			GUILayout.Space(20f);
			if(GUILayout.Button("Save and reimport"))
			{
				EditorUtility.SetDirty(importer);
				importer.SaveAndReimport();
			}
		}

		private void drawHLine() {
			GUILayout.Space(10);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.gray);
			GUILayout.Space(10);
		}
	}
}

#endif

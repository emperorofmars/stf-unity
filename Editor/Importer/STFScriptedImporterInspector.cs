
#if UNITY_EDITOR

using System.IO;
using System.Linq;
using stf.serialisation;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace stf
{
	[CustomEditor(typeof(STFScriptedImporter))]
	public class STFScriptedImporterInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			base.DrawDefaultInspector();

			var importer = (STFScriptedImporter)target;
			var meta = AssetDatabase.LoadAssetAtPath<STFMeta>(importer.assetPath);
			
			GUILayout.Label("File Info", EditorStyles.boldLabel);
			GUILayout.Space(5);

			if(meta)
			{
				EditorGUI.indentLevel++;

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
					EditorGUILayout.LabelField("Binary Version");
					EditorGUILayout.LabelField("Definition Version");
					EditorGUILayout.LabelField("Author");
					EditorGUILayout.LabelField("Copyright");
					EditorGUILayout.LabelField("Generator");
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical();
					EditorGUILayout.LabelField(meta.versionBinary);
					EditorGUILayout.LabelField(meta.versionDefinition);
					EditorGUILayout.LabelField(meta.author);
					EditorGUILayout.LabelField(meta.copyright);
					EditorGUILayout.LabelField(meta.generator);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				EditorGUI.indentLevel--;
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

			GUILayout.Space(10f);
			GUILayout.Label("Imported Assets", EditorStyles.boldLabel);
			
			if(meta != null && meta.importedRawAssets != null)
			{
				var mainAsset = meta.importedRawAssets.Find(a => a.assetId == meta.mainAssetId);
				GUILayout.Space(5f);
				EditorGUILayout.LabelField("Main", EditorStyles.boldLabel);

				EditorGUI.indentLevel++;
				renderAsset(mainAsset);
				EditorGUI.indentLevel--;

				if(meta.importedRawAssets.Count > 1)
				{
					EditorGUILayout.LabelField("Secondary", EditorStyles.boldLabel);
					foreach(var asset in meta.importedRawAssets.FindAll(a => a.assetId != meta.mainAssetId))
					{
						EditorGUI.indentLevel++;
						renderAsset(asset);
						EditorGUI.indentLevel--;
					}
				}
			}

			// handle unparented patch assets

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

		private void renderAsset(STFMeta.AssetInfo assetInfo)
		{
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"Name: {assetInfo.assetName} | Type: {assetInfo.assetType}");
			if(GUILayout.Button("Instantiate authoring scene", GUILayout.Width(200)))
			{
				var instantiated = Object.Instantiate(assetInfo.assetRoot, new Vector3(0, 0, 0), Quaternion.identity);
				instantiated.name = assetInfo.assetRoot.name;
			}
			GUILayout.EndHorizontal();

			// handle patch assets to apply before passing to the second stage loader

			if(assetInfo.secondStageAssets != null && assetInfo.secondStageAssets.Count > 0)
			{
				EditorGUILayout.LabelField("Processed Assets", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				foreach(var asset in assetInfo.secondStageAssets)
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField($"Name: {asset.assetName} | Type: {asset.assetType}");
					if(GUILayout.Button("Instantiate into current scene"))
					{
						var instantiated = Object.Instantiate(asset.assetRoot, new Vector3(0, 0, 0), Quaternion.identity);
						instantiated.name = asset.assetRoot.name;
					}
					GUILayout.EndHorizontal();
				}
				EditorGUI.indentLevel--;
			}
		}
	}
}

#endif

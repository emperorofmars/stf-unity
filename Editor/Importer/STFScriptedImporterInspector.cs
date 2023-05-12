
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
		bool _foldoutFileInfo = true;
		bool _foldoutImportSettings = true;
		bool _foldoutImportedAssets = true;
		int _numRegisteredStages = STFImporterStageRegistry.GetStages().Count;

		public override void OnInspectorGUI()
		{
			base.DrawDefaultInspector();

			var importer = (STFScriptedImporter)target;
			var meta = AssetDatabase.LoadAssetAtPath<STFMeta>(importer.assetPath);
			
			_foldoutFileInfo = EditorGUILayout.Foldout(_foldoutFileInfo, "File Info", true, EditorStyles.foldoutHeader);
			if(_foldoutFileInfo)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
					EditorGUILayout.LabelField("Binary Version");
					EditorGUILayout.LabelField("Definition Version");
					EditorGUILayout.LabelField("Author");
					EditorGUILayout.LabelField("Copyright");
					EditorGUILayout.LabelField("Generator");
				EditorGUILayout.EndVertical();
					EditorGUILayout.BeginVertical();
					if(meta)
					{
						EditorGUILayout.LabelField(meta.versionBinary);
						EditorGUILayout.LabelField(meta.versionDefinition);
						EditorGUILayout.LabelField(meta.author);
						EditorGUILayout.LabelField(meta.copyright);
						EditorGUILayout.LabelField(meta.generator);
					}
					else
					{
						EditorGUILayout.LabelField("-");
						EditorGUILayout.LabelField("-");
						EditorGUILayout.LabelField("-");
						EditorGUILayout.LabelField("-");
						EditorGUILayout.LabelField("-");
					}
					EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}

			drawHLine();
			
			_foldoutImportSettings = EditorGUILayout.Foldout(_foldoutImportSettings, "STF Import Settings", true, EditorStyles.foldoutHeader);
			if(_foldoutImportSettings)
			{
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
			}

			drawHLine();

			_foldoutImportedAssets = EditorGUILayout.Foldout(_foldoutImportedAssets, "Imported Assets", true, EditorStyles.foldoutHeader);
			
			if(_foldoutImportedAssets && meta != null && meta.importedRawAssets != null)
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
			if(_numRegisteredStages != STFImporterStageRegistry.GetStages().Count)
			{
				EditorGUILayout.LabelField("New Stage Registered, press 'Safe and Reimport' to show effect!", EditorStyles.boldLabel);
				GUILayout.Space(5f);
			}
			if(GUILayout.Button("Save and reimport"))
			{
				_numRegisteredStages = STFImporterStageRegistry.GetStages().Count;
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
				var instantiated = PrefabUtility.InstantiatePrefab(assetInfo.assetRoot as GameObject);
			}
			GUILayout.EndHorizontal();

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
						var instantiated = PrefabUtility.InstantiatePrefab(asset.assetRoot as GameObject);
					}
					GUILayout.EndHorizontal();
				}
				EditorGUI.indentLevel--;
			}
		}
	}
}

#endif

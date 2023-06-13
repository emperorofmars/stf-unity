
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

		/*protected override void OnHeaderGUI()
		{
			//base.DrawHeader();
			var importer = (STFScriptedImporter)target;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("STF Import Settings", EditorStyles.whiteLargeLabel);
			EditorGUILayout.EndHorizontal();

			var rect = EditorGUILayout.GetControlRect(false, 0f);
			rect.height = EditorGUIUtility.singleLineHeight;
			rect.y -= rect.height;
			rect.x = 48;
			rect.xMax -= rect.x * 2f;
	
			EditorGUI.DrawRect(rect, new Color32 (56, 56, 56, 255));
	
			EditorGUI.LabelField(rect, "STF Import Settings", EditorStyles.boldLabel);
		}*/

		public override void OnInspectorGUI()
		{
			//base.DrawDefaultInspector();
			EditorGUILayout.LabelField("STF Import Settings", EditorStyles.whiteLargeLabel);
			GUILayout.Space(10f);

			var importer = (STFScriptedImporter)target;
			var meta = AssetDatabase.LoadAssetAtPath<STFMeta>(importer.assetPath);
			
			_foldoutFileInfo = EditorGUILayout.Foldout(_foldoutFileInfo, "File Info", true, EditorStyles.foldoutHeader);
			if(_foldoutFileInfo)
			{
				GUILayout.Space(5f);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Binary Version");
				if(meta) EditorGUILayout.LabelField(meta.versionBinary);
				else EditorGUILayout.LabelField("-");
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Definition Version");
				if(meta) EditorGUILayout.LabelField(meta.versionDefinition);
				else EditorGUILayout.LabelField("-");
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Author");
				if(meta) EditorGUILayout.LabelField(meta.author);
				else EditorGUILayout.LabelField("-");
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Copyright");
				if(meta) EditorGUILayout.LabelField(meta.copyright);
				else EditorGUILayout.LabelField("-");
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Generator");
				if(meta) EditorGUILayout.LabelField(meta.generator);
				else EditorGUILayout.LabelField("-");
				EditorGUILayout.EndHorizontal();
			}

			drawHLine();
			
			_foldoutImportSettings = EditorGUILayout.Foldout(_foldoutImportSettings, "STF Import Settings", true, EditorStyles.foldoutHeader);

			EditorGUI.BeginChangeCheck();
			if(_foldoutImportSettings)
			{
				GUILayout.Space(5f);
				importer.SafeImagesExternal = GUILayout.Toggle(importer.SafeImagesExternal, "Save images to external folder");
				GUILayout.Space(5f);
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
				renderAsset(mainAsset, importer);
				EditorGUI.indentLevel--;

				if(meta.importedRawAssets.Count > 1)
				{
					EditorGUILayout.LabelField("Secondary", EditorStyles.boldLabel);
					foreach(var asset in meta.importedRawAssets.FindAll(a => a.assetId != meta.mainAssetId))
					{
						EditorGUI.indentLevel++;
						renderAsset(asset, importer);
						EditorGUI.indentLevel--;
					}
				}
			}

			// handle unparented patch assets

			var changesDetected = EditorGUI.EndChangeCheck();
			drawHLine();

			GUILayout.Space(20f);
			if(_numRegisteredStages != STFImporterStageRegistry.GetStages().Count)
			{
				EditorGUILayout.LabelField("New Stage registered, press 'Safe and Reimport' to show effect!", EditorStyles.boldLabel);
				GUILayout.Space(5f);
			}
			if(changesDetected)
			{
				EditorGUILayout.LabelField("Settings changed, press 'Safe and Reimport' to show effect!", EditorStyles.boldLabel);
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

		private void renderAsset(STFMeta.AssetInfo assetInfo, STFScriptedImporter importer)
		{
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"Name: {assetInfo.assetName} | Type: {assetInfo.assetType}");
			if(GUILayout.Button("Instantiate authoring scene", GUILayout.Width(200)))
			{
				var instantiated = PrefabUtility.InstantiatePrefab(assetInfo.assetRoot as GameObject);
			}
			GUILayout.EndHorizontal();

			var addonList = importer.Addons.Find(k => k.TargetId == assetInfo.assetId);
			if(addonList != null)
			{
				EditorGUILayout.LabelField("Addons", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;

				for(int addonIdx = 0; addonIdx < addonList.Addons.Count; addonIdx++)
				{
					var enabled = importer.AddonsEnabled.Find(a => a.AddonId == addonList.AddonId);
					if(enabled != null && addonList.Addons[addonIdx] != null)
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(addonList.Addons[addonIdx].name);
						enabled.AddonEnabled = EditorGUILayout.Toggle(enabled.AddonEnabled);
						EditorGUILayout.EndHorizontal();
					}
					else
					{
						EditorGUILayout.LabelField("Addon Broken");
					}
				}
				EditorGUI.indentLevel--;
			}
			else
			{
				EditorGUILayout.LabelField("No addons registered.");
			}

			if(assetInfo.secondStageAssets != null && assetInfo.secondStageAssets.Count > 0)
			{
				EditorGUILayout.LabelField("Processed Assets", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				foreach(var asset in assetInfo.secondStageAssets)
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(asset.assetType);
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

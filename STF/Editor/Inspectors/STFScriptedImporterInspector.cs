
#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;
using STF.Serialisation;

namespace STF.Tools
{
	// A UI for the STFScriptedImporter.
	// Apart from showing information about the file, it adds the ability to easily toggle which addons to apply for the second stages.
	// It also has a button to instantiate the resulting scene of each second-stage, as well as the 'authoring scene' (the STF-Unity intermediary format), for each asset included in the STF file.
	[CustomEditor(typeof(STFScriptedImporter))]
	public class STFScriptedImporterInspector : Editor
	{
		bool _foldoutImportedAssets = true;

		public override void OnInspectorGUI()
		{
			var importer = (STFScriptedImporter)target;
			var importInfo = AssetDatabase.LoadAssetAtPath<STFImportInfo>(importer.assetPath);

			GUILayout.Space(10f);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Binary Version");
			if(importInfo != null && importInfo.Buffers != null) EditorGUILayout.LabelField(importInfo.Buffers.VersionMajor + "." + importInfo.Buffers.VersionMinor);
			else EditorGUILayout.LabelField("-");
			EditorGUILayout.EndHorizontal();

			var mainAsset = importInfo.Assets?.Find(a => a.assetId == importInfo.MainAssetId);

			drawHLine();

			EditorGUILayout.LabelField("Import", EditorStyles.whiteLargeLabel);

			var selectedUnpackLocation = importer.UnpackFolder.Length > 0 ? importer.UnpackFolder : STFDirectoryUtil.DefaultUnpackFolder + "/" + Path.GetFileNameWithoutExtension(importer.assetPath);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Selected Location:");
			EditorGUILayout.LabelField("Assets" + selectedUnpackLocation);
			EditorGUILayout.EndHorizontal();

			if(GUILayout.Button("Select New Import Location", GUILayout.ExpandWidth(false))) {
				selectedUnpackLocation = EditorUtility.OpenFolderPanel("Select New Import Location", "Assets", selectedUnpackLocation);
				importer.UnpackFolder = selectedUnpackLocation;
				if(importer.UnpackFolder != null && importer.UnpackFolder.Length > 0)
				{
					if (importer.UnpackFolder.StartsWith(Application.dataPath)) {
						importer.UnpackFolder = importer.UnpackFolder.Substring(Application.dataPath.Length);
					}
				}
			}

			GUILayout.Space(10f);
			if(GUILayout.Button("Import To Selected Location", GUILayout.ExpandWidth(true))) {
				STFDirectoryUtil.EnsureDefaultUnpackFolder(Path.GetFileNameWithoutExtension(importer.assetPath));
				var deserializer = new STFImporter("Assets" + selectedUnpackLocation, importer.assetPath);
			}

			drawHLine();

			EditorGUILayout.LabelField("Main Asset", EditorStyles.whiteLargeLabel);

			EditorGUI.indentLevel++;
			renderAsset(mainAsset, importer);
			if(GUILayout.Button("Instantiate", GUILayout.ExpandWidth(false))) {
				var assetObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path.Combine("Assets" + selectedUnpackLocation, mainAsset.assetName + ".Prefab"));
				PrefabUtility.InstantiatePrefab(assetObject);
			}
			EditorGUI.indentLevel--;

			drawHLine();

			GUILayout.Space(5f);
			_foldoutImportedAssets = EditorGUILayout.Foldout(_foldoutImportedAssets, "Secondary Assets", true, EditorStyles.foldoutHeader);
			if(_foldoutImportedAssets && importInfo != null && importInfo.Assets != null && importInfo.Assets.Count > 1)
			{
				foreach(var asset in importInfo.Assets.FindAll(a => a.assetId != importInfo.MainAssetId))
				{
					EditorGUI.indentLevel++;
					renderAsset(asset, importer);
					if(GUILayout.Button("Instantiate", GUILayout.ExpandWidth(false))) {
						var assetObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path.Combine("Assets" + selectedUnpackLocation, STFConstants.SecondaryAssetsDirectoryName, asset.assetName + ".Prefab"));
						PrefabUtility.InstantiatePrefab(assetObject);
					}
					EditorGUI.indentLevel--;
				}
			}

			drawHLine();

			if(GUILayout.Button("Refresh"))
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

		private void renderAsset(STFAssetInfo asset, STFScriptedImporter importer)
		{
			if(asset != null)
			{

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Type");
				EditorGUILayout.LabelField(asset.assetType);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Name");
				EditorGUILayout.LabelField(asset.assetName);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Version");
				EditorGUILayout.LabelField(asset.assetVersion);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Author");
				EditorGUILayout.LabelField(asset.assetAuthor);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("URL");
				EditorGUILayout.LabelField(asset.assetURL);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("License");
				EditorGUILayout.LabelField(asset.assetLicense);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("License Link");
				EditorGUILayout.LabelField(asset.assetLicenseLink);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Id");
				EditorGUILayout.LabelField(asset.assetId);
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				EditorGUILayout.LabelField("Invalid Asset");
			}
		}
	}
}

#endif

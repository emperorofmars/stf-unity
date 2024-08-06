
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

			drawHLine();
			//EditorGUI.indentLevel++;
			renderAsset(importInfo, importer);
			drawHLine();


			if(importInfo != null && Directory.Exists(Path.Combine(STFDirectoryUtil.GetUnpackLocation(importer.assetPath))) && File.Exists(Path.Combine(STFDirectoryUtil.GetUnpackLocation(importer.assetPath), importInfo.Name + ".Prefab")))
			{
				if(GUILayout.Button("Instantiate", GUILayout.ExpandWidth(true))) {
					var assetObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path.Combine(STFDirectoryUtil.GetUnpackLocation(importer.assetPath), importInfo.Name + ".Prefab"));
					PrefabUtility.InstantiatePrefab(assetObject);
				}
				GUILayout.Space(10f);
				if(GUILayout.Button("Reimport", GUILayout.ExpandWidth(true))) {
					STFDirectoryUtil.EnsureUnpackLocation(importer.assetPath);
					var deserializer = new STFUnpackingImporter(STFDirectoryUtil.GetUnpackLocation(importer.assetPath), importer.assetPath);
				}
			}
			else
			{
				//EditorGUILayout.LabelField("Import", EditorStyles.whiteLargeLabel);
				GUILayout.Space(10f);
				if(GUILayout.Button("Import", GUILayout.ExpandWidth(true))) {
					STFDirectoryUtil.EnsureUnpackLocation(importer.assetPath);
					var deserializer = new STFUnpackingImporter(STFDirectoryUtil.GetUnpackLocation(importer.assetPath), importer.assetPath);
				}
			}
			//EditorGUI.indentLevel--;

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

		private void renderAsset(STFImportInfo asset, STFScriptedImporter importer)
		{
			if(asset != null)
			{

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Type");
				EditorGUILayout.LabelField(asset.Type);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Name");
				EditorGUILayout.LabelField(asset.Name);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Version");
				EditorGUILayout.LabelField(asset.Version);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Author");
				EditorGUILayout.LabelField(asset.Author);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("URL");
				EditorGUILayout.LabelField(asset.URL);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("License");
				EditorGUILayout.LabelField(asset.License);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("License Link");
				EditorGUILayout.LabelField(asset.LicenseLink);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Id");
				EditorGUILayout.LabelField(asset.Id);
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				EditorGUILayout.LabelField("Invalid Asset");
			}
		}
		

		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			var importer = (STFScriptedImporter)target;
			var importInfo = AssetDatabase.LoadAssetAtPath<STFImportInfo>(importer.assetPath);

			if (importInfo == null || importInfo.Preview == null)
				return null;

			// example.PreviewIcon must be a supported format: ARGB32, RGBA32, RGB24,
			// Alpha8 or one of float formats
			Texture2D tex = new Texture2D (width, height);
			EditorUtility.CopySerialized (importInfo.Preview, tex);

			return tex;
		}
	}
}

#endif

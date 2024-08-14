
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
		bool ChangedUnpackingImport = false;

		public void OnEnable()
		{
			var importer = (STFScriptedImporter)target;
			ChangedUnpackingImport = importer.UnpackingImport;
		}

		public override void OnInspectorGUI()
		{
			var importer = (STFScriptedImporter)target;

			GUILayout.Space(10f);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Unpack into Filesystem");
			ChangedUnpackingImport = EditorGUILayout.Toggle(ChangedUnpackingImport);
			EditorGUILayout.EndHorizontal();

			//EditorGUILayout.LabelField(DirectoryUtil.AssetResourceFolder);

			if(ChangedUnpackingImport != importer.UnpackingImport && GUILayout.Button("Reimport to apply!"))
			{
				importer.UnpackingImport = ChangedUnpackingImport;
				EditorUtility.SetDirty(importer);
				importer.SaveAndReimport();
			}

			drawHLine();

			if(importer.AssetInfo != null)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Binary Version");
				if(importer.AssetInfo != null && importer.AssetInfo.Buffers != null) EditorGUILayout.LabelField(importer.AssetInfo.Buffers.VersionMajor + "." + importer.AssetInfo.Buffers.VersionMinor);
				else EditorGUILayout.LabelField("-");
				EditorGUILayout.EndHorizontal();
			}

			drawHLine();
			
			renderAsset(importer.AssetInfo);

			if(importer.UnpackingImport && importer.UnpackingImport == ChangedUnpackingImport)
			{
				drawHLine();
				
				if(importer.AssetInfo != null && Directory.Exists(Path.Combine(DirectoryUtil.GetUnpackLocation(importer.assetPath))) && File.Exists(Path.Combine(DirectoryUtil.GetUnpackLocation(importer.assetPath), importer.AssetInfo.Name + ".Prefab")))
				{
					if(GUILayout.Button("Instantiate", GUILayout.ExpandWidth(true))) {
						var assetObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path.Combine(DirectoryUtil.GetUnpackLocation(importer.assetPath), importer.AssetInfo.Name + ".Prefab"));
						PrefabUtility.InstantiatePrefab(assetObject);
					}
					GUILayout.Space(10f);
					if(GUILayout.Button("Reimport into filesystem", GUILayout.ExpandWidth(true))) {
						importUnpacking();
					}
				}
				else
				{
					GUILayout.Space(10f);
					if(GUILayout.Button("Import into filesystem", GUILayout.ExpandWidth(true))) {
						importUnpacking();
					}
				}

				drawHLine();

				if(GUILayout.Button("Refresh"))
				{
					EditorUtility.SetDirty(importer);
					importer.SaveAndReimport();
				}
			}
		}

		private void importUnpacking() {
			var importer = (STFScriptedImporter)target;

			DirectoryUtil.EnsureUnpackLocation(importer.assetPath);
			var unpackLocation = DirectoryUtil.GetUnpackLocation(importer.assetPath);

			var unityContext = new UnpackingUnityImportContext(unpackLocation);
			var (asset, _state) = Importer.Parse(unityContext, importer.assetPath);

			var path = Path.Combine(unpackLocation, asset.STFName + ".Prefab");
			PrefabUtility.SaveAsPrefabAsset(asset.gameObject, path);

			DestroyImmediate(asset.gameObject);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private void drawHLine() {
			GUILayout.Space(10);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.gray);
			GUILayout.Space(10);
		}

		private void renderAsset(STFImportInfo asset)
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

			if (importer.AssetInfo == null || importer.AssetInfo.Preview == null)
				return null;

			// example.PreviewIcon must be a supported format: ARGB32, RGBA32, RGB24,
			// Alpha8 or one of float formats
			Texture2D tex = new Texture2D (width, height);
			EditorUtility.CopySerialized(importer.AssetInfo.Preview, tex);

			return tex;
		}
	}
}

#endif

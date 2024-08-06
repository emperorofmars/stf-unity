
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using STF.Serialisation;
using UnityEditor;
using UnityEngine;

namespace STF.Tools
{
	// UI to export STF-Unity intermediary scenes into STF. Apart from selecting the main asset, optionally multiple secondary assets can be included into the export.

	public class STFExportEditor : EditorWindow
	{
		private Vector2 scrollPos;
		public GameObject exportAsset;
		private bool DebugExport = true;


		[MenuItem("STF Tools/Export")]
		public static void Init()
		{
			STFExportEditor window = EditorWindow.GetWindow(typeof(STFExportEditor)) as STFExportEditor;
			window.titleContent = new GUIContent("Export STF");
			window.minSize = new Vector2(600, 700);
		}
		
		void OnGUI()
		{
			GUILayout.Label("Export STF ", EditorStyles.whiteLargeLabel);
			drawHLine();
			scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Select Asset", EditorStyles.whiteLargeLabel, GUILayout.ExpandWidth(false));
			exportAsset = (GameObject)EditorGUILayout.ObjectField(
				exportAsset,
				typeof(GameObject),
				true,
				GUILayout.ExpandWidth(true)
			);
			GUILayout.EndHorizontal();
			drawHLine();

			DebugExport = GUILayout.Toggle(DebugExport, "Save Json Definition Extra");

			GUILayout.Space(10);
			drawHLine();

			string defaultExportFilaName = "new";
			if(exportAsset)
			{
				var exportSTFAsset = exportAsset.GetComponent<ISTFAsset>();
				defaultExportFilaName = exportSTFAsset != null && !string.IsNullOrWhiteSpace(exportSTFAsset.OriginalFileName) ? exportSTFAsset.OriginalFileName : exportAsset.name;
			}

			if(exportAsset && GUILayout.Button("Export", GUILayout.ExpandWidth(true))) {
				var path = EditorUtility.SaveFilePanel("STF Export", "Assets", defaultExportFilaName + ".stf", "stf");
				if(path != null && path.Length > 0) {
					SerializeAsSTFBinary(exportAsset, path, DebugExport);
				}
			}
			
			GUILayout.EndScrollView();
			drawHLine();
			GUILayout.Label("v0.3.0", EditorStyles.label);
		}

		private void drawHLine() {
			GUILayout.Space(10);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.gray);
			GUILayout.Space(10);
		}

		private void SerializeAsSTFBinary(GameObject root, string ExportPath, bool DebugExport = true)
		{
			var trash = new List<GameObject>();
			try
			{
				var exportInstance = Instantiate(root);
				exportInstance.name = root.name;
				trash.Add(exportInstance);
				var setupAsset = STFSetup.SetupStandaloneAssetInplace(exportInstance);
				trash.AddRange(setupAsset.CreatedGos);
				var exporter = new STFExporter(exportInstance.GetComponent<ISTFAsset>(), ExportPath, setupAsset.ResourceMeta, DebugExport);
			}
			catch(Exception e)
			{
				Debug.LogError(e);
			}
			finally
			{
				foreach(var trashObject in trash)
				{
					if(trashObject != null)
					{
						UnityEngine.Object.DestroyImmediate(trashObject);
					}
				}
			}
			return;
		}
	}
}
#endif

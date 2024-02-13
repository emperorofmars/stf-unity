
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
		public GameObject mainExport;
		private List<ISTFAsset> exports = new List<ISTFAsset>() {};
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
			GUILayout.Label("Select Main Asset", EditorStyles.whiteLargeLabel, GUILayout.ExpandWidth(false));
			mainExport = (GameObject)EditorGUILayout.ObjectField(
				mainExport,
				typeof(GameObject),
				true,
				GUILayout.ExpandWidth(true)
			);
			GUILayout.EndHorizontal();
			drawHLine();

			for(int i = 0; i < exports.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Select Secondary Asset", GUILayout.ExpandWidth(false));
				exports[i] = (ISTFAsset)EditorGUILayout.ObjectField(
					exports[i],
					typeof(ISTFAsset),
					true,
					GUILayout.ExpandWidth(true)
				);
				
				if(GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
				{
					exports.RemoveAt(i);
					i--;
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Add Secondary Asset", GUILayout.ExpandWidth(false)))
			{
				exports.Add(null);
			}
			GUILayout.EndHorizontal();

			DebugExport = GUILayout.Toggle(DebugExport, "Save Json Definition Extra");

			GUILayout.Space(10);
			drawHLine();
			if(mainExport && GUILayout.Button("Export", GUILayout.ExpandWidth(true))) {
				var path = EditorUtility.SaveFilePanel("STF Export", "Assets", mainExport.name + ".stf", "stf");
				if(path != null && path.Length > 0) {
					SerializeAsSTFBinary(mainExport, exports, path, DebugExport);
				}
			}
			
			GUILayout.EndScrollView();
			drawHLine();
			GUILayout.Label("v0.0.1", EditorStyles.label);
		}

		private void drawHLine() {
			GUILayout.Space(10);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.gray);
			GUILayout.Space(10);
		}

		private void SerializeAsSTFBinary(GameObject MainAsset, List<ISTFAsset> SecondaryAssets, string ExportPath, bool DebugExport = true)
		{
			var trash = new List<GameObject>();
			try
			{
				var exportInstance = Instantiate(mainExport);
				exportInstance.name = mainExport.name;
				trash.Add(exportInstance);
				var setupAsset = STFSetup.SetupStandaloneAssetInplace(exportInstance);
				trash.AddRange(setupAsset.CreatedGos);
				var exporter = new STFExporter(exportInstance.GetComponent<STFAsset>(), SecondaryAssets, ExportPath, setupAsset.ResourceMeta, DebugExport);
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

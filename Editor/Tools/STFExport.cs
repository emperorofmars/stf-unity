
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using STF.IdComponents;
using STF.Serde;
using UnityEditor;
using UnityEngine;


namespace STF.Tools
{
	// UI to export STF-Unity intermediary scenes into STF. Apart from selecting the main asset, optionally multiple secondary assets can be included into the export.

	public class STFUIExport : EditorWindow
	{
		private Vector2 scrollPos;
		public STFAsset mainExport;
		private List<STFAsset> exports = new List<STFAsset>() {};


		[MenuItem("STF Tools/Export")]
		public static void Init()
		{
			STFUIExport window = EditorWindow.GetWindow(typeof(STFUIExport)) as STFUIExport;
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
			mainExport = (STFAsset)EditorGUILayout.ObjectField(
				mainExport,
				typeof(STFAsset),
				true,
				GUILayout.ExpandWidth(true)
			);
			GUILayout.EndHorizontal();
			drawHLine();

			for(int i = 0; i < exports.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Select Secondary Asset", GUILayout.ExpandWidth(false));
				exports[i] = ((STFAsset)EditorGUILayout.ObjectField(
					exports[i],
					typeof(STFAsset),
					true,
					GUILayout.ExpandWidth(true)
				));
				
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

			GUILayout.Space(10);
			drawHLine();
			if(mainExport && GUILayout.Button("Export", GUILayout.ExpandWidth(true))) {
				var path = EditorUtility.SaveFilePanel("STF Export", "Assets", mainExport.name + "01" + ".stf", "stf");
				if(path != null && path.Length > 0) {
					SerializeAsSTFBinary(mainExport, exports, path);
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

		private void SerializeAsSTFBinary(STFAsset MainAsset, List<STFAsset> SecondaryAssets, string ExportPath)
		{
			var exporter = new STFExporter(MainAsset, SecondaryAssets, ExportPath, true);
			return;
		}
	}
}
#endif

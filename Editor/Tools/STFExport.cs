
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using stf.serialisation;
using UnityEditor;
using UnityEngine;


namespace stf
{
	public class STFUIExport : EditorWindow
	{
		private Vector2 scrollPos;
		public GameObject mainExport;
		private List<GameObject> exports = new List<GameObject>() {};


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
			mainExport = ((GameObject)EditorGUILayout.ObjectField(
				mainExport,
				typeof(GameObject),
				true,
				GUILayout.ExpandWidth(true)
			));
			GUILayout.EndHorizontal();
			drawHLine();

			for(int i = 0; i < exports.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Select Secondary Asset", GUILayout.ExpandWidth(false));
				exports[i] = ((GameObject)EditorGUILayout.ObjectField(
					exports[i],
					typeof(GameObject),
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
				var path = EditorUtility.SaveFilePanel("STF Export", "Assets", mainExport.name + ".stf", "stf");
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

		private void SerializeAsSTFBinary(GameObject mainExport, List<GameObject> secondaryExports, string path)
		{
			var mainAsset = new STFAssetExporter();
			mainAsset.rootNode = mainExport;

			var assets = new List<ISTFAssetExporter>() {mainAsset};

			foreach(var export in secondaryExports)
			{

			}

			var state = new STFExporter(assets);

			File.WriteAllBytes(path, state.GetBinary());
			File.WriteAllText(path + ".json", state.GetPrettyJson());

			return;
		}
	}
}
#endif

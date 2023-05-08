
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
		private GameObject go;

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
			GUILayout.Label("Select GameObject", EditorStyles.whiteLargeLabel, GUILayout.ExpandWidth(false));
			go = ((GameObject)EditorGUILayout.ObjectField(
				go,
				typeof(GameObject),
				true,
				GUILayout.ExpandWidth(true)
			));
			GUILayout.EndHorizontal();

			if(go && GUILayout.Button("Export as External Patch", GUILayout.ExpandWidth(true))) {
				var path = EditorUtility.SaveFilePanel("Export as External Patch", "Assets", go.name + ".json", "json");
				if(path != null && path.Length > 0) {
					File.WriteAllText(path, SerializeAsExternalAsset(go));
				}
			}

			if(go && GUILayout.Button("Export Standalone with External Resources", GUILayout.ExpandWidth(true))) {
				var path = EditorUtility.SaveFilePanel("Export Standalone with External Resources", "Assets", go.name + ".json", "json");
				if(path != null && path.Length > 0) {
					File.WriteAllText(path, SerializeAsStandaloneAssetWithExternalResources(go));
				}
			}

			if(go && GUILayout.Button("Export Standalone", GUILayout.ExpandWidth(true))) {
				var path = EditorUtility.SaveFilePanel("Export Standalone", "Assets", go.name + ".json", "json");
				if(path != null && path.Length > 0) {
					File.WriteAllText(path, SerializeAsStandaloneAsset(go));
				}
			}

			if(go && GUILayout.Button("Export STF Binary", GUILayout.ExpandWidth(true))) {
				var path = EditorUtility.SaveFilePanel("Export STF Binary", "Assets", go.name + ".stf", "stf");
				if(path != null && path.Length > 0) {
					SerializeAsSTFBinary(go, path);
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

		private string SerializeAsExternalAsset(GameObject go)
		{
			var asset = new STFExternalUnityPatchAssetExporter();
			asset.rootNode = go;
			var state = new STFExporter(new List<ISTFAssetExporter>() {asset});

			return state.GetPrettyJson();
		}

		private string SerializeAsStandaloneAssetWithExternalResources(GameObject go)
		{
			var asset = new STFExternalUnityAssetExporter();
			asset.rootNode = go;
			var state = new STFExporter(new List<ISTFAssetExporter>() {asset});

			return state.GetPrettyJson();
		}

		private string SerializeAsStandaloneAsset(GameObject go)
		{
			var asset = new STFAssetExporter();
			asset.rootNode = go;
			var state = new STFExporter(new List<ISTFAssetExporter>() {asset});

			return state.GetPrettyJson();
		}

		private void SerializeAsSTFBinary(GameObject go, string path)
		{
			var asset = new STFAssetExporter();
			asset.rootNode = go;
			var state = new STFExporter(new List<ISTFAssetExporter>() {asset});

			File.WriteAllBytes(path, state.GetBinary());
			File.WriteAllText(path + ".json", state.GetPrettyJson());

			return;
		}
	}
}
#endif

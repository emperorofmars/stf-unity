
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using STF.ApplicationConversion;
using STF.IdComponents;
using STF.Serialisation;
using UnityEditor;
using UnityEngine;

namespace STF.Tools
{
	// UI to export STF-Unity intermediary scenes into STF. Apart from selecting the main asset, optionally multiple secondary assets can be included into the export.

	public class STFApplicationConverterEditor : EditorWindow
	{
		private const string DefaultUnpackFolder = "STF Application Converts";

		public STFAsset Asset;
		private Vector2 scrollPos;
		private string path;

		[MenuItem("STF Tools/Convert To Application")]
		public static void Init()
		{
			STFApplicationConverterEditor window = EditorWindow.GetWindow(typeof(STFApplicationConverterEditor)) as STFApplicationConverterEditor;
			window.titleContent = new GUIContent("Convert To Application");
			window.minSize = new Vector2(600, 700);
		}
		
		void OnGUI()
		{
			GUILayout.Label("Convert To Application", EditorStyles.whiteLargeLabel);
			drawHLine();
			scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Select Asset", EditorStyles.whiteLargeLabel, GUILayout.ExpandWidth(false));
			Asset = (STFAsset)EditorGUILayout.ObjectField(
				Asset,
				typeof(STFAsset),
				true,
				GUILayout.ExpandWidth(true)
			);
			GUILayout.EndHorizontal();
			
			drawHLine();
			// addons

			// select converter

			if(path == null && Asset) path = AssetDatabase.GetAssetPath(Asset) != null ? Path.Combine(AssetDatabase.GetAssetPath(Asset), DefaultUnpackFolder, "unity3d") : Path.Combine("/Assets", DefaultUnpackFolder, Asset.assetInfo.assetName, "unity3d");
			
			if(GUILayout.Button("Select Output Folder", GUILayout.ExpandWidth(false)))
			{
				path = EditorUtility.SaveFolderPanel("Select Output Folder", path, "converted");
			}

			drawHLine();

			if(Asset && GUILayout.Button("Convert", GUILayout.ExpandWidth(true))) {
				//var path = EditorUtility.SaveFilePanel("STF Export", "Assets", Asset.assetInfo?.assetName + "_converted" + ".stf", "stf");
				if(path != null && path.Length > 0) {
					var c = new STFUnityConverter();
					var state = new STFApplicationConvertState {
						_RelMat = new STFRelationshipMatrix(Asset.gameObject, new List<string> {"unity3d"}, new List<Type>{typeof(STFTwistConstraint)}),
						_Root = Asset.gameObject,
						_TargetPath = path,
					};
					c.Convert(state, Asset);
				}
			}

			/*if(Asset && GUILayout.Button("Convert", GUILayout.ExpandWidth(true))) {
				var path = EditorUtility.SaveFilePanel("STF Export", "Assets", mainExport.name + "01" + ".stf", "stf");
				if(path != null && path.Length > 0) {
					SerializeAsSTFBinary(mainExport, exports, path, DebugExport);
				}
			}*/
			
			GUILayout.EndScrollView();
			drawHLine();
		}

		private void drawHLine() {
			GUILayout.Space(10);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.gray);
			GUILayout.Space(10);
		}
	}
}
#endif
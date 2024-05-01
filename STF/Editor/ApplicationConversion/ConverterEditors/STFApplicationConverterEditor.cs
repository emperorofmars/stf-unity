
#if UNITY_EDITOR

using System.IO;
using STF.Serialisation;
using STF.Tools;
using UnityEditor;
using UnityEngine;

namespace STF.ApplicationConversion.Editors
{
	public class STFApplicationConverterEditor : EditorWindow
	{
		private const string DefaultUnpackFolder = "STF Application Converts";

		public ISTFAsset Asset;
		private Vector2 scrollPos;
		private string path;

		[MenuItem("STF Tools/Convert To Application/Plain Unity")]
		public static void Init()
		{
			STFApplicationConverterEditor window = EditorWindow.GetWindow(typeof(STFApplicationConverterEditor)) as STFApplicationConverterEditor;
			window.titleContent = new GUIContent("Convert To Plain Unity Asset");
			window.minSize = new Vector2(600, 700);
		}
		
		void OnGUI()
		{
			GUILayout.Label("Convert To Plain Unity Asset", EditorStyles.whiteLargeLabel);
			drawHLine();
			scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Select Asset", EditorStyles.whiteLargeLabel, GUILayout.ExpandWidth(false));
			var tmpAsset = (ISTFAsset)EditorGUILayout.ObjectField(
				Asset,
				typeof(ISTFAsset),
				true,
				GUILayout.ExpandWidth(true)
			);
			GUILayout.EndHorizontal();
			if(tmpAsset != Asset)
			{
				Asset = tmpAsset;
				path = STFDirectoryUtil.EnsureConvertLocation(Asset, STFUnityConverter._TARGET_NAME);
			}
			
			drawHLine();

			// addons

			// settings

			if(Asset && GUILayout.Button("Convert", GUILayout.ExpandWidth(true))) {
				if(path != null && path.Length > 0) {
					var existingEntries = Directory.EnumerateFileSystemEntries(path); foreach(var entry in existingEntries)
					{
						if(File.Exists(entry)) File.Delete(entry);
						else Directory.Delete(entry, true);
					}
					AssetDatabase.Refresh();

					var c = new STFUnityConverter();
					c.Convert(new STFApplicationConvertStorageContext(path), Asset);
				}
			}
			
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
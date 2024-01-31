
#if UNITY_EDITOR

using System.IO;
using STF.Serialisation;
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
				path = null;
				Debug.Log(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(Asset));
				Debug.Log(Path.GetDirectoryName(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(Asset)));
				path = Path.GetDirectoryName(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(Asset));
				if(!Directory.Exists(Path.Combine(path, DefaultUnpackFolder)))
				{
					AssetDatabase.CreateFolder(path, DefaultUnpackFolder);
					AssetDatabase.Refresh();
				}
				path = Path.Combine(path, DefaultUnpackFolder);
				if(!Directory.Exists(Path.Combine(path, STFUnityConverter._TARGET_NAME)))
				{
					AssetDatabase.CreateFolder(path, STFUnityConverter._TARGET_NAME);
					AssetDatabase.Refresh();
				}
				path = Path.Combine(path, STFUnityConverter._TARGET_NAME);
			}
			
			drawHLine();

			// addons

			// settings

			GUILayout.BeginHorizontal();
			GUILayout.Label("Output Folder:", GUILayout.ExpandWidth(false));
			GUILayout.Label(path, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			if(GUILayout.Button("Select Output Folder", GUILayout.ExpandWidth(false)))
			{
				path = EditorUtility.SaveFolderPanel("Select Output Folder", path, "converted");
			}

			drawHLine();

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
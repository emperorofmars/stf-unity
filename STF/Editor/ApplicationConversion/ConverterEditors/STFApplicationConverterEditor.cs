
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
			Asset = (ISTFAsset)EditorGUILayout.ObjectField(
				Asset,
				typeof(ISTFAsset),
				true,
				GUILayout.ExpandWidth(true)
			);
			GUILayout.EndHorizontal();
			
			drawHLine();

			// addons

			// settings

			if(path == null && Asset != null)
			{
				path = Path.Combine("Assets", DefaultUnpackFolder, Asset.Name, STFUnityConverter._TARGET_NAME);


				if(!Directory.Exists("Assets/" + DefaultUnpackFolder))
				{
					AssetDatabase.CreateFolder("Assets", DefaultUnpackFolder);
					AssetDatabase.Refresh();
				}
				if(!Directory.Exists(Path.Combine("Assets", DefaultUnpackFolder, Asset.Name)))
				{
					AssetDatabase.CreateFolder(Path.Combine("Assets", DefaultUnpackFolder), Asset.Name);
					AssetDatabase.Refresh();
				}
				if(!Directory.Exists(Path.Combine("Assets", DefaultUnpackFolder, Asset.Name, STFUnityConverter._TARGET_NAME)))
				{
					AssetDatabase.CreateFolder(Path.Combine("Assets", DefaultUnpackFolder, Asset.Name), STFUnityConverter._TARGET_NAME);
					AssetDatabase.Refresh();
				}
			}
			
			Debug.Log(AssetDatabase.GetAssetPath(Asset));

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

#if UNITY_EDITOR
#if AVA_VRCSDK3_FOUND

using System.IO;
using STF.ApplicationConversion;
using STF.Tools;
using STF.Types;
using UnityEditor;
using UnityEngine;

namespace AVA.ApplicationConversion
{
	public class AVA_VRC_ConverterEditor : EditorWindow
	{
		private const string DefaultUnpackFolder = "STF Application Converts";

		public ISTFAsset Asset;
		private Vector2 scrollPos;
		private string path;

		[MenuItem("STF Tools/Convert To Application/VRChat SDK3 Avatar")]
		public static void Init()
		{
			AVA_VRC_ConverterEditor window = EditorWindow.GetWindow(typeof(AVA_VRC_ConverterEditor)) as AVA_VRC_ConverterEditor;
			window.titleContent = new GUIContent("Convert To VRChat SDK3 Avatar");
			window.minSize = new Vector2(600, 700);
		}
		
		void OnGUI()
		{
			#if STF_EDITOR
			Debug.Log("STF_EDITOR");
			#else
			Debug.Log("FAIL");
			#endif

			GUILayout.Label("Convert To VRChat SDK3 Avatar", EditorStyles.whiteLargeLabel);
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
				path = DirectoryUtil.EnsureConvertLocation(Asset, STFUnityConverter._TARGET_NAME);
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

					var c = new AVA_VRC_Converter();
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
#endif

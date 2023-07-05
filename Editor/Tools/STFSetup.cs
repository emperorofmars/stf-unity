
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using stf.serialisation;
using UnityEditor;
using UnityEngine;


namespace stf
{
	public class STFSetupEditor : EditorWindow
	{
		private Vector2 scrollPos;
		public GameObject root;

		[MenuItem("STF Tools/Setup")]
		public static void Init()
		{
			STFSetupEditor window = EditorWindow.GetWindow(typeof(STFSetupEditor)) as STFSetupEditor;
			window.titleContent = new GUIContent("Setup STF");
			window.minSize = new Vector2(600, 700);
		}
		
		void OnGUI()
		{
			GUILayout.Label("Setup STF ", EditorStyles.whiteLargeLabel);
			drawHLine();
			scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Select Root GameObject", EditorStyles.whiteLargeLabel, GUILayout.ExpandWidth(false));
			root = ((GameObject)EditorGUILayout.ObjectField(
				root,
				typeof(GameObject),
				true,
				GUILayout.ExpandWidth(true)
			));
			if(root && (root.transform.parent != null || root.GetComponent<STFAssetInfo>() != null || root.GetComponent<STFUUID>() != null)) root = null; // only root nodes allowed
			GUILayout.EndHorizontal();

			drawHLine();

			if(root && GUILayout.Button("Setup as STF intermediary format", GUILayout.ExpandWidth(true)))
			{
				var path = EditorUtility.SaveFolderPanel("Setup Resources Folder", "Assets", "STF_Setup_Resources_" + root.name);

				if(Directory.Exists(path))
				{
					AssetDatabase.DeleteAsset(path);
					AssetDatabase.Refresh();
				}

				if (path.StartsWith(Application.dataPath)) path = "Assets" + path.Substring(Application.dataPath.Length);
				AssetDatabase.CreateFolder(path.Substring(0, path.Length - Path.GetFileName(path).Length), Path.GetFileName(path));

				var rootInstance = Instantiate(root);
				rootInstance.name = root.name + "_STFSetup";

				Debug.Log($"PATH: {path}");

				var resources = STFSetup.SetupInplace(rootInstance, path);
				foreach(var resource in resources)
				{
					AssetDatabase.CreateAsset(resource, path + Path.DirectorySeparatorChar + resource.name + ".asset");
				}
				AssetDatabase.Refresh();
				root = null;
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
	}
}
#endif

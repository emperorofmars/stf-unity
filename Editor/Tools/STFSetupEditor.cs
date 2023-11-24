
#if UNITY_EDITOR

using STF.IdComponents;
using STF.Serialisation;
using UnityEditor;
using UnityEngine;

namespace STF.Tools
{
	// Editor tool to setup a unity scene root GameObject into the STF-Unity intermediary format.
	// Will add UUID's and determine armatures. May be expanded in the future.
	public class STFSetupEditor : EditorWindow
	{
		private Vector2 scrollPos;
		public GameObject root;
		public bool duplicate = true;

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
			root = (GameObject)EditorGUILayout.ObjectField(
				root,
				typeof(GameObject),
				true,
				GUILayout.ExpandWidth(true)
			);
			if(root && (root.transform.parent != null || root.GetComponent<STFAsset>() != null || root.GetComponent<ISTFNode>() != null)) root = null; // only root nodes allowed
			GUILayout.EndHorizontal();
			duplicate = GUILayout.Toggle(duplicate, "Setup On Duplicate");

			drawHLine();

			if(root && GUILayout.Button("Setup as STF intermediary format", GUILayout.ExpandWidth(true)))
			{
				var rootInstance = root;
				if(duplicate)
				{
					rootInstance = Instantiate(root);
					rootInstance.name = root.name + "_STFSetup";
				}

				STFSetup.SetupStandaloneAssetInplace(rootInstance);
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

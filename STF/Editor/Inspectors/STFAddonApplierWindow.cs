
#if UNITY_EDITOR

using System.IO;
using STF.Serialisation;
using UnityEditor;
using UnityEngine;

namespace STF.Editors
{
	public class STFAddonApplierWindow : EditorWindow
	{
		private const string DefaultUnpackFolder = "STF Addon Applier";

		public ISTFAsset TargetAsset;
		public STFAddonAsset Addon;
		private Vector2 scrollPos;

		[MenuItem("STF Tools/Apply Addons")]
		public static void Init()
		{
			STFAddonApplierWindow window = EditorWindow.GetWindow(typeof(STFAddonApplierWindow)) as STFAddonApplierWindow;
			window.titleContent = new GUIContent("Apply STF Addons");
			window.minSize = new Vector2(600, 700);
		}
		
		void OnGUI()
		{
			GUILayout.Label("Apply STF Addons", EditorStyles.whiteLargeLabel);
			drawHLine();
			scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Select Target Asset", EditorStyles.whiteLargeLabel, GUILayout.ExpandWidth(false));
			TargetAsset = (ISTFAsset)EditorGUILayout.ObjectField(
				TargetAsset,
				typeof(ISTFAsset),
				true,
				GUILayout.ExpandWidth(true)
			);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Select Addon", EditorStyles.whiteLargeLabel, GUILayout.ExpandWidth(false));
			Addon = (STFAddonAsset)EditorGUILayout.ObjectField(
				Addon,
				typeof(STFAddonAsset),
				true,
				GUILayout.ExpandWidth(true)
			);
			GUILayout.EndHorizontal();
			
			drawHLine();

			if(TargetAsset && Addon && GUILayout.Button("Convert", GUILayout.ExpandWidth(true))) {
				// TODO: Significantly expand the options, most importantly create a storage context interface similar to application conversions so that resources can be adapted/created and stored.
				STFAddonApplier.Apply(TargetAsset, Addon);
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
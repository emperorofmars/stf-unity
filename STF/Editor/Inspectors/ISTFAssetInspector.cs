
#if UNITY_EDITOR

using System.Collections.Generic;
using STF.Serialisation;
using UnityEditor;
using UnityEngine;

namespace STF.Types
{
	[CustomEditor(typeof(ISTFAsset), true)]
	public class ISTFAssetInspector : Editor
	{
		private bool DebugExport = false;
		private bool AllowDegradedExport = false;
		private readonly List<STFAddonAsset> SetAddons = new() {null};

		public override void OnInspectorGUI()
		{
			var asset = (ISTFAsset)target;
			Undo.RecordObject(asset, "STF asset");

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField(asset.Type, EditorStyles.whiteLargeLabel);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Id"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_STFName"));
			
			drawHLine();

			EditorGUILayout.LabelField("Export", EditorStyles.whiteLargeLabel);
			EditorGUI.indentLevel++;
			if(asset.AnyDegraded)
			{
				EditorGUILayout.LabelField("Warning: This asset is degraded!");
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Allow export of degraded asset");
				AllowDegradedExport = GUILayout.Toggle(AllowDegradedExport, "Save Json Definition Extra");
				EditorGUILayout.EndHorizontal();
			}
			if(!asset.AnyDegraded || AllowDegradedExport)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(10);
				if(GUILayout.Button("Export")) ExportEditor.ExportDialog(asset, DebugExport);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				DebugExport = GUILayout.Toggle(DebugExport, "Save Json Definition Extra");
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel--;
			
			drawHLine();

			EditorGUILayout.LabelField("Select Addons", EditorStyles.whiteLargeLabel);
			EditorGUI.indentLevel++;
			for(int i = 0; i < SetAddons.Count; i++)
			{
				var wasNull = SetAddons[i] == null;
				
				EditorGUILayout.BeginHorizontal();
				SetAddons[i] = (STFAddonAsset)EditorGUILayout.ObjectField(SetAddons[i], typeof(STFAddonAsset), true);
				if(SetAddons[i] != null && GUILayout.Button("Remove", GUILayout.ExpandWidth(false))) { SetAddons.RemoveAt(i); i--; }
				EditorGUILayout.EndHorizontal();

				if(wasNull && SetAddons[i] != null) SetAddons.Add(null);
				if(!wasNull && SetAddons[i] == null) { SetAddons.RemoveAt(i); i--; }
			}
			if(SetAddons.Find(a => a != null) && GUILayout.Button("Apply"))
			{
				Debug.Log("TODO apply addons");
			}
			EditorGUI.indentLevel--;
			
			drawHLine();

			EditorGUILayout.LabelField("Convert to target application", EditorStyles.whiteLargeLabel);
			EditorGUI.indentLevel++;
			foreach(var converter in STFRegistry.ApplicationConverters)
			{
				if(converter.CanConvert(asset))
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(converter.TargetName);
					if(GUILayout.Button("Apply"))
					{
						Debug.Log("TODO convert");
					}
					if(GUILayout.Button("Apply with Addons"))
					{
						Debug.Log("TODO convert with addons");
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUI.indentLevel--;

			drawHLine();

			EditorGUILayout.LabelField("Asset Metadata", EditorStyles.whiteLargeLabel);
			EditorGUI.indentLevel++;
			DrawPropertiesExcluding(serializedObject, "m_Script", "_Id", "_STFName", "_Degraded", "AnyDegraded", "ImportMeta", "AppliedAddonMetas", "OriginalFileName");
			EditorGUI.indentLevel--;

			if(EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(asset);
				if(PrefabUtility.IsPartOfAnyPrefab(asset))
				{
					PrefabUtility.RecordPrefabInstancePropertyModifications(asset);
				}
			}
		}
		
		private void drawHLine() {
			GUILayout.Space(10);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.gray);
			GUILayout.Space(10);
		}
	}
}

#endif

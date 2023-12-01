/*#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;
using STF.Serialisation;

namespace STF.Tools
{
	[CustomEditor(typeof(ASTFResource), true)]
	public class BaseFeatureEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, "m_Script", "SerializedResourceComponents", "_Components");
			GUILayout.Space(10);
			
			EditorGUILayout.LabelField("Components", EditorStyles.whiteLargeLabel);
			GUILayout.Space(5);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("AAAAAAAAA");
			EditorGUILayout.LabelField("BBBBBBBBBBBBB");
			EditorGUILayout.EndHorizontal();

		}
	}
}

#endif
*/
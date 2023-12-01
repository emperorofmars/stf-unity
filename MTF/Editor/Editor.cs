
#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace MTF
{
	[CustomEditor(typeof(Material))]
	public class MaterialInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			var material = (Material)target;

			EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"));

			DrawPropertiesExcluding(serializedObject, "Id", "m_Script");
		}
	}
}

#endif

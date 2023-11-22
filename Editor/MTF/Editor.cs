
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
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Name");
			EditorGUILayout.LabelField(material.name);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Id");
			EditorGUILayout.LabelField(material.Id);
			GUILayout.Button("Copy");
			EditorGUILayout.EndHorizontal();
		}
	}
}

#endif

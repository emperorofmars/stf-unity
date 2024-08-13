
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using STF.Types;

namespace AVA.Types.Editors
{
	[CustomEditor(typeof(AVAFacialTrackingSimple))]
	public class AVAFacialTrackingSimpleInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			//base.DrawDefaultInspector();
			var c = (AVAFacialTrackingSimple)target;

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Id"));

			if((c.Extends != null || c.Extends.Count == 0) && c.GetComponent<AVAAvatar>() && GUILayout.Button("Setup Extends", GUILayout.ExpandWidth(false)))
			{
				c.Extends.Clear();
				var avatar = c.GetComponent<AVAAvatar>();
				c.Extends.Add(avatar?.Id);
				c.TargetMeshInstance = avatar?.MainMeshInstance;
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Extends");
			EditorGUILayout.LabelField(c.Extends?.Count == 1 ? c.Extends[0] : "-");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Mesh Instance");
			c.TargetMeshInstance = (STFMeshInstance)EditorGUILayout.ObjectField(c.TargetMeshInstance, typeof(STFMeshInstance), true);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10f);
			if(c.TargetMeshInstance != null && c.Extends?.Count == 1)
			{
				if(GUILayout.Button("Map Visemes & Expressions", GUILayout.ExpandWidth(false))) {
					c.Map();
				}
			}

			GUILayout.Space(10f);
			EditorGUILayout.LabelField("Mapped Visemes & Expressions");
			foreach(var m in c.Mappings)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(m.VisemeName);
				m.BlendshapeName = EditorGUILayout.TextField(m.BlendshapeName);
				EditorGUILayout.EndHorizontal();
			}

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(c);
			}
		}
	}
}

#endif


#if UNITY_EDITOR

using System;
using STF.Serialisation;
using UnityEditor;
using UnityEngine;

namespace STF.Inspectors
{
	public abstract class ASTFNodeInspector : Editor
	{
		private bool _editId = false;
		public override void OnInspectorGUI()
		{
			var node = (ISTFNode)target;
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Type");
			EditorGUILayout.LabelField(node.Type);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Id");
			EditorGUILayout.LabelField(node.Id);
			if(GUILayout.Button("Copy")) GUIUtility.systemCopyBuffer = node.Id;
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Separator();
			if(GUILayout.Button(!_editId ? "Edit" : "Done", GUILayout.ExpandWidth(false))) _editId = !_editId;
			EditorGUILayout.EndHorizontal();
			if(_editId)
			{
				EditorGUILayout.BeginHorizontal();
				node.Id = EditorGUILayout.TextField(node.Id);
				if(GUILayout.Button("Paste"))
				{
					if(Guid.TryParse(GUIUtility.systemCopyBuffer, out var ret)) node.Id = ret.ToString();
				}
				if(GUILayout.Button("Generate New")) node.Id = Guid.NewGuid().ToString();
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Origin");
			EditorGUILayout.LabelField(node.Origin);
			if(GUILayout.Button("Copy")) GUIUtility.systemCopyBuffer = node.Origin;
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();

			DrawPropertiesExcluding(serializedObject, new string[] {"Type", "_Id", "_Origin", "m_Script"});
			if(EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(target);
		}
	}

	[CustomEditor(typeof(STFNode))]
	public class STFNodeInspector : ASTFNodeInspector {}

	[CustomEditor(typeof(STFArmatureInstanceNode))]
	public class STFArmatureInstanceNodeInspector : ASTFNodeInspector {}

	[CustomEditor(typeof(STFBoneNode))]
	public class STFBoneNodeInspector : ASTFNodeInspector {}

	[CustomEditor(typeof(STFBoneInstanceNode))]
	public class STFBoneInstanceNodeInspector : ASTFNodeInspector {}
}

#endif

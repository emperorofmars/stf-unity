
#if UNITY_EDITOR

using System;
using STF.Serialisation;
using UnityEditor;
using UnityEngine;

namespace STF.Inspectors
{
	[CustomEditor(typeof(ISTFAsset))]
	public class STFAssetInspector : Editor
	{
		private bool _editId = false;
		public override void OnInspectorGUI()
		{
			var asset = (STFAsset)target;
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Type");
			EditorGUILayout.LabelField(asset.Type);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Id");
			EditorGUILayout.LabelField(asset.Id);
			if(GUILayout.Button("Copy")) GUIUtility.systemCopyBuffer = asset.Id;
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Separator();
			if(GUILayout.Button(!_editId ? "Edit" : "Done", GUILayout.ExpandWidth(false))) _editId = !_editId;
			EditorGUILayout.EndHorizontal();
			if(_editId)
			{
				EditorGUILayout.BeginHorizontal();
				asset.Id = EditorGUILayout.TextField(asset.Id);
				if(GUILayout.Button("Paste"))
				{
					if(Guid.TryParse(GUIUtility.systemCopyBuffer, out var ret)) asset.Id = ret.ToString();
				}
				if(GUILayout.Button("Generate New")) asset.Id = Guid.NewGuid().ToString();
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Name");
			asset.Name = EditorGUILayout.TextField(asset.Name);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Version");
			asset.Version = EditorGUILayout.TextField(asset.Version);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Author");
			asset.Author = EditorGUILayout.TextField(asset.Author);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("URL");
			asset.URL = EditorGUILayout.TextField(asset.URL);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("License");
			asset.License = EditorGUILayout.TextField(asset.License);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("License Link");
			asset.LicenseLink = EditorGUILayout.TextField(asset.LicenseLink);
			EditorGUILayout.EndHorizontal();

			if(EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(target);
		}
	}
}

#endif

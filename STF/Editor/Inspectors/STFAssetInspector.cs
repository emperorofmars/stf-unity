
#if UNITY_EDITOR

using System;
using STF.IdComponents;
using UnityEditor;
using UnityEngine;

namespace STF.Inspectors
{
	[CustomEditor(typeof(STFAsset))]
	public class STFAssetInspector : Editor
	{
		private bool _editId = false;
		public override void OnInspectorGUI()
		{
			var asset = (STFAsset)target;
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Type");
			EditorGUILayout.LabelField(asset.assetInfo.assetType);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Id");
			EditorGUILayout.LabelField(asset.assetInfo.assetId);
			if(GUILayout.Button("Copy")) GUIUtility.systemCopyBuffer = asset.assetInfo.assetId;
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Separator();
			if(GUILayout.Button(!_editId ? "Edit" : "Done", GUILayout.ExpandWidth(false))) _editId = !_editId;
			EditorGUILayout.EndHorizontal();
			if(_editId)
			{
				EditorGUILayout.BeginHorizontal();
				asset.assetInfo.assetId = EditorGUILayout.TextField(asset.assetInfo.assetId);
				if(GUILayout.Button("Paste"))
				{
					if(Guid.TryParse(GUIUtility.systemCopyBuffer, out var ret)) asset.assetInfo.assetId = ret.ToString();
				}
				if(GUILayout.Button("Generate New")) asset.assetInfo.assetId = Guid.NewGuid().ToString();
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Name");
			asset.assetInfo.assetName = EditorGUILayout.TextField(asset.assetInfo.assetName);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Version");
			asset.assetInfo.assetVersion = EditorGUILayout.TextField(asset.assetInfo.assetVersion);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Author");
			asset.assetInfo.assetAuthor = EditorGUILayout.TextField(asset.assetInfo.assetAuthor);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("URL");
			asset.assetInfo.assetURL = EditorGUILayout.TextField(asset.assetInfo.assetURL);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("License");
			asset.assetInfo.assetLicense = EditorGUILayout.TextField(asset.assetInfo.assetLicense);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("License Link");
			asset.assetInfo.assetLicenseLink = EditorGUILayout.TextField(asset.assetInfo.assetLicenseLink);
			EditorGUILayout.EndHorizontal();

			if(EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(target);
		}
	}
}

#endif

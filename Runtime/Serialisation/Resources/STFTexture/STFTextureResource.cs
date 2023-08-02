
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace stf.serialisation
{
	public class STFTextureResource : ScriptableObject
	{
		public string originalName;
		public string format;
		public int width;
		public int height;
		public bool linear;
		[HideInInspector] public byte[] data;
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(STFTextureResource))]
	public class STFScriptedImporterInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			var texture = (STFTextureResource)target;
			base.DrawDefaultInspector();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Buffer Length");
			EditorGUILayout.LabelField(texture.data?.Length.ToString());
			EditorGUILayout.EndHorizontal();

			
			if(texture.data.Length > 0 && GUILayout.Button("Save image to file"))
			{
				var path = EditorUtility.SaveFilePanel("Save Image", "Assets", texture.originalName, texture.format);
				File.WriteAllBytes(path, texture.data);
				AssetDatabase.Refresh();
			}
		}
	}
#endif
}

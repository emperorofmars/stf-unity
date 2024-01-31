
#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.CodeDom.Compiler;

namespace MTF
{
	[CustomEditor(typeof(Material))]
	public class MaterialInspector : Editor
	{
		string[] Converters = DetectConverters();
		int Selection = 0;
		private Vector2 shaderScrollPos;


		public override void OnInspectorGUI()
		{
			var material = (Material)target;

			EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"));

			EditorGUILayout.Space(20);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Select Target Shader");
			shaderScrollPos = GUILayout.BeginScrollView(shaderScrollPos, GUILayout.MaxHeight(100));
			Selection = GUILayout.SelectionGrid(Selection, Converters, 1);
			GUILayout.EndScrollView();
			EditorGUILayout.EndHorizontal();

			if(GUILayout.Button("Convert to Selected Shader"))
			{
				var convertState = new MTFEditorMaterialConvertState(Path.GetDirectoryName(AssetDatabase.GetAssetPath(material.ConvertedMaterial)), material.name);
				var newUnityMaterial = MTF.ShaderConverterRegistry.MaterialConverters[Converters[Selection]].ConvertToUnityMaterial(convertState, material);
				EditorUtility.CopySerialized(newUnityMaterial, material.ConvertedMaterial);
				AssetDatabase.SaveAssets();
			}

			EditorGUILayout.Space(20);

			DrawPropertiesExcluding(serializedObject, "Id", "m_Script");
		}

		private static string[] DetectConverters()
		{
			var ret = new List<string>();
			foreach(var converter in MTF.ShaderConverterRegistry.MaterialConverters)
			{
				ret.Add(converter.Key);
			}
			return ret.ToArray();
		}
	}

	
	public class MTFEditorMaterialConvertState : MTF.IMaterialConvertState
	{
		string Name;
		string Location;
		public MTFEditorMaterialConvertState(string Location, string Name)
		{
			this.Location = Location;
			this.Name = Name;
		}
		
		public void SaveResource(UnityEngine.Object Resource, string FileExtension)
		{
			Debug.Log("Generated");
			Debug.Log(Resource);
			Debug.Log(Path.Combine(Location, Name + "_Converted." + FileExtension));
			AssetDatabase.CreateAsset(Resource, Path.Combine(Location, Name + "_Converted." + FileExtension));
			AssetDatabase.Refresh();
		}
		public Texture2D SaveImageResource(byte[] Bytes, string Name, string Extension)
		{
			//return State.SaveAndLoadResource<Texture2D>(Bytes, this.Name + Name, Extension);
			var path = Path.Combine(Location, Name + "." + Extension);
			File.WriteAllBytes(path, Bytes);
			AssetDatabase.Refresh();
			return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
		}
	}
}

#endif

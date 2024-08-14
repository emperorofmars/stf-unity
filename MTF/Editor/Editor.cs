
#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.CodeDom.Compiler;
using STF_Util;
using System.Linq;
using MTF.PropertyValues;

namespace MTF
{
	[CustomEditor(typeof(Material))]
	public class MaterialInspector : Editor
	{
		readonly string[] Converters = DetectConverters();
		int ShaderSelection = 0;
		private string[] PropertyOptionLabels;
		private Type[] PropertyOptions;
		private int PropertyIndex;

		private string NewPropertyName;

		public void OnEnable()
		{
			var material = (Material)target;

			PropertyOptions = ReflectionUtils.GetAllSubclasses(typeof(IPropertyValue));
			PropertyOptionLabels = PropertyOptions.Select(t => t.Name).ToArray();
		}

		public override void OnInspectorGUI()
		{
			var material = (Material)target;

			EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("MaterialName"));

			drawHLine();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Select Target Shader");
			ShaderSelection = EditorGUILayout.Popup(ShaderSelection, Converters);
			EditorGUILayout.EndHorizontal();

			if(GUILayout.Button("Convert to Selected Shader"))
			{
				var convertState = new MTFEditorMaterialConvertState(Path.GetDirectoryName(AssetDatabase.GetAssetPath(material.ConvertedMaterial)), material.ConvertedMaterial != null ? material.ConvertedMaterial.name : material.name);
				var newUnityMaterial = MTF.ShaderConverterRegistry.MaterialConverters[Converters[ShaderSelection]].ConvertToUnityMaterial(convertState, material, material.ConvertedMaterial);
				if(material.ConvertedMaterial)
				{
					EditorUtility.CopySerialized(newUnityMaterial, material.ConvertedMaterial);
				}
				else
				{
					AssetDatabase.CreateAsset(newUnityMaterial, Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(material)), newUnityMaterial.name + ".Asset"));
					material.ConvertedMaterial = newUnityMaterial;
				}
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			GUILayout.Space(10);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ConvertedMaterial"));

			drawHLine();
			DrawPropertiesExcluding(serializedObject, "Id", "MaterialName", "ConvertedMaterial", "m_Script", "Properties");
			drawHLine();

			for(int i = 0; i < material.Properties.Count; i++)
			{
				var prop = material.Properties[i];
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(prop.Type);
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("X", GUILayout.ExpandWidth(false)))
				{
					for(int j = 0; j < prop.Values.Count; j++)
					{
						if(!AssetDatabase.IsMainAsset(prop.Values[j]) && AssetDatabase.GetAssetPath(material) == AssetDatabase.GetAssetPath(prop.Values[j]))
						{
							AssetDatabase.RemoveObjectFromAsset(prop.Values[j]);
							AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(material.GetInstanceID()));
						}
					}
					material.Properties.RemoveAt(i);
					i--;
					continue;
				}
				EditorGUILayout.EndHorizontal();
				
				EditorGUI.indentLevel++;

				for(int j = 0; j < prop.Values.Count; j++)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(prop.Values[j].Type);
					prop.Values[j] = (IPropertyValue)EditorGUILayout.ObjectField(
						prop.Values[j],
						typeof(IPropertyValue),
						true,
						GUILayout.ExpandWidth(false)
					);
					
					if(j > 0 && GUILayout.Button("Up", GUILayout.ExpandWidth(false)))
					{
						var tmp = prop.Values[j];
						prop.Values[j] = prop.Values[j - 1];
						prop.Values[j - 1] = tmp;
					}
					if(j < prop.Values.Count - 1 && GUILayout.Button("Down", GUILayout.ExpandWidth(false)))
					{
						var tmp = prop.Values[j];
						prop.Values[j] = prop.Values[j + 1];
						prop.Values[j + 1] = tmp;
					}
					GUILayout.Space(20);
					GUILayout.FlexibleSpace();

					if(GUILayout.Button("X", GUILayout.ExpandWidth(false)))
					{
						if(!AssetDatabase.IsMainAsset(prop.Values[j]) && AssetDatabase.GetAssetPath(material) == AssetDatabase.GetAssetPath(prop.Values[j]))
						{
							AssetDatabase.RemoveObjectFromAsset(prop.Values[j]);
							AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(material.GetInstanceID()));
						}
						prop.Values.RemoveAt(j);
						j--;
					}
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(5);

				EditorGUILayout.BeginHorizontal();
				PropertyIndex = EditorGUILayout.Popup(PropertyIndex, PropertyOptionLabels);
				if(GUILayout.Button("Add New", GUILayout.ExpandWidth(true)))
				{
					var instance = (IPropertyValue)ScriptableObject.CreateInstance(PropertyOptions[PropertyIndex]);
					prop.Values.Add(instance);
					instance.name = PropertyOptions[PropertyIndex].Name + "_" + instance.GetInstanceID();
					
					AssetDatabase.AddObjectToAsset(instance, AssetDatabase.GetAssetPath(material.GetInstanceID()));
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(material.GetInstanceID()));
				}
				EditorGUILayout.EndHorizontal();

				EditorGUI.indentLevel--;

				GUILayout.Space(20);
			}

			GUILayout.Space(20);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("New Property Name");
			NewPropertyName = EditorGUILayout.TextField(NewPropertyName);
			EditorGUILayout.EndHorizontal();
			if(!string.IsNullOrWhiteSpace(NewPropertyName) && GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
			{
				material.Properties.Add(new Material.Property {Type = NewPropertyName});
			}
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

		private void drawHLine() {
			GUILayout.Space(10);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.gray);
			GUILayout.Space(10);
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

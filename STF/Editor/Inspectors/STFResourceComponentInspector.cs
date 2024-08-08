
#if UNITY_EDITOR

using UnityEngine;
using System.Linq;
using UnityEditor;
using STF.Util;
using System.Reflection;
using System;
using STF_Util;

namespace STF.Serialisation
{
	[CustomEditor(typeof(ISTFResource), true)]
	public class STFResourceComponentInspector : Editor
	{
		private string[] ComponentOptionLabels;
		private Type[] ComponentOptions;
		private int ComponentSelection = 0;
		
		public void OnEnable()
		{
			ComponentOptions = ReflectionUtils.GetAllSubclasses(typeof(ISTFResourceComponent));
			ComponentOptionLabels = ComponentOptions.Select(t => t.Name).ToArray();
		}

		public override void OnInspectorGUI()
		{
			var c = (ISTFResource)target;
			EditorGUI.BeginChangeCheck();
			DrawPropertiesExcluding(serializedObject, "Components");
			
			//EditorGUILayout.PropertyField(serializedObject.FindProperty("Components"));

			drawHLine();
			EditorGUILayout.LabelField("Resource Components");

			for(int i = 0; i < c.Components.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				c.Components[i] = (ISTFResourceComponent)EditorGUILayout.ObjectField(
					c.Components[i],
					typeof(ISTFResourceComponent),
					true,
					GUILayout.ExpandWidth(true)
				);
					
				if(i > 0 && GUILayout.Button("Up", GUILayout.ExpandWidth(false)))
				{
					var tmp = c.Components[i];
					c.Components[i] = c.Components[i - 1];
					c.Components[i - 1] = tmp;
				}
				if(i < c.Components.Count - 1 && GUILayout.Button("Down", GUILayout.ExpandWidth(false)))
				{
					var tmp = c.Components[i];
					c.Components[i] = c.Components[i + 1];
					c.Components[i + 1] = tmp;
				}
				GUILayout.Space(20);
				if(GUILayout.Button("x", GUILayout.ExpandWidth(false)))
				{
					if(!AssetDatabase.IsMainAsset(c.Components[i]) && AssetDatabase.GetAssetPath(c) == AssetDatabase.GetAssetPath(c.Components[i]))
					{
						AssetDatabase.RemoveObjectFromAsset(c.Components[i]);
						AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(c.GetInstanceID()));
					}
					c.Components.RemoveAt(i);
					i--;
				}
				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			ComponentSelection = EditorGUILayout.Popup(ComponentSelection, ComponentOptionLabels);
			if(GUILayout.Button("Add New Component"))
			{
				var instance = (ISTFResourceComponent)ScriptableObject.CreateInstance(ComponentOptions[ComponentSelection]);
				instance.name = ComponentOptions[ComponentSelection].Name + "_" + instance.Id;
				instance.Resource = c;
				c.Components.Add(instance);
				
				AssetDatabase.AddObjectToAsset(instance, AssetDatabase.GetAssetPath(c.GetInstanceID()));
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(c.GetInstanceID()));
			}
			EditorGUILayout.EndHorizontal();

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(c);
			}
		}

		private void drawHLine() {
			GUILayout.Space(10);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), Color.gray);
			GUILayout.Space(10);
		}
	}
}

#endif

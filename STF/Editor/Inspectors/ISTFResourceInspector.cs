
#if UNITY_EDITOR

using UnityEngine;
using System.Linq;
using UnityEditor;
using System;
using STF_Util;

namespace STF.Types
{
	[CustomEditor(typeof(ISTFResource), true)]
	public class ISTFResourceInspector : Editor
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
			var resource = (ISTFResource)target;
			EditorGUI.BeginChangeCheck();
			DrawPropertiesExcluding(serializedObject, "Components");

			drawHLine();
			EditorGUILayout.LabelField("Resource Components");

			for(int i = 0; i < resource.Components.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				resource.Components[i] = (ISTFResourceComponent)EditorGUILayout.ObjectField(
					resource.Components[i],
					typeof(ISTFResourceComponent),
					true,
					GUILayout.ExpandWidth(true)
				);
					
				if(i > 0 && GUILayout.Button("Up", GUILayout.ExpandWidth(false)))
				{
					(resource.Components[i - 1], resource.Components[i]) = (resource.Components[i], resource.Components[i - 1]);
				}
				if (i < resource.Components.Count - 1 && GUILayout.Button("Down", GUILayout.ExpandWidth(false)))
				{
					(resource.Components[i + 1], resource.Components[i]) = (resource.Components[i], resource.Components[i + 1]);
				}
				GUILayout.Space(20);
				if(GUILayout.Button("x", GUILayout.ExpandWidth(false)))
				{
					if(!AssetDatabase.IsMainAsset(resource.Components[i]) && AssetDatabase.GetAssetPath(resource) == AssetDatabase.GetAssetPath(resource.Components[i]))
					{
						AssetDatabase.RemoveObjectFromAsset(resource.Components[i]);
						AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(resource.GetInstanceID()));
					}
					resource.Components.RemoveAt(i);
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
				instance.Resource = new ResourceReference(resource);
				resource.Components.Add(instance);
				
				AssetDatabase.AddObjectToAsset(instance, AssetDatabase.GetAssetPath(resource.GetInstanceID()));
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(resource.GetInstanceID()));
			}
			EditorGUILayout.EndHorizontal();

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(resource);
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

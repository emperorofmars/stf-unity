
#if UNITY_EDITOR

using UnityEngine;
using System.Linq;
using UnityEditor;
using STF.Util;
using System.Reflection;
using System;

namespace STF.Serialisation
{
	[CustomEditor(typeof(ISTFResource), true)]
	public class STFResourceComponentInspector : Editor
	{
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
					typeof(GameObject),
					true,
					GUILayout.ExpandWidth(true)
				);
				if(GUILayout.Button("x"))
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

			drawHLine();
			EditorGUILayout.LabelField("Add New");

			foreach(var t in ReflectionUtils.GetAllSubclasses(typeof(ISTFResourceComponent)))
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(t.Name);
				if(GUILayout.Button("Add"))
				{
					var instance = (ISTFResourceComponent)ScriptableObject.CreateInstance(t);
					c.Components.Add(instance);
					instance.name = t.Name + "_" + instance.Id;
					
					AssetDatabase.AddObjectToAsset(instance, AssetDatabase.GetAssetPath(c.GetInstanceID()));
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(c.GetInstanceID()));
				}
				EditorGUILayout.EndHorizontal();
			}


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

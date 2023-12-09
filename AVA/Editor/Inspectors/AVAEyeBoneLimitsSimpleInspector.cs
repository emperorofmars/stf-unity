
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using UnityEditor;

namespace AVA.Serialisation
{
	[CustomEditor(typeof(AVAEyeBoneLimitsSimple))]
	public class AVAEyeBoneLimitsSimpleInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			//base.DrawDefaultInspector();
			var c = (AVAEyeBoneLimitsSimple)target;

			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"));

			var avatar = c.GetComponent<AVAAvatar>();
			var humanoid = avatar.TryGetHumanoidDefinition();

			if(humanoid != null && humanoid.Mappings != null)
			{
				var eyeLeft = humanoid.Mappings.Find(m => m.humanoidName == "EyeLeft");
				var eyeRight = humanoid.Mappings.Find(m => m.humanoidName == "EyeRight");
				if(eyeLeft == null && eyeRight == null)
				{
					EditorGUILayout.LabelField("Warning: Eyes not mapped!");
				}
			}
			else
			{
				EditorGUILayout.LabelField("Humanoid mappings not set up!");
			}
			if(!avatar)
			{
				EditorGUILayout.LabelField("Avatar not set up!");
			}

			if(avatar && humanoid)
			{
				if(c.Extends == null || c.Extends.Count != 1) c.Extends = new List<string>() {avatar.Id};
				else EditorGUILayout.LabelField("Relationships set up correctly");
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Limit Up");
			c.up = EditorGUILayout.FloatField(c.up);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Limit Down");
			c.down = EditorGUILayout.FloatField(c.down);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Limit Inner");
			c.inner = EditorGUILayout.FloatField(c.inner);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Limit Outer");
			c.outer = EditorGUILayout.FloatField(c.outer);
			EditorGUILayout.EndHorizontal();

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(c);
			}
		}
	}
}

#endif

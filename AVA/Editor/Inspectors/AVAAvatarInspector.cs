
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AVA.Types.Editors
{
	[CustomEditor(typeof(AVAAvatar))]
	public class AVAAvatarInspector : Editor
	{
		private bool editPosition = false;

		public override void OnInspectorGUI()
		{
			var c = (AVAAvatar)target;

			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_Id"));

			var humanoidDefinition = c.TryGetHumanoidDefinition();
			if(humanoidDefinition == null)
			{
				EditorGUILayout.LabelField("Humanoid mappings not set up!");
				GUILayout.Space(10f);
			}

			if(!c.MainMeshInstance.IsRef)
			{
				if(GUILayout.Button("Try Auto-Setup", GUILayout.ExpandWidth(false))) c.TrySetup();
				GUILayout.Space(20f);
			}
			EditorGUILayout.PropertyField(serializedObject.FindProperty("MainMeshInstance"));
			
			if(humanoidDefinition != null && humanoidDefinition.Mappings != null)
			{
				var eyeLeft = c.FindBoneInstance(humanoidDefinition, "EyeLeft");
				var eyeRight = c.FindBoneInstance(humanoidDefinition, "EyeRight");
				GUILayout.Space(10f);
				if(GUILayout.Button(eyeLeft != null && eyeRight != null ? "Set viewport between the eyes" : "Set viewport to head", GUILayout.ExpandWidth(false)))
				{
					c.SetupViewport(humanoidDefinition);
				}
				
				GUILayout.Space(10f);
				if(!editPosition && GUILayout.Button("Edit viewport", GUILayout.ExpandWidth(false))) editPosition = true;
				else if(editPosition && GUILayout.Button("Stop editing viewport", GUILayout.ExpandWidth(false))) editPosition = false;
			}

			GUILayout.Space(20f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("viewport_parent"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("viewport_position"));

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(c);
			}
		}

		public void OnSceneGUI()
		{
			var c = (AVAAvatar)target;
			
			if(c.viewport_parent.Node && editPosition)
			{
				Handles.Label(c.viewport_parent.Node.transform.position + c.viewport_position, "Viewport");
				c.viewport_position = Handles.DoPositionHandle(c.viewport_parent.Node.transform.position + c.viewport_position, Quaternion.identity) - c.viewport_parent.Node.transform.position;
			}
		}
		
		[DrawGizmo(GizmoType.Selected)]
		public static void OnDrawGizmo(AVAAvatar target, GizmoType gizmoType)
		{
			var c = (AVAAvatar)target;

			if(c && c.viewport_parent.Node)
			{
				Gizmos.DrawSphere(c.viewport_parent.Node.transform.position + c.viewport_position, 0.01f);
			}
		}
	}
}

#endif

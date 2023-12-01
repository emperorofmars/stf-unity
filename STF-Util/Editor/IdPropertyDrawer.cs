#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace STF_Util
{
	[CustomPropertyDrawer(typeof(IdAttribute))]
	public class IdPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var Id = attribute as IdAttribute;
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			if(Id.editable) Id.editingId = EditorGUI.TextField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), Id.editingId);
			else EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.stringValue);

			if(!Id.editable && GUI.Button(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, 50, EditorGUIUtility.singleLineHeight), "Copy"))
			{
				GUIUtility.systemCopyBuffer = property.stringValue;
			}
			if(!Id.editable && GUI.Button(new Rect(position.x + 60, position.y + EditorGUIUtility.singleLineHeight, 50, EditorGUIUtility.singleLineHeight), "Edit"))
			{
				Id.editingId = property.stringValue;
				Id.editable = true;
			}

			if(Id.editable && GUI.Button(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, 50, EditorGUIUtility.singleLineHeight), "Save"))
			{
				property.stringValue = Id.editingId;
				Id.editingId = "";
				Id.editable = false;
			}
			if(Id.editable && GUI.Button(new Rect(position.x + 60, position.y + EditorGUIUtility.singleLineHeight, 50, EditorGUIUtility.singleLineHeight), "Revert"))
			{
				Id.editingId = "";
				Id.editable = false;
			}
			if(Id.editable && GUI.Button(new Rect(position.x + 120, position.y + EditorGUIUtility.singleLineHeight, 50, EditorGUIUtility.singleLineHeight), "Paste"))
			{
				Id.editingId = GUIUtility.systemCopyBuffer;
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 2 + 5;
		}
	}
}

#endif

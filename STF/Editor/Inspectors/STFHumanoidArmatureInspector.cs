
#if UNITY_EDITOR

using UnityEngine;
using System.Linq;
using UnityEditor;
using STF.Util;

namespace STF.Serialisation
{
	[CustomEditor(typeof(STFHumanoidArmature))]
	public class STFHumanoidArmatureInspector : Editor
	{
		private int locomotionSelection = 0;
		private string[] locomotionOptions = new string[] { "plantigrade", "digitigrade" };
		private string[] locomotionDisplayOptions = new string[] { "Plantigrade", "Digitigrade" };
		private bool _foldoutMappings = true;

		void OnEnable()
		{
			var c = (STFHumanoidArmature)target;
			if(c.LocomotionType != null)
			{
				for(int i = 0; i < locomotionOptions.Length; i++)
				{
					if(c.LocomotionType == locomotionOptions[i])
					{
						locomotionSelection = i;
						break;
					}
				}
			}
		}

		public override void OnInspectorGUI()
		{
			//base.DrawDefaultInspector();
			var c = (STFHumanoidArmature)target;

			EditorGUI.BeginChangeCheck();

			DrawPropertiesExcluding(serializedObject, "m_Script", "LocomotionType", "Mappings");

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Id");
			c.Id = EditorGUILayout.TextField(c.Id);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Locomotion Type");

			var locomotionSelectionNew = EditorGUILayout.Popup(locomotionSelection, locomotionDisplayOptions);
			if(locomotionSelectionNew != locomotionSelection || c.LocomotionType == null || c.LocomotionType.Length == 0)
			{
				c.LocomotionType = locomotionOptions[locomotionSelectionNew];
				locomotionSelection = locomotionSelectionNew;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10f);
			
			EditorGUILayout.PrefixLabel("Mapped " + (c.Mappings != null ? c.Mappings.Count : 0) + " bones.");
			if(c.Resource == null)
			{
				var mainAsset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target));
				if(mainAsset != null && mainAsset is ISTFResource)
				{
					c.Resource = (ISTFResource)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target));
				}
				else
				{
					c.Resource = (ISTFResource)EditorGUILayout.ObjectField("Parent Resource", c.Resource, typeof(STFArmature), false);
				}
			}
			if(c.Resource != null && c.Resource.GetType() == typeof(STFArmature))
			{
				var armature = ((c.Resource as STFArmature).Resource as GameObject).GetComponent<STFArmatureNodeInfo>();
				if(GUILayout.Button("Map Humanoid Bones", GUILayout.ExpandWidth(false))) {
					c.Map(armature.Bones.Select(b => b.transform).ToArray());
				}
			}
			else
			{
				EditorGUILayout.LabelField("This Resource Component can be placed only on an STFArmature!");
			}

			if(c.Resource != null && c.Resource.GetType() == typeof(STFArmature) && c.Mappings != null && c.Mappings.Count > 0)
			{
				var armature = ((c.Resource as STFArmature).Resource as GameObject).GetComponent<STFArmatureNodeInfo>();
				GUILayout.Space(10f);
				if(GUILayout.Button("Create Unity Avatar", GUILayout.ExpandWidth(false))) {
					var path = EditorUtility.SaveFilePanel("Save Avatar", "Assets", "avatar", "asset");
					var avatar = STFHumanoidArmature.GenerateAvatar(c, armature);
					if (path.StartsWith(Application.dataPath)) {
						path = "Assets" + path.Substring(Application.dataPath.Length);
					}
					AssetDatabase.CreateAsset(avatar, path);
					c.GeneratedAvatar = avatar;
				}
			}

			GUILayout.Space(10f);

			_foldoutMappings = EditorGUILayout.Foldout(_foldoutMappings, "Mappings", true, EditorStyles.foldoutHeader);
			if(_foldoutMappings)
			{
				GUILayout.Space(5f);
				//base.DrawDefaultInspector();

				foreach(var mapping in STFHumanoidArmature.NameMappings)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(mapping.Key);
					var bone = c.Mappings.Find(m => m.humanoidName == mapping.Key);
					if(bone != null)
					{
						bone.bone = (GameObject)EditorGUILayout.ObjectField(bone.bone, typeof(GameObject), true);
					}
					else
					{
						if(GUILayout.Button("Add Bone Mapping", GUILayout.ExpandWidth(false))) {
							c.Mappings.Add(new STFHumanoidArmature.BoneMappingPair(mapping.Key, null));
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(c);
			}
			
		}
	}
}

#endif

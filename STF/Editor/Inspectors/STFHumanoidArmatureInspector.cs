
#if UNITY_EDITOR

using UnityEngine;
using System.Linq;
using UnityEditor;

namespace STF.Types
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
					c.Resource = new ResourceReference((ISTFResource)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target)));
				}
				else
				{
					c.Resource = new ResourceReference((ISTFResource)EditorGUILayout.ObjectField("Parent Resource", c.Resource.Resource, typeof(STFArmature), false));
				}
			}
			var stfArmatureValid = c.Resource.IsValid() && c.Resource.Ref.GetType() == typeof(STFArmature) && (c.Resource.Ref as STFArmature).Resource != null;
			var stfArmature = c.Resource.Ref as STFArmature;

			if(stfArmatureValid)
			{
				var armature = (stfArmature.Resource as GameObject).GetComponent<STFArmatureNodeInfo>();
				if(GUILayout.Button("Map Humanoid Bones", GUILayout.ExpandWidth(false))) {
					c.Map(armature.Bones.Select(b => b.transform).ToArray());
				}

				if(c.Mappings != null && c.Mappings.Count > 0)
				{
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
			}
			else
			{
				GUILayout.Space(20f);
				EditorGUILayout.LabelField("STF Armature is missing its Unity resource!");
				EditorGUILayout.LabelField("This can happen if its imported only into the AssetCtx of the ScriptedImporter.");
				EditorGUILayout.LabelField("This can also happen if this Resource Component is not placed on an STFArmature!");
			}

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(c);
			}
			
		}
	}
}

#endif

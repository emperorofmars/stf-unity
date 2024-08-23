
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using VRC;

namespace nna.jank
{
	// Taken from: https://discussions.unity.com/t/trying-to-add-new-data-to-fbx-imports-is-absolutely-miserable/906116/7
	[InitializeOnLoad]
	public static class UnityModelImporterInspectorExtension
	{
		static UnityModelImporterInspectorExtension() => Editor.finishedDefaultHeaderGUI += ShowQualityOfLifeButtons;

		private static void ShowQualityOfLifeButtons(Editor editor)
		{
			if (!editor.target || editor.target is not ModelImporter) return;

			var importer = (ModelImporter)editor.target;

			var contextOptions = NNARegistry.GetAvaliableContexts();
			int selectedIndex = contextOptions.FindIndex(c => c == importer.userData);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Select Import Context");
			selectedIndex = EditorGUILayout.Popup(selectedIndex, contextOptions.ToArray());
			EditorGUILayout.EndHorizontal();
			
			var newSelectedImportContext = NNARegistry.DefaultContext;
			if(selectedIndex >= 0 && selectedIndex < contextOptions.Count) newSelectedImportContext = contextOptions[selectedIndex];
			else newSelectedImportContext = NNARegistry.DefaultContext;

			if(importer.userData != newSelectedImportContext)
			{
				importer.userData = newSelectedImportContext;
				importer.MarkDirty();
			}
		}
	}
}

#endif


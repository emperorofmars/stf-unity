using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using STF.Serialisation;


#if UNITY_EDITOR
using UnityEditor;
#endif

// This exists for now purely to showcase how data could be stored to populate the ui of a character editor application
namespace AVA.Serialisation
{
	[Serializable]
	public class CharacterEditorEntry
	{
		public string type;
		public string display_name;
		public string tooltip;
		public Texture2D icon;
		public List<AnimationClip> options;
	}
	/*public class CharacterEditorSliderEntry : ACharacterEditorEntry
	{
		public BlendTree blendtree;
	}*/
	[Serializable]
	public class CharacterEditorCategory
	{
		public string display_name;
		public List<CharacterEditorEntry> entries = new List<CharacterEditorEntry>();
	}

	public class AVACharacterEditorSetup : ISTFNodeComponent
	{
		public static string _TYPE = "AVA.character-editor-setup";
		public override string Type => _TYPE;
		
		public string model_description;
		public string help_text;

		public List<CharacterEditorCategory> categories = new List<CharacterEditorCategory>();
	}

/*
#if UNITY_EDITOR
	[InitializeOnLoad]
	public class Register_AVACharacterEditorSetup
	{
		static Register_AVACharacterEditorSetup()
		{
			STFRegistry.RegisterNodeComponentImporter(AVACharacterEditorSetup._TYPE, new AVACharacterEditorSetupImporter());
			STFRegistry.RegisterNodeComponentExporter(typeof(AVACharacterEditorSetup), new AVACharacterEditorSetupExporter());
		}
	}
#endif
*/
}
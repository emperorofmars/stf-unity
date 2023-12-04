#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AVA.ApplicationConversion
{
	[InitializeOnLoad, ExecuteInEditMode]
	public class AVA_VRC_Detector
	{
		const string AVA_VRCSDK3_FOUND = "AVA_VRCSDK3_FOUND";

		static AVA_VRC_Detector()
		{
			//if(AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("VRC.SDK3.Avatars")))
			if(Directory.GetFiles(Path.GetDirectoryName(Application.dataPath), "VRCAvatarDescriptorEditor3.cs", SearchOption.AllDirectories).Length > 0)
			{
				Debug.Log("AVA: Found VRC SDK 3");
				ScriptDefinesManager.AddDefinesIfMissing(BuildTargetGroup.Standalone, AVA_VRCSDK3_FOUND);
			}
			else
			{
				Debug.Log("AVA: Didn't find VRC SDK 3");
				ScriptDefinesManager.RemoveDefines(AVA_VRCSDK3_FOUND);
			}
		}
	}
}

#endif

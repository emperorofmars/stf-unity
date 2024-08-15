#if UNITY_EDITOR

using System.IO;
using STF.Util;
using UnityEditor;
using UnityEngine;

namespace AVA.ApplicationConversion
{
	[InitializeOnLoad, ExecuteInEditMode]
	public class AVA_VRM_Detector
	{
		const string AVA_UNIVRM_FOUND = "AVA_UNIVRM_FOUND";

		static AVA_VRM_Detector()
		{
			if(Directory.GetFiles(Path.GetDirectoryName(Application.dataPath), "IVRMComponent.cs", SearchOption.AllDirectories).Length > 0)
			{
				Debug.Log("AVA: Found UniVRM");
				ScriptDefinesManager.AddDefinesIfMissing(BuildTargetGroup.Standalone, AVA_UNIVRM_FOUND);
			}
			else
			{
				Debug.Log("AVA: Didn't find UniVRM");
				ScriptDefinesManager.RemoveDefines(AVA_UNIVRM_FOUND);
			}
		}
	}
}

#endif

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace STF.Util
{
	[InitializeOnLoad, ExecuteInEditMode]
	public class RegisterAVADefine
	{
		const string DefineString = "AVA";

		static RegisterAVADefine()
		{
			ScriptDefinesManager.AddDefinesIfMissing(BuildTargetGroup.Standalone, DefineString);
		}
	}
}

#endif

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace STF.Util
{
	[InitializeOnLoad, ExecuteInEditMode]
	public class RegisterSTFDefine
	{
		const string DefineString = "STF";

		static RegisterSTFDefine()
		{
			ScriptDefinesManager.AddDefinesIfMissing(BuildTargetGroup.Standalone, DefineString);
		}
	}
}

#endif

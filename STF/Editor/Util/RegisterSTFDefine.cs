#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace STF.Util
{
	[InitializeOnLoad, ExecuteInEditMode]
	public class STFDefine
	{
		const string DefineString = "STF";

		static STFDefine()
		{
			ScriptDefinesManager.AddDefinesIfMissing(BuildTargetGroup.Standalone, DefineString);
		}
	}
}

#endif

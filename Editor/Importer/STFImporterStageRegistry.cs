
#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using stf.serialisation;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace stf
{
	public static class STFImporterStageRegistry
	{
		private static List<ISTFSecondStage> RegisteredSecondStages = new List<ISTFSecondStage> {
			new STFDefaultSecondStage()
		};

		public static void Register(ISTFSecondStage stage)
		{
			RegisteredSecondStages.Add(stage);
		}

		public static List<ISTFSecondStage> Get()
		{
			return RegisteredSecondStages;
		}
	}
}

#endif

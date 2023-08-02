
using System.Collections.Generic;
using stf.serialisation;

namespace stf
{
	// Second stages to run after the intermediate (authoring) scene has been created.
	public static class STFImporterStageRegistry
	{
		private static List<ISTFSecondStage> RegisteredSecondStages = new List<ISTFSecondStage> {
			new STFDefaultSecondStage()
		};

		public static void RegisterStage(ISTFSecondStage stage)
		{
			RegisteredSecondStages.Add(stage);
		}

		public static List<ISTFSecondStage> GetStages()
		{
			return RegisteredSecondStages;
		}
	}
}

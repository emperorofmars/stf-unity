
using System.Collections.Generic;
using stf.serialisation;

namespace stf
{
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

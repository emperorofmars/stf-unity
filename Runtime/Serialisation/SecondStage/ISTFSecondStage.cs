
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{
	public class SecondStageResult
	{
		public List<ISTFAsset> assets;
		public List<UnityEngine.Object> resources;
	}

	public interface ISTFSecondStage
	{
		bool CanHandle(ISTFAsset asset);
		SecondStageResult Convert(ISTFAsset asset);
	}
}
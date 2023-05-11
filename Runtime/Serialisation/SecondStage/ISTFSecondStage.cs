
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{
	public interface ISTFSecondStage
	{
		void convert(ISTFAsset asset);
		List<ISTFAsset> GetAssets();
		List<UnityEngine.Object> GetResources();
	}
}
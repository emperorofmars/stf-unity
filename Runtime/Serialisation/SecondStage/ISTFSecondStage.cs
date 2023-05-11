
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{
	public interface ISTFSecondStage
	{
		void init(ISTFImporter state);
		void AddTask(Task task);
		string GetMainAssetId();
		Dictionary<string, ISTFAsset> GetAssets();
		List<UnityEngine.Object> GetResources();
	}
}
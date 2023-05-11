
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{
	public interface ISTFSecondStage
	{
		void init(ISTFImporter state, STFMeta meta);
		void init(string mainAssetId, Dictionary<string, ISTFAsset> assets, List<UnityEngine.Object> resources, STFMeta meta);
		void AddTask(Task task);
		string GetMainAssetId();
		Dictionary<string, List<ISTFAsset>> GetAssets();
		List<UnityEngine.Object> GetResources();
	}
}
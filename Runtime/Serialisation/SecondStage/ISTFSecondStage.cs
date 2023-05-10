
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stf.serialisation
{
	public interface ISTFSecondStage
	{
		void AddTask(Task task);
		Dictionary<string, ISTFAsset> GetAssets();
		List<UnityEngine.Object> GetResources();
	}
}
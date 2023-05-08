
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFImporter
	{
		STFImportContext GetContext();
		void AddTask(Task task);
		void AddNode(string id, GameObject go);
		Dictionary<string, ISTFAsset> GetAssets();
		GameObject GetNode(string id);
		List<UnityEngine.Object> GetResources();
		UnityEngine.Object GetResource(string id);
		byte[] GetBuffer(string id);
		STFMeta GetMeta();
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFImporter
	{
		STFImportContext GetContext();
		void AddTask(Task task);
		void AddPostprocessTask(Task task);
		void AddNode(string id, GameObject go);
		void AddResources(string id, UnityEngine.Object resource);
		void AddComponent(string id, Component component);
		void AddTrashObject(UnityEngine.Object trash);
		string GetMainAssetId();
		Dictionary<string, ISTFAsset> GetAssets();
		GameObject GetNode(string id);
		List<UnityEngine.Object> GetResources();
		UnityEngine.Object GetResource(string id);
		Component GetComponent(string id);
		byte[] GetBuffer(string id);
		STFMeta GetMeta();
	}
}
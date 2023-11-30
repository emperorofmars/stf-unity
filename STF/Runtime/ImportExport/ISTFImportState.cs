
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace STF.Serialisation
{
	public interface ISTFImportState
	{
		STFImportContext Context {get;}
		string TargetLocation {get;}
		string MainAssetId {get;}
		JObject JsonRoot {get;}

		// id -> asset
		//Dictionary<string, STFAsset> Assets {get;}

		// id -> resource
		Dictionary<string, UnityEngine.Object> Resources  {get;}

		// id -> buffer
		Dictionary<string, byte[]> Buffers {get;}

		void AddTask(Task task);
		void AddResource(UnityEngine.Object Resource, string Id);
		void AddTrash(UnityEngine.Object Trash);

		void SaveResource(UnityEngine.Object Resource, string FileExtension, string Id);
		void SaveResource<T>(UnityEngine.Object Resource, string FileExtension, T Meta, string Id) where T: UnityEngine.Object, ISTFResource;
		void SaveResource<T>(GameObject Resource, T Meta, string Id) where T: UnityEngine.Object, ISTFResource;
		void SaveResource<M, R>(byte[] Resource, string FileExtension, M Meta, string Id) where M: UnityEngine.Object, ISTFResource where R: UnityEngine.Object;
		T SaveAndLoadResource<T>(byte[] Resource, string Name, string FileExtension) where T: UnityEngine.Object;
		void SaveResourceBelongingToId(UnityEngine.Object Resource, string FileExtension, string OwnerId);
		void SaveGeneratedResource(UnityEngine.Object Resource, string FileExtension);

		UnityEngine.Object LoadResource(ISTFResource Resource);
		UnityEngine.Object Instantiate(UnityEngine.Object Resource);

		void SaveAsset(GameObject Root, string Name, bool Main = false);
	}
}


using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serialisation
{
	public interface ISTFImportState
	{
		STFImportContext Context {get;}
		string TargetLocation {get;}
		string MainAssetId {get;}
		JObject JsonRoot {get;}

		// id -> asset
		Dictionary<string, STFAsset> Assets {get;}

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
		void SaveResource<T>(byte[] Resource, string FileExtension, T Meta, string Id) where T: UnityEngine.Object, ISTFResource;
		void SaveResourceBelongingToId(UnityEngine.Object Resource, string FileExtension, string OwnerId);

		UnityEngine.Object Instantiate(UnityEngine.Object Resource);

		void SaveAsset(GameObject Root, string Name, bool Main = false);
	}
}

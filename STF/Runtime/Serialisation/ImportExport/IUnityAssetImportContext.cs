
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace STF.Serialisation
{
	public interface IUnityAssetImportContext
	{
		STFImportState State {get; set;}
		void SaveSubResource(Object Component, Object Resource);
		void SaveResource(Object Resource, string FileExtension, string Id);
		void SaveResource<T>(Object Resource, string FileExtension, T Meta, string Id) where T: ISTFResource;
		void SaveResource<T>(GameObject Resource, T Meta, string Id) where T: ISTFResource;
		void SaveResource<M, R>(byte[] Resource, string FileExtension, M Meta, string Id) where M: ISTFResource where R: Object;
		T SaveAndLoadResource<T>(byte[] Resource, string Name, string FileExtension) where T: Object;
		void SaveResourceBelongingToId(Object Resource, string FileExtension, string OwnerId);
		void SaveGeneratedResource(Object Resource, string FileExtension);

		Object Instantiate(Object Resource);
	}
}

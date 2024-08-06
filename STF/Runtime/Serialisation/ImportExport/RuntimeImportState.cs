
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace STF.Serialisation
{
	public class RuntimeUnityAssetImportContext : IUnityAssetImportContext
	{
		public STFImportState State { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

		public Object Instantiate(Object Resource)
		{
			throw new System.NotImplementedException();
		}

		public T SaveAndLoadResource<T>(byte[] Resource, string Name, string FileExtension) where T : Object
		{
			throw new System.NotImplementedException();
		}

		public void SaveGeneratedResource(Object Resource, string FileExtension)
		{
			throw new System.NotImplementedException();
		}

		public void SaveResource(Object Resource, string FileExtension, string Id)
		{
			throw new System.NotImplementedException();
		}

		public void SaveResource<T>(Object Resource, string FileExtension, T Meta, string Id) where T : ISTFResource
		{
			throw new System.NotImplementedException();
		}

		public void SaveResource<T>(GameObject Resource, T Meta, string Id) where T : ISTFResource
		{
			throw new System.NotImplementedException();
		}

		public void SaveResource<M, R>(byte[] Resource, string FileExtension, M Meta, string Id)
			where M : ISTFResource
			where R : Object
		{
			throw new System.NotImplementedException();
		}

		public void SaveResourceBelongingToId(Object Resource, string FileExtension, string OwnerId)
		{
			throw new System.NotImplementedException();
		}

		public void SaveSubResource(Object Component, Object Resource)
		{
			throw new System.NotImplementedException();
		}
	}
}

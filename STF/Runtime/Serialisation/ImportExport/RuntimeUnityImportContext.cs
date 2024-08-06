
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace STF.Serialisation
{
	public class RuntimeUnityImportContext : IUnityImportContext
	{
		STFImportState _State;
		public STFImportState State { get => _State; set => _State = value; }

		public List<Object> AssetCtxObjects = new List<Object>();

		public Object Instantiate(Object Resource)
		{
			return Object.Instantiate(Resource);
		}

		public Object SaveAndLoadResource(byte[] Resource, string Name, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			if(FileExtension == ".png")
			{
				var tex = new Texture2D(2, 2);
				tex.LoadImage(Resource);
				tex.name = Name;
				return tex;
			}
			return null;
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

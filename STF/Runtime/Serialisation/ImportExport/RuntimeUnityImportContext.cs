
using System.Collections.Generic;
using UnityEngine;

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

		public void SaveResource(ISTFResource Resource, string Id)
		{
			State.AddResource(Resource, Id);
			AssetCtxObjects.Add(Resource);
		}

		public Object SaveAndLoadResource(byte[] Resource, string Name, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			if(FileExtension == ".png")
			{
				var tex = new Texture2D(2, 2);
				tex.LoadImage(Resource);
				tex.name = Name;
				AssetCtxObjects.Add(tex);
				return tex;
			}
			return null;
		}

		public void SaveGeneratedResource(Object Resource, string FileExtension)
		{
			AssetCtxObjects.Add(Resource);
		}

		/*public void SaveResource(Object Resource, string FileExtension, string Id)
		{
			State.AddResource(Resource, Id);
		}*/

		public void SaveResource<T>(Object Resource, string FileExtension, T Meta, string Id) where T : ISTFResource
		{
			Meta.Resource = Resource;
			AssetCtxObjects.Add(Resource);
			State.AddResource(Meta, Id);
		}

		public void SaveResource<T>(GameObject Resource, T Meta, string Id) where T : ISTFResource
		{
			Meta.Resource = Resource;
			AssetCtxObjects.Add(Resource);
			State.AddResource(Meta, Id);
		}

		public void SaveResource<M, R>(byte[] Resource, string FileExtension, M Meta, string Id) where M: ISTFResource where R: Object
		{
			var saved = SaveAndLoadResource(Resource, Meta.Name, FileExtension);
			Meta.Resource = saved;
			State.AddResource(Meta, Id);
		}

		public void SaveResourceBelongingToId(Object Resource, string FileExtension, string OwnerId)
		{
			AssetCtxObjects.Add(Resource);
		}

		public void SaveSubResource(Object Component, Object Resource)
		{
			AssetCtxObjects.Add(Component);
		}
	}
}

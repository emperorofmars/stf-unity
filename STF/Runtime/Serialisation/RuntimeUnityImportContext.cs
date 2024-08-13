
using System.Collections.Generic;
using UnityEngine;

namespace STF.Serialisation
{
	public class RuntimeUnityImportContext : IUnityImportContext
	{
		public List<Object> AssetCtxObjects = new List<Object>();

		public Object SaveResource(ISTFResource Resource)
		{
			AssetCtxObjects.Add(Resource);
			return Resource;
		}
		public Object SaveSubResource(Object Component, Object Resource)
		{
			AssetCtxObjects.Add(Component);
			return Component;
		}
		public Object SaveGeneratedResource(GameObject Resource)
		{
			var instance = Instantiate(Resource);
			AssetCtxObjects.Add(instance);
			return instance;
		}
		public Object SaveGeneratedResource(Object Resource, string FileExtension)
		{
			AssetCtxObjects.Add(Resource);
			return Resource;
		}
		public Object SaveGeneratedResource(byte[] Resource, string Name, string FileExtension)
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
		public Object Instantiate(Object Resource)
		{
			return Object.Instantiate(Resource);
		}
	}
}

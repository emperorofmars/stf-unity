
using System.Collections.Generic;
using STF.Types;
using UnityEngine;

namespace STF.Serialisation
{
	public class RuntimeUnityImportContext : IUnityImportContext
	{
		public bool IsDegraded => true;
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
		public (Object, ISTFBuffer) SaveGeneratedResource(byte[] Resource, string Name, string FileExtension, string BufferId = null)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			if(FileExtension == ".png")
			{
				var tex = new Texture2D(2, 2);
				tex.LoadImage(Resource);
				tex.name = Name;
				var buf = ScriptableObject.CreateInstance<STFBuffer>();
				buf.Id = BufferId;
				buf.Data = Resource;
				buf.name = BufferId;
				AssetCtxObjects.Add(tex);
				AssetCtxObjects.Add(buf);
				return (tex, buf);
			}
			return (null, null);
		}
		public Object Instantiate(Object Resource)
		{
			return Object.Instantiate(Resource);
		}
	}
}

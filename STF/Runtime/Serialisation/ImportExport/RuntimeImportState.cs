
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace STF.Serialisation
{
	public class RuntimeImportState : ISTFImportState
	{

		public override T SaveAndLoadResource<T>(byte[] Resource, string Name, string FileExtension)
		{
			throw new System.NotImplementedException();
		}

		public override void SaveGeneratedResource(Object Resource, string FileExtension)
		{
			throw new System.NotImplementedException();
		}

		public override void SaveResource(Object Resource, string FileExtension, string Id)
		{
			throw new System.NotImplementedException();
		}

		public override void SaveResource<T>(Object Resource, string FileExtension, T Meta, string Id)
		{
			throw new System.NotImplementedException();
		}

		public override void SaveResource<T>(GameObject Resource, T Meta, string Id)
		{
			throw new System.NotImplementedException();
		}

		public override void SaveResource<M, R>(byte[] Resource, string FileExtension, M Meta, string Id)
		{
			throw new System.NotImplementedException();
		}

		public override void SaveResourceBelongingToId(Object Resource, string FileExtension, string OwnerId)
		{
			throw new System.NotImplementedException();
		}

		public override void SaveSubResource(Object Component, Object Resource)
		{
			throw new System.NotImplementedException();
		}

		public override Object Instantiate(Object Resource)
		{
			return Object.Instantiate(Resource);
		}
	}
}

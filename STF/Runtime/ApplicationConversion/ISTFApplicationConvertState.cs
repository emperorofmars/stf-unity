using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public struct STFApplicationConverterContext
	{
		public Dictionary<Type, ISTFNodeComponentApplicationConverter> NodeComponent;
		public Dictionary<Type, ISTFResourceApplicationConverter> Resource;
	}

	public interface ISTFApplicationConvertState
	{
		STFRelationshipMatrix RelMat {get;}
		Dictionary<UnityEngine.Object, UnityEngine.Object> ConvertedResources {get;}
		GameObject Root {get;}
		string TargetApplication {get;}
		STFApplicationConverterContext ConverterContext {get;}


		List<UnityEngine.Object> RegisteredResources {get;}
		Dictionary<UnityEngine.Object, UnityEngine.Object> RegisteredResourcesContext {get;}
		void RegisterResource(UnityEngine.Object Resource, UnityEngine.Object Context = null);
		UnityEngine.Object DuplicateResource(UnityEngine.Object Resource);
		void SaveGeneratedResource(UnityEngine.Object Resource, string fileExtension);
		void SaveConvertedResource(UnityEngine.Object OriginalResource, UnityEngine.Object ConvertedResource, string fileExtension);

		void SaveEverything();

		void AddTask(Task Task);
		void AddTrash(UnityEngine.Object Trash);
	}
	
	public interface ISTFApplicationConvertStorageContext
	{
		string TargetPath {get;}

		UnityEngine.Object DuplicateResource(UnityEngine.Object Resource);
		void SaveGeneratedResource(UnityEngine.Object Resource, string fileExtension);
		void SavePrefab(GameObject Go, string Name = null);
		void SaveEverything();
	}
}
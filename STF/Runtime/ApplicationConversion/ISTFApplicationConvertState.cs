using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFApplicationConvertState
	{
		STFRelationshipMatrix RelMat {get;}
		Dictionary<UnityEngine.Object, UnityEngine.Object> ConvertedResources {get;}
		GameObject Root {get;}
		string TargetApplication {get;}

		List<UnityEngine.Object> RegisteredResources {get;}
		Dictionary<UnityEngine.Object, UnityEngine.Object> RegisteredResourcesContext {get;}
		void RegisterResource(UnityEngine.Object Resource, UnityEngine.Object Context = null);
		void SaveGeneratedResource(UnityEngine.Object Resource, string fileExtension);
		void SaveConvertedResource(UnityEngine.Object OriginalResource, UnityEngine.Object ConvertedResource, string fileExtension);

		void AddTask(Task Task);
		void AddTrash(UnityEngine.Object Trash);
	}
	
	public interface ISTFApplicationConvertStorageContext
	{
		string TargetPath {get;}

		UnityEngine.Object DuplicateResource(UnityEngine.Object Resource);
		void SaveGeneratedResource(UnityEngine.Object Resource, string fileExtension);
	}
}
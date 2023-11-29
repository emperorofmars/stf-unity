using System.Collections.Generic;
using System.Threading.Tasks;
using STF.IdComponents;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFApplicationConvertState
	{
		STFRelationshipMatrix RelMat {get;}
		Dictionary<UnityEngine.Object, UnityEngine.Object> ConvertedResources {get;}
		GameObject Root {get;}
		string TargetApplication {get;}

		void SaveGeneratedResource(UnityEngine.Object Resource, string fileExtension);
		void SaveConvertedResource(UnityEngine.Object OriginalResource, UnityEngine.Object ConvertedResource, string fileExtension);

		void AddTask(Task Task);
		void AddTrash(UnityEngine.Object Trash);
	}
	public interface ISTFApplicationConvertStateInternal : ISTFApplicationConvertState
	{
		string TargetPath {get;}
		void RunTasks();
		void DeleteTrash();
	}
}
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
		void SaveGeneratedResource(UnityEngine.Object Resource, string fileExtension);
		void SaveConvertedResource(UnityEngine.Object OriginalResource, UnityEngine.Object ConvertedResource, string fileExtension);

		void AddTask(Task Task);
		void AddTrash(UnityEngine.Object Trash);
	}

	public interface ISTFApplicationConverter
	{
		string TargetName {get;}
		bool CanConvert(STFAsset Asset);
		GameObject Convert(ISTFApplicationConvertState State, STFAsset Asset);
	}
}
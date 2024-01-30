using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using STF.Util;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public class STFApplicationConvertState : ISTFApplicationConvertState
	{
		private ISTFApplicationConvertStorageContext StorageContext;

		public STFRelationshipMatrix _RelMat;
		public STFRelationshipMatrix RelMat => _RelMat;

		
		public STFApplicationConverterContext _ConverterContext;
		public STFApplicationConverterContext ConverterContext => _ConverterContext;

		public Dictionary<UnityEngine.Object, UnityEngine.Object> _ConvertedResources = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		public Dictionary<UnityEngine.Object, UnityEngine.Object> ConvertedResources => _ConvertedResources;

		public GameObject _Root;
		public GameObject Root => _Root;

		public string TargetPath => StorageContext.TargetPath;
		public string _TargetApplication;
		public string TargetApplication => TargetApplication;

		public List<Task> _Tasks = new List<Task>();
		public List<UnityEngine.Object> _Trash = new List<UnityEngine.Object>();

		public List<UnityEngine.Object> _RegisteredResources = new List<UnityEngine.Object>();
		public List<UnityEngine.Object> RegisteredResources => _RegisteredResources;

		public Dictionary<UnityEngine.Object, UnityEngine.Object> _RegisteredResourcesContext = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		public Dictionary<UnityEngine.Object, UnityEngine.Object> RegisteredResourcesContext => _RegisteredResourcesContext;

		public STFApplicationConvertState(ISTFApplicationConvertStorageContext StorageContext, STFApplicationConverterContext ConverterContext, GameObject Root, string Target, List<string> ValidTargets, List<Type> ConversibleTypes)
		{
			this.StorageContext = StorageContext;
			_ConverterContext = ConverterContext;
			_Root = Root;
			_TargetApplication = Target;
			_RelMat = new STFRelationshipMatrix(Root, ValidTargets, ConversibleTypes);
		}

		public void AddTask(Task Task)
		{
			_Tasks.Add(Task);
		}

		public void AddTrash(UnityEngine.Object Trash)
		{
			_Trash.Add(Trash);
		}

		public void RegisterResource(UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			Resource = StorageContext.DuplicateResource(Resource);
			_RegisteredResources.Add(Resource);
			if(Context != null) _RegisteredResourcesContext.Add(Resource, Context);
		}

		public void SaveConvertedResource(UnityEngine.Object OriginalResource, UnityEngine.Object ConvertedResource, string FileExtension)
		{
			SaveGeneratedResource(ConvertedResource, FileExtension);
			_ConvertedResources.Add(OriginalResource, ConvertedResource);
		}

		public void SaveGeneratedResource(UnityEngine.Object Resource, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			StorageContext.SaveGeneratedResource(Resource, FileExtension);
		}

		public void RunTasks()
		{
			Utils.RunTasks(_Tasks);
		}

		public void DeleteTrash()
		{
			foreach(var trashObject in _Trash)
			{
				if(trashObject != null)
				{
					UnityEngine.Object.DestroyImmediate(trashObject);
				}
			}
		}
	}
}
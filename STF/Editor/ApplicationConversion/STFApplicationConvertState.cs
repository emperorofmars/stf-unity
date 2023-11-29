using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using STF.Util;
using UnityEditor;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public class STFApplicationConvertState : ISTFApplicationConvertStateInternal
	{
		public STFRelationshipMatrix _RelMat;
		public STFRelationshipMatrix RelMat => _RelMat;

		public Dictionary<Object, Object> _ConvertedResources;
		public Dictionary<Object, Object> ConvertedResources => _ConvertedResources;

		public GameObject _Root;
		public GameObject Root => _Root;

		public string _TargetPath;
		public string TargetPath => _TargetPath;
		public string _TargetApplication;
		public string TargetApplication => TargetApplication;

		public List<Task> _Tasks = new List<Task>();
		public List<Object> _Trash = new List<Object>();

		public void AddTask(Task Task)
		{
			_Tasks.Add(Task);
		}

		public void AddTrash(Object Trash)
		{
			_Trash.Add(Trash);
		}

		public void SaveConvertedResource(Object OriginalResource, Object ConvertedResource, string FileExtension)
		{
			SaveGeneratedResource(ConvertedResource, FileExtension);
			_ConvertedResources.Add(OriginalResource, ConvertedResource);
		}

		public void SaveGeneratedResource(Object Resource, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			AssetDatabase.CreateAsset(Resource, Path.Combine(TargetPath, TargetApplication, Resource.name + FileExtension));
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
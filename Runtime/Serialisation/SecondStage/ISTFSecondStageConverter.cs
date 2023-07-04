
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace stf.serialisation
{
	public interface ISTFSecondStageContext
	{
		STFRelationshipMatrix RelMat {get;}
		void AddTask(Task task);
		UnityEngine.Object GetConvertedResource(UnityEngine.Object resource);
	}

	public class STFSecondStageContext : ISTFSecondStageContext
	{
		private STFRelationshipMatrix _RelMat;
		public STFRelationshipMatrix RelMat => _RelMat;
		private List<Task> Tasks = new List<Task>();
		private Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceConversions = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

		public STFSecondStageContext(STFRelationshipMatrix relMat)
		{
			_RelMat = relMat;
		}

		public STFSecondStageContext(GameObject root, List<string> targets, List<Type> conversibleTypes)
		{
			_RelMat = new STFRelationshipMatrix(root, targets, conversibleTypes);
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public UnityEngine.Object GetConvertedResource(UnityEngine.Object resource)
		{
			return resource != null && ResourceConversions.ContainsKey(resource) ? ResourceConversions[resource] : resource;
		}

		public void RunTasks(int maxIterationDepth = 100)
		{
			int iteration = 0;
			do
			{
				var currentTasks = Tasks;
				Tasks = new List<Task>();
				foreach(var task in currentTasks)
				{
					task.RunSynchronously();
					if(task.Exception != null) throw task.Exception;
				}
				iteration++;
			}
			while(Tasks.Count > 0 && iteration < maxIterationDepth);
		}
	}

	public interface ISTFSecondStageConverter
	{
		void convert(Component component, GameObject root, List<UnityEngine.Object> resources, ISTFSecondStageContext context);
	}
}

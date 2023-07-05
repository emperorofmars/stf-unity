
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
		void AddConvertedResource(UnityEngine.Object originalResource, UnityEngine.Object convertedResource);
		UnityEngine.Object GetConvertedResource(GameObject root, UnityEngine.Object resource, STFSecondStageContext context);
	}

	public class STFSecondStageContext : ISTFSecondStageContext
	{
		private STFRelationshipMatrix _RelMat;
		public STFRelationshipMatrix RelMat => _RelMat;
		private List<Task> Tasks = new List<Task>();
		Dictionary<Type, ISTFSecondStageResourceProcessor> ResourceProcessors = new Dictionary<Type, ISTFSecondStageResourceProcessor>();
		public Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceConversions = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

		public STFSecondStageContext(STFRelationshipMatrix relMat, Dictionary<Type, ISTFSecondStageResourceProcessor> resourceProcessors)
		{
			_RelMat = relMat;
			ResourceProcessors = resourceProcessors;
		}

		public STFSecondStageContext(GameObject root, List<string> targets, List<Type> conversibleTypes, Dictionary<Type, ISTFSecondStageResourceProcessor> resourceProcessors)
		{
			_RelMat = new STFRelationshipMatrix(root, targets, conversibleTypes);
			ResourceProcessors = resourceProcessors;
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public void AddConvertedResource(UnityEngine.Object originalResource, UnityEngine.Object convertedResource)
		{
			lock(originalResource)
			{
				ResourceConversions.Add(originalResource, convertedResource);
			}
		}

		public UnityEngine.Object GetConvertedResource(GameObject root, UnityEngine.Object resource, STFSecondStageContext context)
		{
			if(ResourceProcessors.ContainsKey(resource.GetType()))
			{
				lock(resource)
				{
					return ResourceProcessors[resource.GetType()].convert(root, resource, context);
				}
			}
			else
			{
				return resource;
			}
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

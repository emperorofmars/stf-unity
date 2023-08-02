
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace stf.serialisation
{
	// The interface for second-stage-importers. They convert the STF-Unity intermediary scene into an application specific one.
	// The result of a second-stage is not back-convertable, as things will be liberally thrown away, component relationships resolved, and potential optimizations applied.

	public interface ISTFSecondStageContext
	{
		STFRelationshipMatrix RelMat {get;}
		void AddTask(Task task);
		void AddOriginalResource(string id, UnityEngine.Object resource);
		UnityEngine.Object GetOriginalResource(string id);
		void AddResource(UnityEngine.Object resource);
		void AddConvertedResource(UnityEngine.Object originalResource, UnityEngine.Object convertedResource);
		UnityEngine.Object GetConvertedResource(GameObject root, UnityEngine.Object resource);
		string GetPathForResourcesThatMustExistInFS();
	}

	public class STFSecondStageContext : ISTFSecondStageContext
	{
		private STFRelationshipMatrix _RelMat;
		public STFRelationshipMatrix RelMat => _RelMat;
		private List<Task> Tasks = new List<Task>();
		Dictionary<string, UnityEngine.Object> OriginalResources = new Dictionary<string, UnityEngine.Object>();
		public List<UnityEngine.Object> Resources = new List<UnityEngine.Object>();
		Dictionary<Type, ISTFSecondStageResourceProcessor> ResourceProcessors = new Dictionary<Type, ISTFSecondStageResourceProcessor>();
		public Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceConversions = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		public string PathForResourcesThatMustExistInFS;

		public STFSecondStageContext(STFRelationshipMatrix relMat, Dictionary<Type, ISTFSecondStageResourceProcessor> resourceProcessors, string pathForResourcesThatMustExistInFS)
		{
			_RelMat = relMat;
			ResourceProcessors = resourceProcessors;
			PathForResourcesThatMustExistInFS = pathForResourcesThatMustExistInFS;
		}

		public STFSecondStageContext(GameObject root, List<string> targets, List<Type> conversibleTypes, Dictionary<Type, ISTFSecondStageResourceProcessor> resourceProcessors, string pathForResourcesThatMustExistInFS)
		{
			_RelMat = new STFRelationshipMatrix(root, targets, conversibleTypes);
			ResourceProcessors = resourceProcessors;
			PathForResourcesThatMustExistInFS = pathForResourcesThatMustExistInFS;
		}

		public STFSecondStageContext(STFRelationshipMatrix relMat, Dictionary<Type, ISTFSecondStageResourceProcessor> resourceProcessors)
		{
			_RelMat = relMat;
			ResourceProcessors = resourceProcessors;
			PathForResourcesThatMustExistInFS = null;
		}

		public STFSecondStageContext(GameObject root, List<string> targets, List<Type> conversibleTypes, Dictionary<Type, ISTFSecondStageResourceProcessor> resourceProcessors)
		{
			_RelMat = new STFRelationshipMatrix(root, targets, conversibleTypes);
			ResourceProcessors = resourceProcessors;
			PathForResourcesThatMustExistInFS = null;
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}
		
		public void AddResource(UnityEngine.Object resource)
		{
			if(!Resources.Contains(resource)) Resources.Add(resource);
		}

		public void AddConvertedResource(UnityEngine.Object originalResource, UnityEngine.Object convertedResource)
		{
			lock(ResourceConversions)
			{
				ResourceConversions.Add(originalResource, convertedResource);
			}
		}

		public UnityEngine.Object GetConvertedResource(GameObject root, UnityEngine.Object resource)
		{
			lock(ResourceConversions)
			{
				if(ResourceConversions.ContainsKey(resource))
				{
					return ResourceConversions[resource];
				}
				else if(ResourceProcessors.ContainsKey(resource.GetType()))
				{
					var converted = ResourceProcessors[resource.GetType()].Convert(root, resource, this);
					ResourceConversions.Add(resource, converted);
					return converted;
				}
				else
				{
					return resource;
				}
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
		
		public string GetPathForResourcesThatMustExistInFS()
		{
			return PathForResourcesThatMustExistInFS;
		}

		public void AddOriginalResource(string id, UnityEngine.Object resource)
		{
			if(!OriginalResources.ContainsKey(id)) OriginalResources.Add(id, resource);
		}

		public UnityEngine.Object GetOriginalResource(string id)
		{
			return OriginalResources.ContainsKey(id) ? OriginalResources[id] : null;
		}
	}

	public interface ISTFSecondStageConverter
	{
		Dictionary<string, UnityEngine.Object> CollectOriginalResources(Component component, GameObject root, ISTFSecondStageContext context);
		void Convert(Component component, GameObject root, ISTFSecondStageContext context);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using STF.Serialisation;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFApplicationConverter
	{
		bool CanConvert(ISTFAsset Asset);
		GameObject Convert(ISTFApplicationConvertStorageContext StorageContext, ISTFAsset Asset);
	}
	

	public abstract class ASTFApplicationConverter : ISTFApplicationConverter
	{
		public abstract STFApplicationConverterContext ConverterContext {get;}
		// node converters ?
		// resource component converters ???
		
		public abstract List<Type> WhitelistedComponents {get;}
		public abstract string TargetName {get;}
		public abstract List<string> Targets {get;}

		public abstract bool CanConvert(ISTFAsset Asset);

		public GameObject Convert(ISTFApplicationConvertStorageContext StorageContext, ISTFAsset Asset)
		{
			GameObject ret = null;
			STFApplicationConvertState state = null;
			try
			{
				ret = UnityEngine.Object.Instantiate(Asset.gameObject);
				ret.name = Asset.gameObject.name + "_" + TargetName;

				state = new STFApplicationConvertState(StorageContext, ConverterContext, ret, TargetName, Targets, ConverterContext.NodeComponent.Keys.ToList());

				// gather and convert resources
				foreach(var component in ret.GetComponentsInChildren<Component>())
				{
					if(state.RelMat.IsMatched(component) && ConverterContext.NodeComponent.ContainsKey(component.GetType()))
					{
						ConverterContext.NodeComponent[component.GetType()].ConvertResources(state, component);
					}
				}
				state.RunTasks();
				state.SaveEverything();

				foreach(var resource in state.RegisteredResources)
				{
					if(ConverterContext.Resource.ContainsKey(resource.GetType()))
					{
						ConverterContext.Resource[resource.GetType()].Convert(state, resource);
					}
				}
				state.RunTasks();
				state.SaveEverything();

				// convert node components
				foreach(var component in ret.GetComponentsInChildren<Component>())
				{
					if(state.RelMat.IsMatched(component) && ConverterContext.NodeComponent.ContainsKey(component.GetType()))
					{
						ConverterContext.NodeComponent[component.GetType()].Convert(state, component);
					}
				}
				state.RunTasks();
				state.SaveEverything();

				// cleanup
				foreach(var component in ret.GetComponentsInChildren<Component>())
				{
					if(!WhitelistedComponents.Contains(component.GetType()))
					{
						#if UNITY_EDITOR
							UnityEngine.Object.DestroyImmediate(component);
						#else
							UnityEngine.Object.Destroy(component);
						#endif
					}
				}
				StorageContext.SavePrefab(ret);
				return ret;
			}
			catch(Exception e)
			{
				// TODO: delete generated stuff
				throw new Exception($"Error during STF conversion to application: {TargetName}", e);
			}
			finally
			{
				state?.DeleteTrash();
			}
		}
	}
}
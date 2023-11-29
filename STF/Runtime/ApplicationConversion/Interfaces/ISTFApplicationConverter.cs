using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STF.IdComponents;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public interface ISTFApplicationConverter
	{
		bool CanConvert(STFAsset Asset);
		GameObject Convert(ISTFApplicationConvertStorageContext StorageContext, STFAsset Asset);
	}
	

	public abstract class ASTFApplicationConverter : ISTFApplicationConverter
	{
		//protected abstract Dictionary<Type, ISTFSecondStageResourceProcessor> ResourceProcessors {get;}
		public abstract Dictionary<Type, ISTFNodeComponentApplicationConverter> Converters {get;}
		public abstract List<Type> WhitelistedComponents {get;}
		public abstract string TargetName {get;}
		public abstract List<string> Targets {get;}

		public abstract bool CanConvert(STFAsset Asset);

		public GameObject Convert(ISTFApplicationConvertStorageContext StorageContext, STFAsset Asset)
		{
			GameObject ret = null;
			STFApplicationConvertState state = null;
			try
			{
				ret = UnityEngine.Object.Instantiate(Asset.gameObject);
				ret.name = Asset.gameObject.name + "_" + TargetName;

				state = new STFApplicationConvertState(StorageContext, ret, TargetName, Targets, Converters.Keys.ToList());

				foreach(var component in ret.GetComponentsInChildren<Component>())
				{
					if(Converters.ContainsKey(component.GetType()))
					{
						Converters[component.GetType()].Convert(state, component);
					}
				}
				state.RunTasks();

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
				return ret;
			}
			catch(Exception e)
			{
				throw new Exception("Error during STF conversion: ", e);
			}
			finally
			{
				state?.DeleteTrash();
			}
		}
	}
}
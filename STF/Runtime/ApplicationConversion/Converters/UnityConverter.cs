using System;
using System.Collections.Generic;
using System.Linq;
using STF.IdComponents;
using STF.Serialisation;
using STF.Util;
using UnityEngine;
using UnityEngine.Animations;

namespace STF.ApplicationConversion
{
	public class STFUnityConverter : ISTFApplicationConverter
	{
		public string TargetName => "unity3d";

		private List<Type> WhitelistedComponents => new List<Type> {
			typeof(Transform), typeof(Animator), typeof(RotationConstraint), typeof(SkinnedMeshRenderer)
		};

		private Dictionary<Type, ISTFNodeComponentApplicationConverter> Converters => new Dictionary<Type, ISTFNodeComponentApplicationConverter>() {
			{typeof(STFTwistConstraint), new STFTwistConstraintConverter()},
		};

		public bool CanConvert(STFAsset Asset)
		{
			return Asset.assetInfo?.assetType == STFAssetImporter._TYPE;
		}

		public GameObject Convert(ISTFApplicationConvertStateInternal State, STFAsset Asset)
		{
			GameObject ret = null;
			try
			{
				ret = UnityEngine.Object.Instantiate(Asset.gameObject);
				ret.name = Asset.gameObject.name;

				foreach(var component in ret.GetComponentsInChildren<Component>())
				{
					if(Converters.ContainsKey(component.GetType()))
					{
						Converters[component.GetType()].Convert(State, component);
					}
				}
				State.RunTasks();

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
				State.DeleteTrash();
			}
		}
	}
}
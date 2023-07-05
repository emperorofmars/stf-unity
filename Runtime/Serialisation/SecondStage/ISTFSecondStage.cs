
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace stf.serialisation
{
	public class SecondStageResult
	{
		public List<ISTFAsset> assets;
		public List<UnityEngine.Object> resources;
	}

	public interface ISTFSecondStage
	{
		bool CanHandle(ISTFAsset asset, UnityEngine.Object adaptedUnityAsset);
		SecondStageResult Convert(ISTFAsset asset, UnityEngine.Object adaptedUnityAsset);
	}

	public abstract class ASTFSecondStageDefault : ISTFSecondStage
	{
		protected abstract Dictionary<Type, ISTFSecondStageResourceProcessor> ResourceProcessors {get;}
		protected abstract Dictionary<Type, ISTFSecondStageConverter> Converters {get;}
		protected abstract List<Type> WhitelistedComponents {get;}
		protected abstract string GameObjectSuffix {get;}
		protected abstract string StageName {get;}
		protected abstract string AssetTypeName {get;}
		protected abstract List<string> Targets {get;}

		public abstract bool CanHandle(ISTFAsset asset, UnityEngine.Object adaptedUnityAsset);

		public SecondStageResult Convert(ISTFAsset asset, UnityEngine.Object adaptedUnityAsset)
		{
			var originalRoot = (GameObject)adaptedUnityAsset;
			var convertedResources = new List<UnityEngine.Object>();

			GameObject convertedRoot = UnityEngine.Object.Instantiate(originalRoot);
			convertedRoot.name = originalRoot.name + "_" + GameObjectSuffix;
			try
			{
				var context = new STFSecondStageContext(convertedRoot, Targets, new List<Type>(Converters.Keys), ResourceProcessors);
				convertTree(convertedRoot, convertedResources, context);
				context.RunTasks();
				if(context.ResourceConversions.Count > 0) convertedResources.AddRange(context.ResourceConversions.Values);
				cleanup(convertedRoot);
			}
			catch(Exception e)
			{
				#if UNITY_EDITOR
					UnityEngine.Object.DestroyImmediate(convertedRoot);
				#else
					UnityEngine.Object.Destroy(convertedRoot);
				#endif
				throw new Exception("Error during AVA " + StageName + " Loader import: ", e);
			}

			var secondStageAsset = new STFSecondStageAsset(convertedRoot, asset.getId() + "_" + GameObjectSuffix, asset.GetSTFAssetName(), AssetTypeName);
			return new SecondStageResult {assets = new List<ISTFAsset>{secondStageAsset}, resources = convertedResources};
		}

		protected void convertTree(GameObject root, List<UnityEngine.Object> resources, STFSecondStageContext context)
		{
			foreach(var converter in Converters)
			{
				var components = root.GetComponentsInChildren(converter.Key);
				foreach(var component in components)
				{
					if(context.RelMat.IsMatched(component))
						converter.Value.convert(component, root, resources, context);
				}
			}
		}

		protected void cleanup(GameObject root)
		{
			foreach(var component in root.GetComponentsInChildren<Component>())
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
		}
	}
}
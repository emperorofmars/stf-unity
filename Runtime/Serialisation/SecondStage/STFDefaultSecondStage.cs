
using System;
using System.Collections.Generic;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	public class STFDefaultSecondStage : ISTFSecondStage
	{
		private Dictionary<Type, ISTFSecondStageConverter> converters = new Dictionary<Type, ISTFSecondStageConverter>() {
			{typeof(STFTwistConstraintBack), new STFTwistConstraintBackConverter()},
			{typeof(STFTwistConstraintForward), new STFTwistConstraintForwardConverter()}
		};
		
		public bool CanHandle(ISTFAsset asset)
		{
			return asset.GetSTFAssetType() == "asset";
		}

		public SecondStageResult Convert(ISTFAsset asset)
		{
			var originalRoot = (GameObject)asset.GetAsset();
			var resources = new List<UnityEngine.Object>();

			GameObject convertedRoot = UnityEngine.Object.Instantiate(originalRoot);
			convertedRoot.name = originalRoot.name + "_Default";

			try
			{
				var context = new STFSecondStageContext {RelMat = new STFRelationshipMatrix(convertedRoot, "")};
				convertTree(convertedRoot, resources, context);
			}
			catch(Exception e)
			{
				#if UNITY_EDITOR
					UnityEngine.Object.DestroyImmediate(convertedRoot);
				#else
					UnityEngine.Object.Destroy(convertedRoot);
				#endif
				throw new Exception("Error during STF Default Loader import: ", e);
			}

			var secondStageAsset = new STFSecondStageAsset(convertedRoot, asset.getId() + "_Default", asset.GetSTFAssetName());
			return new SecondStageResult {assets = new List<ISTFAsset>{secondStageAsset}, resources = new List<UnityEngine.Object>{}};
		}

		private void convertTree(GameObject root, List<UnityEngine.Object> resources, STFSecondStageContext context)
		{
			foreach(var converter in converters)
			{
				var components = root.GetComponentsInChildren(converter.Key);
				foreach(var component in components)
				{
					converter.Value.convert(component, root, resources, context);
				}
			}
		}
	}
}
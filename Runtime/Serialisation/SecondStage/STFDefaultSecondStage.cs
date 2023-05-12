
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	public class STFDefaultSecondStage : ISTFSecondStage
	{
		private Dictionary<Type, ISTFSecondStageConverter> converters = new Dictionary<Type, ISTFSecondStageConverter>() {{typeof(STFTwistConstraintBack), new STFTwistConstraintBackConverter()}};
		
		public bool CanHandle(ISTFAsset asset)
		{
			return asset.GetSTFAssetType() == "asset";
		}

		public SecondStageResult Convert(ISTFAsset asset)
		{
			var originalRoot = (GameObject)asset.GetAsset();

			// actually convert this
			GameObject convertedRoot = UnityEngine.Object.Instantiate(originalRoot);
			convertedRoot.name = originalRoot.name;

			convertTree(convertedRoot);

			var secondStageAsset = new STFSecondStageAsset(convertedRoot, asset.getId() + "_sub", asset.GetSTFAssetName());
			//this.assets.Add(secondStageAsset);
			return new SecondStageResult {assets = new List<ISTFAsset>{secondStageAsset}, resources = new List<UnityEngine.Object>{}};
		}

		private void convertTree(GameObject root)
		{
			foreach(var converter in converters)
			{
				var components = root.GetComponentsInChildren(converter.Key);
				foreach(var component in components)
				{
					converter.Value.convert(component, root);
				}
			}
		}

		/*private List<Task> tasks = new List<Task>();
		string mainAssetId;
		List<UnityEngine.Object> originalResources;
		List<ISTFAsset> assets = new List<ISTFAsset>();
		List<UnityEngine.Object> resources = new List<UnityEngine.Object>();

		public void convert(ISTFAsset asset)
		{
			this.tasks = new List<Task>();
			this.assets =  new List<ISTFAsset>();
			this.originalResources = resources;
			this.resources = new List<UnityEngine.Object>();
			run(asset);
		}
		
		public void AddTask(Task task)
		{
			tasks.Add(task);
		}

		public string GetMainAssetId()
		{
			return mainAssetId;
		}

		public List<ISTFAsset> GetAssets()
		{
			return assets;
		}

		public List<UnityEngine.Object> GetResources()
		{
			return resources;
		}

		private void run(ISTFAsset asset)
		{
			if(asset.GetSTFAssetType() == "asset")
			{
				var originalRoot = (GameObject)asset.GetAsset();

				// actually convert this
				GameObject convertedRoot = UnityEngine.Object.Instantiate(originalRoot);
				convertedRoot.name = originalRoot.name;

				convertTree(convertedRoot);

				var secondStageAsset = new STFSecondStageAsset(convertedRoot, asset.getId() + "_sub", asset.GetSTFAssetName());
				this.assets.Add(secondStageAsset);
			}
			else
			{
				Debug.LogWarning($"Could not convert asset {asset.GetSTFAssetName()} of type {asset.GetSTFAssetType()}");
			}
		}*/
	}
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using stf.Components;
using UnityEngine;

namespace stf.serialisation
{
	public class STFDefaultSecondStage : ISTFSecondStage
	{
		private List<Task> tasks = new List<Task>();
		string mainAssetId;
		Dictionary<string, ISTFAsset> originalAssets;
		List<UnityEngine.Object> originalResources;
		Dictionary<string, ISTFAsset> assets = new Dictionary<string, ISTFAsset>();
		List<UnityEngine.Object> resources = new List<UnityEngine.Object>();
		Dictionary<Type, ISTFSecondStageConverter> converters = new Dictionary<Type, ISTFSecondStageConverter>() {{typeof(STFTwistConstraintBack), new STFTwistConstraintBackConverter()}};

		public void init(ISTFImporter state)
		{
			this.tasks = new List<Task>();
			this.assets = new Dictionary<string, ISTFAsset>();
			this.mainAssetId = state.GetMainAssetId();
			this.originalAssets = state.GetAssets();
			this.originalResources = state.GetResources();
			this.resources = new List<UnityEngine.Object>();
			foreach(var asset in originalAssets)
			{
				run(asset.Value);
			}
		}

		public void init(string mainAssetId, Dictionary<string, ISTFAsset> assets, List<UnityEngine.Object> resources)
		{
			this.tasks = new List<Task>();
			this.assets = new Dictionary<string, ISTFAsset>();
			this.mainAssetId = mainAssetId;
			this.originalAssets = assets;
			this.originalResources = resources;
			this.resources = new List<UnityEngine.Object>();
			foreach(var asset in originalAssets)
			{
				run(asset.Value);
			}
		}
		
		public void AddTask(Task task)
		{
			tasks.Add(task);
		}

		public string GetMainAssetId()
		{
			return mainAssetId;
		}

		public Dictionary<string, ISTFAsset> GetAssets()
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

				var secondStageAsset = new STFSecondStageAsset(convertedRoot, asset.getId(), asset.GetSTFAssetName());
				assets.Add(asset.getId(), secondStageAsset);
			}
			else
			{
				Debug.LogWarning($"Could not convert asset {asset.GetSTFAssetName()} of type {asset.GetSTFAssetType()}");
			}
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
	}
}
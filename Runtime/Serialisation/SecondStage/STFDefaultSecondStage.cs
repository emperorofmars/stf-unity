
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
		STFMeta meta;
		private List<Task> tasks = new List<Task>();
		string mainAssetId;
		Dictionary<string, ISTFAsset> originalAssets;
		List<UnityEngine.Object> originalResources;
		Dictionary<string, List<ISTFAsset>> assets = new Dictionary<string, List<ISTFAsset>>();
		List<UnityEngine.Object> resources = new List<UnityEngine.Object>();
		Dictionary<Type, ISTFSecondStageConverter> converters = new Dictionary<Type, ISTFSecondStageConverter>() {{typeof(STFTwistConstraintBack), new STFTwistConstraintBackConverter()}};

		public void init(ISTFImporter state, STFMeta meta)
		{
			this.meta = meta;
			this.tasks = new List<Task>();
			this.assets = new Dictionary<string, List<ISTFAsset>>();
			this.mainAssetId = state.GetMainAssetId();
			this.originalAssets = state.GetAssets();
			this.originalResources = state.GetResources();
			this.resources = new List<UnityEngine.Object>();
			foreach(var asset in originalAssets)
			{
				run(asset.Value);
			}
		}

		public void init(string mainAssetId, Dictionary<string, ISTFAsset> assets, List<UnityEngine.Object> resources, STFMeta meta)
		{
			this.meta = meta;
			this.tasks = new List<Task>();
			this.assets = new Dictionary<string, List<ISTFAsset>>();
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

		public Dictionary<string, List<ISTFAsset>> GetAssets()
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
				if(!assets.ContainsKey(asset.getId())) assets.Add(asset.getId(), new List<ISTFAsset>() { secondStageAsset });
				else assets[asset.getId()].Add(secondStageAsset);

				var parent = meta.importedRawAssets.First(a => a.assetId == asset.getId());
				parent.secondStageAssets.Add(new STFMeta.AssetInfo { assetId = asset.getId() + "_sub", assetName = asset.GetSTFAssetName() + "_resolved", assetRoot = convertedRoot, assetType = "unity", visible = true});
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
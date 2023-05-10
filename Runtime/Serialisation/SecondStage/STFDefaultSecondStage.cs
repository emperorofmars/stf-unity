
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
		Dictionary<Type, ISTFSecondStageConverter> converters;

		public STFDefaultSecondStage(string mainAssetId, Dictionary<string, ISTFAsset> assets, List<UnityEngine.Object> resources)
		{
			this.mainAssetId = mainAssetId;
			this.originalAssets = assets;
			this.originalResources = resources;
			this.resources = new List<UnityEngine.Object>(originalResources);
			foreach(var asset in assets)
			{
				run(asset.Value);
			}
		}

		public void init(ISTFImporter state)
		{
			this.mainAssetId = state.GetMainAssetId();
			this.originalAssets = state.GetAssets();
			this.originalResources = state.GetResources();
		}
		
		public void AddTask(Task task)
		{
			tasks.Add(task);
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
				var root = (GameObject)asset.GetAsset();
			}
		}
	}
}
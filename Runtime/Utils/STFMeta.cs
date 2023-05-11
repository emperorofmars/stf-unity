
using System.Collections.Generic;
using UnityEngine;

namespace stf
{
	public class STFMeta : ScriptableObject
	{
		
		[System.Serializable]
		public class ResourceInfo
		{
			public UnityEngine.Object resource;
			public string id;
			public string name;
			public bool external;
			public UnityEngine.Object originalResource;
			public string originalFormat;
			public string originalExternalAssetPath;
		}
		
		[System.Serializable]
		public class AssetInfo
		{
			public string assetId;
			public string assetType;
			public string assetName;
			public UnityEngine.Object assetRoot;
			public bool visible;
			public List<AssetInfo> secondStageAssets = new List<AssetInfo>();
		}
		
		public string versionBinary;
		public string versionDefinition;
		public string copyright;
		public string generator;
		public string author;


		public string mainAssetId;
		public List<ResourceInfo> resourceInfo = new List<ResourceInfo>();
		public List<AssetInfo> importedRawAssets = new List<AssetInfo>();


	}
}

using System;
using System.Collections.Generic;
using stf.serialisation;
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
		public class Relationship
		{
			public string sourceId;
			public string type;
			public List<string> targetIds = new List<string>();
		}
		
		public string mainAsset;
		public List<ResourceInfo> resourceInfo = new List<ResourceInfo>();
		public List<ResourceInfo> relationships = new List<ResourceInfo>();


	}
}
